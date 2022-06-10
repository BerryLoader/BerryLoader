using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BerryLoaderNS
{
    public static class MenuAPI
    {
        public static CustomButton buttonPrefab;
        public static Transform spacerPrefab;

        public static List<Transform> Screens = new List<Transform>();

        public static void Init()
        {
            buttonPrefab = GameCanvas.instance.OptionsScreen.GetComponentInChildren<CustomButton>();
            spacerPrefab = GameCanvas.instance.MainMenuScreen.GetChild(0).GetChild(3);
        }

        public static RectTransform CreateScreen(string text)
        {
            var screen = MonoBehaviour.Instantiate(GameCanvas.instance.OptionsScreen, GameCanvas.instance.transform);
            screen.GetComponentInChildren<TextMeshProUGUI>().text = text;
            MonoBehaviour.Destroy(screen.transform.GetChild(0).GetChild(0).GetComponent<LocSetter>());
            foreach (Transform child in screen.GetChild(0).GetChild(1))
            {
                MonoBehaviour.Destroy(child.gameObject);
            }
            Screens.Add(screen);
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