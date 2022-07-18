using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		[HarmonyPatch(typeof(GameDataLoader), "LoadModCards")]
		[HarmonyPrefix]
		static bool LoadCards(ref List<CardData> __result)
		{
			var injectables = new List<CardData>();
			var cards = ((IEnumerable<CardData>)Resources.LoadAll<CardData>("Cards")).ToList<CardData>();

			var wood = cards.Find(x => x.Id == "wood");
			var shed = cards.Find(x => x.Id == "blueprint_shed");

			BerryLoader.L.LogInfo("loading cards and blueprints..");

			foreach (var modDir in BerryLoader.modDirs)
			{
				// cant use continue here :(
				if (Directory.Exists(Path.Combine(modDir, "Cards")))
				{
					foreach (var file in new DirectoryInfo(Path.Combine(modDir, "Cards")).GetFiles())
					{
						var content = File.ReadAllText(Path.Combine(modDir, "Cards", file.Name));
						if (content == "") continue;
						ModCard modcard = JsonConvert.DeserializeObject<ModCard>(content);

						BerryLoader.L.LogInfo($"loading card: {modcard.id}");

						var inst = MonoBehaviour.Instantiate(wood.gameObject); // TODO: instantiate under some parent
						CardData card = inst.GetComponent<CardData>();
						card.Id = modcard.id;
						ModOverride mo = card.gameObject.AddComponent<ModOverride>();
						mo.Name = modcard.name;
						mo.Description = modcard.description;
						card.Value = modcard.value;
						if (modcard.audio != null)
						{
							card.PickupSoundGroup = PickupSoundGroup.Custom;
							WorldManager.instance.StartCoroutine(ResourceHelper.GetAudioClip(card.Id, Path.Combine(modDir, "Sounds", modcard.audio)));
						}
						var tex = new Texture2D(1024, 1024); // TODO: size?
						tex.LoadImage(File.ReadAllBytes(Path.Combine(modDir, "Images", modcard.icon)));
						card.Icon = Sprite.Create(tex, wood.Icon.rect, wood.Icon.pivot);
						card.MyCardType = EnumHelper.ToCardType(modcard.type);
						card.gameObject.SetActive(false);
						if (!modcard.cardDataScript.Equals(""))
						{
							var cardDataScript = BerryLoader.modTypes[modcard.cardDataScript];
							CardData component = (CardData)inst.AddComponent(cardDataScript);
							ReflectionHelper.CopyCardDataProps(component, card);
							MonoBehaviour.DestroyImmediate(card);
							component.gameObject.SetActive(false);
							injectables.Add(component);
						}
						else
							injectables.Add(card);
					}
				}

				if (!Directory.Exists(Path.Combine(modDir, "Blueprints"))) continue;
				foreach (var file in new DirectoryInfo(Path.Combine(modDir, "Blueprints")).GetFiles())
				{
					var content = File.ReadAllText(Path.Combine(modDir, "Blueprints", file.Name));
					if (content == "") continue;
					ModBlueprint modblueprint = JsonConvert.DeserializeObject<ModBlueprint>(content);

					BerryLoader.L.LogInfo($"loading blueprint: {modblueprint.id}");

					var bpinst = MonoBehaviour.Instantiate(shed); // TODO: instantiate under some parent
					var bp = bpinst.GetComponent<Blueprint>();
					bpinst.gameObject.SetActive(false);
					ModOverride mo = bpinst.gameObject.AddComponent<ModOverride>();
					mo.Name = modblueprint.name;
					bp.Id = modblueprint.id;
					var tex = new Texture2D(512, 512); // TODO: size?
					tex.LoadImage(File.ReadAllBytes(Path.Combine(modDir, "Images", modblueprint.icon)));
					bp.Icon = Sprite.Create(tex, wood.Icon.rect, wood.Icon.pivot);
					bp.BlueprintGroup = EnumHelper.ToBlueprintGroup(modblueprint.group);
					//bp.StackPostText = modblueprint.stackText; // gone?
					bp.Subprints = new List<Subprint>();
					foreach (ModSubprint ms in modblueprint.subprints)
					{
						var sp = new Subprint();
						sp.RequiredCards = ms.requiredCards.Split(',').Select(str => str.Trim()).ToArray();
						sp.CardsToRemove = ms.cardsToRemove.Split(',').Select(str => str.Trim()).ToArray();
						sp.ResultCard = ms.resultCard;
						sp.Time = ms.time;
						sp.StatusTerm = ms.status;
						sp.ExtraResultCards = ms.extraResultCards.Split(',').Select(str => str.Trim()).ToArray(); // this implementation could be wrong; needs more info
						bp.Subprints.Add(sp);
					}
					injectables.Add(bp);
				}
			}

			__result = injectables;
			return false;
		}

		[HarmonyPatch(typeof(GameDataLoader), MethodType.Constructor)]
		[HarmonyPostfix]
		static void LoadBoosters(GameDataLoader __instance)
		{
			BerryLoader.L.LogInfo(__instance.BoosterPackPrefabs.Count);
			BerryLoader.L.LogInfo("loading boosters..");

			var injectables = new List<Boosterpack>();
			var humble = __instance.BoosterPackPrefabs.Find(x => x.BoosterId == "basic");

			foreach (var modDir in BerryLoader.modDirs)
			{
				if (!Directory.Exists(Path.Combine(modDir, "Boosterpacks"))) continue;
				foreach (var file in new DirectoryInfo(Path.Combine(modDir, "Boosterpacks")).GetFiles())
				{
					var content = File.ReadAllText(Path.Combine(modDir, "Boosterpacks", file.Name));
					if (content == "") continue;
					ModBoosterpack modbooster = JsonConvert.DeserializeObject<ModBoosterpack>(content);

					BerryLoader.L.LogInfo($"loading boosterpack: {modbooster.id}");

					var bpinst = MonoBehaviour.Instantiate(humble.gameObject).GetComponent<Boosterpack>();
					/*
					so to explain the SetActive stuff: Start() gets called one frame after the object is intantiated *if* the object is active.
					when the pack gets created and Start() gets called in the menu (which is when everything gets loaded), the boosterpack gets destroyed
					for some reason. so we inject a neat little SetActive(true) in WM.CreateBoosterPack to essentially override this behavior
					*/
					/*
					as of the island update beta, the above explanation is outdated, i can no longer explain why the hell its necessary
					to make sure its inactive when instantiating it, but it works, and removing it breaks everything, so whatever
					*/
					bpinst.gameObject.SetActive(false);
					ModOverride mo = bpinst.gameObject.AddComponent<ModOverride>();
					mo.Name = modbooster.name;
					var tex = new Texture2D(1024, 1024); // TODO: size?
					tex.LoadImage(File.ReadAllBytes(Path.Combine(modDir, "Images", modbooster.icon)));
					bpinst.BoosterpackIcon = Sprite.Create(tex, humble.BoosterpackIcon.rect, humble.BoosterpackIcon.pivot);
					bpinst.BoosterId = modbooster.id;
					bpinst.MinAchievementCount = modbooster.minAchievementCount;
					bpinst.CardBags.Clear();
					foreach (var cb in modbooster.cardBags)
					{
						var cardbag = new CardBag();
						cardbag.CardBagType = EnumHelper.ToCardBagType(cb.type);
						cardbag.CardsInPack = cb.cards;
						cardbag.SetCardBag = EnumHelper.ToSetCardBag(cb.setCardBag);
						cardbag.SetPackCards = new List<string>(); // ???
						cardbag.Chances = new List<CardChance>();
						foreach (var chance in cb.chances)
						{
							var cc = new CardChance();
							cc.Id = chance.id;
							cc.Chance = chance.chance;
							cc.HasMaxCount = chance.hasMaxCount;
							cc.MaxCountToGive = chance.maxCountToGive;
							cc.PrerequisiteCardId = chance.Prerequisite;
							cardbag.Chances.Add(cc);
						}
						bpinst.CardBags.Add(cardbag);
					}

					injectables.Add(bpinst);
				}
			}

			__instance.BoosterPackPrefabs.AddRange(injectables);
			__instance.BoosterPackPrefabs = __instance.BoosterPackPrefabs.OrderBy<Boosterpack, int>((Func<Boosterpack, int>)(x => x.MinAchievementCount)).ToList<Boosterpack>();
		}
	}
}
