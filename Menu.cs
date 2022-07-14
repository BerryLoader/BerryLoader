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
		public RectTransform DumpScreen;

		public CustomButton menuButton;
		public CustomButton skipIntroButton;
		public CustomButton compactTooltipsButton;
		public CustomButton disablePauseTextButton;
		public CustomButton dumpScreenButton;

		public CustomButton dumpTexturesButton;
		public CustomButton dumpBoostersButton;
		public CustomButton dumpBlueprintsButton;

		public void Start()
		{
			BerryLoader.L.LogInfo("initing screen..");
			Transform berryText = MonoBehaviour.Instantiate(GameCanvas.instance.MainMenuScreen.GetChild(0).GetChild(0), GameCanvas.instance.MainMenuScreen.GetChild(0));
			berryText.SetSiblingIndex(1);
			berryText.GetComponent<TextMeshProUGUI>().text = "(+BerryLoader)";
			berryText.GetComponent<TextMeshProUGUI>().fontSize = 30;

			ModOptionsScreen = MenuAPI.CreateScreen("Mod Options", versionText: $"{BerryLoader.VERSION}-pre");
			DumpScreen = MenuAPI.CreateScreen("Dump Assets");

			var parent = ModOptionsScreen.GetChild(0).GetChild(1);

			menuButton = MenuAPI.CreateButton(GameCanvas.instance.MainMenuScreen.GetChild(0).GetChild(5), "Mod Options", ModOptionsScreen);
			menuButton.transform.SetSiblingIndex(5);

			skipIntroButton = MenuAPI.CreateConfigButton(parent, "Skip Intro", BerryLoader.configSkipIntro);

			compactTooltipsButton = MenuAPI.CreateConfigButton(parent, "Compact Tooltips", BerryLoader.configCompactTooltips);

			disablePauseTextButton = MenuAPI.CreateConfigButton(parent, "Disable \"Paused\" text", BerryLoader.configDisablePauseText);

			MenuAPI.CreateSpacer(parent);

			dumpScreenButton = MenuAPI.CreateButton(parent, "Dump Assets", DumpScreen);

			MenuAPI.CreateSpacer(parent);

			MenuAPI.CreateButton(parent, "Back", GameCanvas.instance.MainMenuScreen);

			var dp = DumpScreen.GetChild(0).GetChild(1);

			dumpTexturesButton = MenuAPI.CreateButton(dp, "Dump Textures", (() =>
			{
				ModalScreen.instance.Clear();
				ModalScreen.instance.SetTexts("Dump Textures", "<color=#ff003f>PLEASE DO NOT DISTRIBUTE ANY DUMPED ASSETS</color>\n\n<size=80%>Note: Your game will lag for a bit. Check logs for more information.</size>");
				ModalScreen.instance.AddOption("Dump", (() => { GameCanvas.instance.CloseModal(); BerryLoaderNS.Dumper.DumpTextures(); }));
				ModalScreen.instance.AddOption("Cancel", (() => { GameCanvas.instance.CloseModal(); }));
				GameCanvas.instance.OpenModal();
			}));

			dumpBoostersButton = MenuAPI.CreateButton(dp, "Dump Boosterpacks", (() => { Dumper.DumpBoosters(); }));

			dumpBlueprintsButton = MenuAPI.CreateButton(dp, "Dump Blueprints", (() => { Dumper.DumpBlueprints(); }));

			MenuAPI.CreateSpacer(dp);

			MenuAPI.CreateButton(dp, "Back", ModOptionsScreen);
		}
	}
}
