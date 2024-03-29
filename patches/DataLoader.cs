using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
			var descriptionOverrideField = typeof(CardData).GetField("descriptionOverride", BindingFlags.Instance | BindingFlags.NonPublic);

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
						mo.Name = modcard.nameOverride;
						mo.Description = modcard.descriptionOverride;
						card.NameTerm = modcard.nameTerm;
						card.DescriptionTerm = modcard.descriptionTerm;
						card.Value = modcard.value;
						if (modcard.audio != null)
						{
							card.PickupSoundGroup = PickupSoundGroup.Custom;
							// the PickupSound is assigned in a WorldManager.CreateCard patch (patches/WMCreate.cs)
							// otherwise since this loading is technically async it can cause race conditions and its
							// probably just better to load (possibly large) audio files asynchronously
							WorldManager.instance.StartCoroutine(ResourceHelper.GetAudioClip(Path.Combine(modDir, "Sounds", modcard.audio), (AudioClip ac) =>
							{
								ResourceHelper.AudioClips.Add(card.Id, ac);
							}));
						}
						card.Icon = ResourceHelper.GetSprite(Path.Combine(modDir, "Images", modcard.icon));
						card.MyCardType = EnumHelper.ToCardType(modcard.type);
						card.gameObject.SetActive(false);
						if (!modcard.script.Equals(""))
						{
							if (!BerryLoader.modTypes.ContainsKey(modcard.script))
							{
								BerryLoader.L.LogError($"Could not find script {modcard.script}, the card will not be loaded");
								continue;
							}
							var cardDataScript = BerryLoader.modTypes[modcard.script];
							CardData component = (CardData)inst.AddComponent(cardDataScript);
							ReflectionHelper.CopyCardDataProps(component, card);
							if (modcard.ExtraProps != null)
							{
								foreach (KeyValuePair<string, JToken> entry in modcard.ExtraProps)
								{
									if (!entry.Key.StartsWith("_"))
										continue;
									var key = entry.Key.TrimStart('_');
									FieldInfo field = component.GetType().GetField(key);
									if (field != null)
									{
										if (BerryLoader.configVerboseLogging.Value)
											BerryLoader.L.LogInfo($"found Extraprop {key} ({field.FieldType}): {entry.Value.ToString()}");
										field.SetValue(component, entry.Value.ToObject(field.FieldType));
									}
									else
										BerryLoader.L.LogError($"Property {key} doesn't exist on {component.GetType()}");
								}
							}
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

					BerryLoader.L.LogInfo($"loading blueprint: {modblueprint.id} | {shed}");

					var bpinst = MonoBehaviour.Instantiate(shed); // TODO: instantiate under some parent
					var bp = bpinst.GetComponent<Blueprint>();
					bpinst.gameObject.SetActive(false);
					ModOverride mo = bpinst.gameObject.AddComponent<ModOverride>();
					mo.Name = modblueprint.nameOverride;
					bp.NameTerm = modblueprint.nameTerm;
					bp.Id = modblueprint.id;
					bp.Icon = ResourceHelper.GetSprite(Path.Combine(modDir, "Images", modblueprint.icon));
					bp.BlueprintGroup = EnumHelper.ToBlueprintGroup(modblueprint.group);
					bp.HideFromIdeasTab = modblueprint.hideFromIdeasTab;
					bp.HideFromCardopedia = modblueprint.hideFromCardopedia;
					bp.IsInvention = modblueprint.isInvention;
					bp.NeedsExactMatch = modblueprint.needsExactMatch;
					bp.Subprints = new List<Subprint>();
					for (int i = 0; i < modblueprint.subprints.Count; i++)
					{
						ModSubprint ms = modblueprint.subprints[i];
						var sp = new Subprint();
						sp.RequiredCards = ms.requiredCards.Split(',').Select(str => str.Trim()).Where(str => !string.IsNullOrEmpty(str)).ToArray();
						sp.CardsToRemove = ms.cardsToRemove?.Split(',').Select(str => str.Trim()).Where(str => !string.IsNullOrEmpty(str)).ToArray();
						sp.ResultCard = ms.resultCard;
						sp.Time = ms.time;
						sp.StatusTerm = ms.statusTerm;
						if (!string.IsNullOrEmpty(ms.statusOverride))
							mo.SubprintStatuses.Add(i, ms.statusOverride);
						sp.ExtraResultCards = ms.extraResultCards?.Split(',').Select(str => str.Trim()).Where(str => !string.IsNullOrEmpty(str)).ToArray(); // this implementation could be wrong; needs more info
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
					mo.Name = modbooster.nameOverride;
					bpinst.NameTerm = modbooster.nameTerm;
					bpinst.BoosterpackIcon = ResourceHelper.GetSprite(Path.Combine(modDir, "Images", modbooster.icon));
					bpinst.BoosterId = modbooster.id;
					bpinst.MinAchievementCount = modbooster.minAchievementCount;
					bpinst.Cost = modbooster.cost;
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

		[HarmonyPatch(typeof(WorldManager), "Awake")]
		[HarmonyPostfix]
		public static void ValidateData(WorldManager __instance)
		{
			if (BerryLoader.ValidateGameData)
			{
				BerryLoader.L.LogInfo("validating game data..");
				var validator = new GameDataValidator(__instance.GameDataLoader);
				validator.Check();
			}
		}
	}
}
