using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerryLoaderNS
{
	public static class MenuAPI
	{
		public static CustomButton buttonPrefab;
		public static Transform spacerPrefab;

		static FieldInfo ScreensField;
		static FieldInfo ScreenPositionsField;
		static FieldInfo ScreenInTransitionField;

		public static void Init()
		{
			buttonPrefab = PrefabManager.instance.ButtonPrefab;
			spacerPrefab = GameCanvas.instance.MainMenuScreen.GetChild(0).GetChild(3);
			ScreensField = typeof(GameCanvas).GetField("screens", BindingFlags.Instance | BindingFlags.NonPublic);
			ScreenPositionsField = typeof(GameCanvas).GetField("screenPositions", BindingFlags.Instance | BindingFlags.NonPublic);
			ScreenInTransitionField = typeof(GameCanvas).GetField("screenInTransition", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static RectTransform CreateScreen(string text, string versionText = null)
		{
			var screen = MonoBehaviour.Instantiate(GameCanvas.instance.PauseScreen, GameCanvas.instance.transform);
			screen.GetComponentInChildren<TextMeshProUGUI>().text = text;
			MonoBehaviour.Destroy(screen.transform.GetChild(0).GetChild(0).GetComponent<LocSetter>());
			MonoBehaviour.Destroy(screen.transform.GetComponent<PauseScreen>());
			if (versionText != null)
			{
				var version = MonoBehaviour.Instantiate(GameCanvas.instance.OptionsScreen.GetChild(0).GetChild(0).GetChild(0), screen.GetChild(0).GetChild(0));
				version.GetComponent<TextMeshProUGUI>().text = versionText;
			}
			foreach (Transform child in screen.GetChild(0).GetChild(1))
			{
				MonoBehaviour.Destroy(child.gameObject);
			}
			List<RectTransform> screens = (List<RectTransform>)ScreensField.GetValue(GameCanvas.instance);
			screens.Add(screen);
			ScreensField.SetValue(GameCanvas.instance, screens);
			List<GameCanvas.ScreenPosition> screenPositions = (List<GameCanvas.ScreenPosition>)ScreenPositionsField.GetValue(GameCanvas.instance);
			screenPositions.Add(GameCanvas.ScreenPosition.Left);
			ScreenPositionsField.SetValue(GameCanvas.instance, screenPositions);
			List<bool> screenInTransition = (List<bool>)ScreenInTransitionField.GetValue(GameCanvas.instance);
			screenInTransition.Add(false);
			ScreenInTransitionField.SetValue(GameCanvas.instance, screenInTransition);
			return screen;
		}

		public static Transform CreateSpacer(Transform parent)
		{
			return MonoBehaviour.Instantiate(spacerPrefab, parent);
		}

		public static CustomButton CreateButton(Transform parent, string text, Action clicked)
		{
			var button = MonoBehaviour.Instantiate(buttonPrefab, parent);
			MonoBehaviour.Destroy(button.transform.GetChild(0).GetComponent<LocSetter>());
			button.GetComponentInChildren<TextMeshProUGUI>().text = text;
			button.Clicked += clicked;
			return button;
		}

		public static CustomButton CreateButton(Transform parent, string text, RectTransform screen)
		{
			var button = MonoBehaviour.Instantiate(buttonPrefab, parent);
			MonoBehaviour.Destroy(button.transform.GetChild(0).GetComponent<LocSetter>());
			button.GetComponentInChildren<TextMeshProUGUI>().text = text;
			button.Clicked += (() => { GameCanvas.instance.SetScreen(screen); });
			return button;
		}

		public static CustomButton CreateConfigButton(Transform parent, string text, ConfigEntry<bool> config)
		{
			CustomButton button = null; // init as null so its usable in callback
			button = CreateButton(parent, $"{text}: {BoolToString(config.Value)}", (() =>
			{
				config.Value = !config.Value;
				button.GetComponentInChildren<TextMeshProUGUI>().text = $"{text}: {BoolToString(config.Value)}";
			}));
			return button;
		}

		public static string BoolToString(bool v) => v ? "On" : "Off";
	}
}