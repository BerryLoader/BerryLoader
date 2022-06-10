using BepInEx;
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

/*
Screen (script : MonoBehaviour)
    Title (modify TextMeshProUGUI.text)
    Buttons
        Button
        Spacer
*/

namespace BerryLoaderNS
{
	public class CustomMenu : MonoBehaviour
	{
		public RectTransform ModOptionsScreen;
		public CustomButton buttonPrefab;
		public CustomButton backButtonPrefab;
		public Transform spacerPrefab;

		public CustomButton menuButton;
		public CustomButton skipIntroButton;
		public CustomButton compactTooltipsButton;

		public CustomButton dumpTexturesButton;

		public RectTransform TestScreen;

		public void Start()
		{
			BerryLoader.L.LogInfo("initing screen..");
			Transform berryText = MonoBehaviour.Instantiate(GameCanvas.instance.MainMenuScreen.GetChild(0).GetChild(0), GameCanvas.instance.MainMenuScreen.GetChild(0));
			berryText.SetSiblingIndex(1);
			berryText.GetComponent<TextMeshProUGUI>().text = "(+BerryLoader)";
			berryText.GetComponent<TextMeshProUGUI>().fontSize = 30;

			ModOptionsScreen = MenuAPI.CreateScreen("Mod Options");

			var parent = ModOptionsScreen.GetChild(0).GetChild(1);

			menuButton = MenuAPI.CreateButton(GameCanvas.instance.MainMenuScreen.GetChild(0).GetChild(5), "Mod Options", (() =>
			{
				GameCanvas.instance.SetScreen(ModOptionsScreen);
			}));
			menuButton.transform.SetSiblingIndex(6);

			skipIntroButton = MenuAPI.CreateConfigButton(parent, "Skip Intro", BerryLoader.configSkipIntro);

			compactTooltipsButton = MenuAPI.CreateConfigButton(parent, "Compact Tooltips", BerryLoader.configCompactTooltips);

			MenuAPI.CreateSpacer(parent);

			dumpTexturesButton = MenuAPI.CreateButton(parent, "Dump Textures", (() =>
			{
				// TODO: since updated text doesnt get rendered, this should use a modal
				dumpTexturesButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dumping textures..";
				BerryLoaderNS.Dumper.DumpTextures();
				dumpTexturesButton.GetComponentInChildren<TextMeshProUGUI>().text = "Dump textures";
			}));

			MenuAPI.CreateSpacer(parent);

			MenuAPI.CreateButton(parent, "Back", (() =>
			{
				GameCanvas.instance.SetScreen(GameCanvas.instance.MainMenuScreen);
			}));

			BerryLoader.ModOptionsScreen = ModOptionsScreen;

			TestScreen = MenuAPI.CreateScreen("Load Save");
			var p = TestScreen.GetChild(0).GetChild(1);
			MenuAPI.CreateButton(p, "Vanilla (Moon 14)", (() => { }));
			MenuAPI.CreateButton(p, "Modded Campaign (Moon 3)", (() => { }));
			MenuAPI.CreateButton(p, "IMPOSSIBLE MODE (Moon 50)", (() => { }));
			MenuAPI.CreateSpacer(p);
			MenuAPI.CreateButton(p, "Back", (() =>
			{
				GameCanvas.instance.SetScreen(GameCanvas.instance.MainMenuScreen);
			}));

			MenuAPI.CreateButton(parent, "Test", (() =>
			{
				GameCanvas.instance.SetScreen(TestScreen);
			}));
		}
	}
}
