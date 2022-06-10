using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

namespace BerryLoaderNS
{
    [BepInPlugin("BerryLoader", "BerryLoader", "0.0.1")]
    [BepInProcess("Stacklands.exe")]
    public class BerryLoader : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource L;
        public static bool RenderBoosters = false;
        public static List<CardData> CardDataInjectables = new List<CardData>();
        public static List<ModBoosterpack> BoosterpackInjectables = new List<ModBoosterpack>();
        public static string berryDir;
        public static string modsDir;

        public static List<string> modDirs = new List<string>();
        public static List<ModManifest> modManifests = new List<ModManifest>();
        public static Dictionary<string, Type> modTypes = new Dictionary<string, Type>();
        public static Dictionary<string, Type> idToScript = new Dictionary<string, Type>();
        public static List<BerryLoaderMod> modClasses = new List<BerryLoaderMod>();

        public static GameCard tempCurrentGameCard;
        public static CardData tempCurrentCardData;

        public static RectTransform ModOptionsScreen = null;

        public static ConfigEntry<bool> configSkipIntro;
        public static ConfigEntry<bool> configCompactTooltips;
        public static ConfigEntry<bool> configDisablePauseText;

        private void Awake()
        {
            configSkipIntro = Config.Bind("Patches", "SkipIntro", false, "enable intro skip");
            configCompactTooltips = Config.Bind("Patches", "CompactTooltips", false, "enable compact tooltips");
            configDisablePauseText = Config.Bind("Patches", "DisablePauseText", false, "disable flashing pause text");

            L = Logger;
            L.LogInfo($"BerryLoader is loaded!");
            Harmony.CreateAndPatchAll(typeof(BerryLoader));
            Harmony.CreateAndPatchAll(typeof(Patches));
            DiscordAPI.Init();

            berryDir = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx/Plugins/BerryLoader");
            modsDir = Path.Combine(Directory.GetCurrentDirectory(), "mods");

            L.LogInfo("loading manifests..");
            foreach (var directory in new DirectoryInfo(modsDir).GetDirectories())
            {
                modDirs.Add(directory.ToString());
                var modid = directory.Name;
                modManifests.Add(JsonConvert.DeserializeObject<ModManifest>(File.ReadAllText(Path.Combine(modsDir, modid, "manifest.json"))));
            }

            // exceptions here: circular dep, not found, bad version
            modManifests = DependencyHelper.GetValidModLoadOrder(modManifests);

            L.LogInfo("loading assemblies..");
            foreach (var manifest in modManifests)
            {
                // Stacklands/mods/<id>/<id>.dll
                L.LogInfo($"loading {manifest.id}.dll..");
                var mod = Assembly.Load(File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "mods", manifest.id, $"{manifest.id}.dll")));
                foreach (var t in ReflectionHelper.GetSafeTypes(mod))
                {
                    BerryLoader.L.LogInfo(t.ToString());
                    if (t.BaseType == typeof(BerryLoaderMod))
                    {
                        L.LogInfo($"Found main class: {t.ToString()}");
                        BerryLoaderMod c = (BerryLoaderMod)Activator.CreateInstance(t);
                        c.manifest = manifest;
                        modClasses.Add(c);
                    }
                    modTypes[t.ToString()] = t;
                }
            }

            foreach (var mod in modClasses)
                mod.Init();
        }

        [HarmonyPatch(typeof(WorldManager), "Awake")]
        [HarmonyPrefix]
        static void Prefix(WorldManager __instance)
        {
            L.LogInfo("awaking");
            WorldManager.instance = __instance;
            BerryLoader.CardDataInjectables.Clear();
            __instance.CardDataPrefabs = ((IEnumerable<CardData>)Resources.LoadAll<CardData>("Cards")).ToList<CardData>();

            L.LogInfo($"carddataprefabs before injection: {((IEnumerable<CardData>)Resources.LoadAll<CardData>("Cards")).ToList<CardData>().Count}");

            foreach (var mod in modClasses)
                mod.PreInjection();

            // get a base object we can instantiate
            var wood = ((IEnumerable<CardData>)Resources.LoadAll<CardData>("Cards")).ToList<CardData>().Find(x => x.Id == "wood");
            var shed = ((IEnumerable<CardData>)Resources.LoadAll<CardData>("Cards")).ToList<CardData>().Find(x => x.Id == "blueprint_shed");
            L.LogInfo("loading cards and blueprints..");
            foreach (var modDir in modDirs)
            {
                foreach (var file in new DirectoryInfo(Path.Combine(modDir, "Cards")).GetFiles())
                {
                    var content = File.ReadAllText(Path.Combine(modDir, "Cards", file.Name));
                    ModCard modcard = JsonConvert.DeserializeObject<ModCard>(content);

                    if (!modcard.gameCardScript.Equals(""))
                        idToScript[modcard.id] = modTypes[modcard.gameCardScript];

                    var inst = MonoBehaviour.Instantiate(wood.gameObject);
                    CardData card = inst.GetComponent<CardData>();
                    card.StartCoroutine(ResourceHelper.GetAudioClip(card, modcard.audio));
                    card.Id = modcard.id;
                    ModOverride mo = card.gameObject.AddComponent<ModOverride>();
                    mo.Name = modcard.name;
                    mo.Description = modcard.description;
                    card.Value = modcard.value;
                    var tex = new Texture2D(1024, 1024); // TODO: size
                    tex.LoadImage(File.ReadAllBytes(Path.Combine(modDir, "Images", modcard.icon)));
                    card.Icon = Sprite.Create(tex, wood.Icon.rect, wood.Icon.pivot);
                    card.MyCardType = CardType.Resources; // TODO: uhhhh
                    card.MyGameCard = MonoBehaviour.Instantiate(__instance.GameCardPrefab);
                    card.MyGameCard.gameObject.SetActive(false); // deactivate it so Start() methods dont get called on next frame
                    card.gameObject.SetActive(false);
                    if (!modcard.cardDataScript.Equals(""))
                    {
                        inst.AddComponent(modTypes[modcard.cardDataScript]);
                        ReflectionHelper.CopyCardDataProps((CardData)inst.GetComponent(modTypes[modcard.cardDataScript]), card);
                        DestroyImmediate(card);
                        BerryLoader.CardDataInjectables.Add(inst.GetComponent<CardData>());
                        ((CardData)inst.GetComponent(modTypes[modcard.cardDataScript])).gameObject.SetActive(true);
                    }
                    else
                        BerryLoader.CardDataInjectables.Add(card);
                }

                foreach (var file in new DirectoryInfo(Path.Combine(modDir, "Blueprints")).GetFiles())
                {
                    var content = File.ReadAllText(Path.Combine(modDir, "Blueprints", file.Name));
                    ModBlueprint modblueprint = JsonConvert.DeserializeObject<ModBlueprint>(content);

                    var bpinst = MonoBehaviour.Instantiate(shed);
                    var bp = bpinst.GetComponent<Blueprint>();
                    bpinst.gameObject.SetActive(false);
                    ModOverride mo = bpinst.gameObject.AddComponent<ModOverride>();
                    mo.Name = modblueprint.name;
                    // mo.Description = modblueprint.description; // description doesnt exist in modblueprint (for now?)
                    bp.Id = modblueprint.id;
                    // texture! which is 512x for some reason???
                    bp.BlueprintGroup = BlueprintGroup.Cooking; // TODO: uhhhh
                    bp.StackPostText = modblueprint.stackText;
                    bp.Subprints = new List<Subprint>();
                    foreach (ModSubprint ms in modblueprint.subprints)
                    {
                        var sp = new Subprint();
                        sp.RequiredCards = ms.requiredCards.Split(',').Select(str => str.Trim()).ToArray();
                        sp.CardsToRemove = ms.cardsToRemove.Split(',').Select(str => str.Trim()).ToArray();
                        sp.ResultCard = ms.resultCard;
                        sp.Time = ms.time;
                        // sp.StatusTerm = ms.statusTerm; //??? oh translation; should have override
                        // sp.ExtraResultCards = ms.extraResultCards //?? check modsubprint
                        bp.Subprints.Add(sp);
                    }
                    BerryLoader.CardDataInjectables.Add(bp);
                }
            }
        }

        // wtf is qk
        [HarmonyPatch(typeof(WorldManager), "Awake")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> WMAwakeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var CardDataPrefabsField = AccessTools.Field(typeof(WorldManager), "CardDataPrefabs");
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stfld && (FieldInfo)instruction.operand == CardDataPrefabsField)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, CardDataPrefabsField);
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(BerryLoader), "CardDataInjectables"));
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(IEnumerable<CardData>));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<CardData>), "AddRange", new Type[] { typeof(IEnumerable<CardData>) }));
                }
            }
        }

        [HarmonyPatch(typeof(WorldManager), "Awake")]
        [HarmonyPostfix]
        static void WMAwakePost()
        {
            RenderBoosters = false;
            L.LogInfo($"carddataprefabs after injection: {WorldManager.instance.CardDataPrefabs.Count}");

            var humble = WorldManager.instance.BoosterPackPrefabs.Find(x => x.NameTerm == "pack_humble_beginnings_text");
            L.LogInfo("loading boosterpacks..");
            foreach (var modDir in modDirs)
            {
                foreach (var file in new DirectoryInfo(Path.Combine(modDir, "Boosterpacks")).GetFiles())
                {
                    var content = File.ReadAllText(Path.Combine(modDir, "Boosterpacks", file.Name));
                    ModBoosterpack modbooster = JsonConvert.DeserializeObject<ModBoosterpack>(content);

                    var bpinst = MonoBehaviour.Instantiate(humble.gameObject).GetComponent<Boosterpack>();
                    bpinst.gameObject.SetActive(false);
                    /*
					so to explain the SetActive stuff: Start() gets called one frame after the object is intantiated *if* the object is active.
					when the pack gets created and Start() gets called in the menu (which is when everything gets loaded), the boosterpack gets destroyed
					for some reason. so we inject a neat little SetActive(true) in WM.CreateBoosterPack to essentially override this behavior
					*/
                    ModOverride mo = bpinst.gameObject.AddComponent<ModOverride>();
                    mo.Name = modbooster.name;
                    bpinst.BoosterId = modbooster.id;
                    bpinst.MinAchievementCount = modbooster.minAchievementCount;
                    bpinst.CardBags.Clear();
                    foreach (var cb in modbooster.cardBags)
                    {
                        var cardbag = new CardBag();
                        cardbag.CardBagType = CardBagType.Chances; // TODO: enummy :(
                        cardbag.CardsInPack = cb.cards;
                        cardbag.SetCardBag = SetCardBag.BasicResources; // TODO: enummy :(
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

                    WorldManager.instance.BoosterPackPrefabs.Add(bpinst);
                    BerryLoader.BoosterpackInjectables.Add(modbooster);
                }
            }

            foreach (var mod in modClasses)
                mod.PostInjection();
        }

        [HarmonyPatch(typeof(WorldManager), "CreateBoosterpack")]
        [HarmonyPrefix]
        static bool CBP(WorldManager __instance, ref Boosterpack __result, Vector3 position, string boosterId)
        {
            Boosterpack boosterpack = UnityEngine.Object.Instantiate<Boosterpack>(__instance.GetBoosterPrefab(boosterId));
            boosterpack.transform.position = position;
            boosterpack.gameObject.SetActive(true); //bruh
            if (__instance.CurrentSaveGame.FoundBoosterIds.Contains(boosterId))
            {
                __result = boosterpack;
                return false;
            }
            L.LogInfo("not in currentsavegame wtf?");
            __instance.CurrentSaveGame.FoundBoosterIds.Add(boosterId);
            __result = boosterpack;
            return false;
        }

        [HarmonyPatch(typeof(WorldManager), "CreateCard", new Type[] { typeof(Vector3), typeof(CardData), typeof(bool), typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        static bool WMCC(WorldManager __instance, ref CardData __result, Vector3 position, CardData cardDataPrefab, bool faceUp, bool checkAddToStack = true, bool playSound = true)
        {
            if ((UnityEngine.Object)cardDataPrefab == (UnityEngine.Object)null)
                return (CardData)null;
            if (playSound)
                AudioManager.me.PlaySound2D(AudioManager.me.CardCreate, vol: 0.1f);
            GameCard newCard = UnityEngine.Object.Instantiate<GameCard>(__instance.GameCardPrefab);
            newCard.transform.position = position;
            CardData card = UnityEngine.Object.Instantiate<CardData>(cardDataPrefab);
            card.transform.SetParent(newCard.transform);
            card.transform.localPosition = Vector3.zero;
            card.UniqueId = Guid.NewGuid().ToString().Substring(0, 12);
            newCard.CardData = card;
            card.MyGameCard = newCard;
            newCard.gameObject.name = cardDataPrefab.gameObject.name;
            newCard.SetFaceUp(faceUp);
            if (checkAddToStack)
                __instance.CheckIfCanAddOnStack(newCard);
            newCard.transform.position = position;
            __instance.AllCards.Add(newCard);
            AchievementManager.instance.CardCreated(card);
            __instance.FoundCard(card);
            if (card.MyCardType == CardType.Ideas && (UnityEngine.Object)GameScreen.instance != (UnityEngine.Object)null)
                GameScreen.instance.UpdateIdeasLog();
            if (idToScript.ContainsKey(cardDataPrefab.Id))
            {
                tempCurrentGameCard = newCard;
                newCard.gameObject.AddComponent(idToScript[cardDataPrefab.Id]);
                Destroy(newCard);
                card.MyGameCard = (GameCard)newCard.gameObject.GetComponent(idToScript[cardDataPrefab.Id]);
                // copy props here?
                //__result = card;
                //return false;
            }
            __result = card;
            return false;
        }
    }
}
