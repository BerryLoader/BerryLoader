using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		// TODO: implement translation system for the entire override system
		[HarmonyPatch(typeof(Boosterpack), "Name", MethodType.Getter)]
		[HarmonyPatch(typeof(CardData), "Name", MethodType.Getter)]
		[HarmonyPrefix]
		static bool NameOverride(CardData __instance, ref string __result)
		{
			var ov = __instance.GetComponent<ModOverride>();
			if (ov is not null && !string.IsNullOrEmpty(ov.Name))
			{
				__result = ov.Name;
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(CardData), "Description", MethodType.Getter)]
		[HarmonyPrefix]
		static bool DescriptionOverride(CardData __instance, ref string __result)
		{
			string descriptionOverride = (string)typeof(CardData).GetField("descriptionOverride", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
			if (!string.IsNullOrEmpty(descriptionOverride))
			{
				__result = descriptionOverride;
				return false;
			}
			var ov = __instance.GetComponent<ModOverride>();
			if (ov is not null && !string.IsNullOrEmpty(ov.Name))
			{
				__result = ov.Description;
				return false;
			}
			return true;
		}

		// this could be a transpiler but meh
		[HarmonyPatch(typeof(GameCard), "SetColors")]
		[HarmonyPrefix]
		static bool SetColorsOverride(GameCard __instance)
		{
			// why is there so much hardcoding in this function :((
			MaterialPropertyBlock propBlock = (MaterialPropertyBlock)typeof(GameCard).GetField("propBlock", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
			Color? color = ColorManager.instance.DefaultCard;
			Color? color2 = ColorManager.instance.DefaultCard2;
			Color? iconColor = ColorManager.instance.DefaultCardIcon;
			if (__instance.CardData.MyCardType == CardType.Rumors)
			{
				color = ColorManager.instance.RumorCard;
				color2 = ColorManager.instance.RumorCard2;
				iconColor = ColorManager.instance.RumorCardIcon;
			}
			else if (__instance.CardData.MyCardType == CardType.Locations)
			{
				color = ColorManager.instance.LocationCard;
				color2 = ColorManager.instance.LocationCard2;
				iconColor = ColorManager.instance.LocationCardIcon;
			}
			else if ((__instance.CardData is Mob mob && mob.IsAggressive) || __instance.CardData is StrangePortal || __instance.CardData is PirateBoat)
			{
				color = ColorManager.instance.AggresiveMobCard;
				color2 = ColorManager.instance.AggresiveMobCard2;
				iconColor = ColorManager.instance.AggresiveMobIcon;
			}
			else if (__instance.CardData.MyCardType == CardType.Fish)
			{
				color = ColorManager.instance.FishCard;
				color2 = ColorManager.instance.FishCard2;
				iconColor = ColorManager.instance.FishCardIcon;
			}
			else if (__instance.CardData.Id == "gold" || __instance.CardData.Id == "goblet" || __instance.CardData.Id == "key" || __instance.CardData.Id == "treasure_chest" || __instance.CardData.Id == "temple" || __instance.CardData.Id == "shell" || __instance.CardData.Id == "sacred_key" || __instance.CardData.Id == "sacred_chest" || __instance.CardData.Id == "island_relic" || __instance.CardData.Id == "compass")
			{
				color = ColorManager.instance.GoldCard;
				color2 = ColorManager.instance.GoldCard2;
				iconColor = ColorManager.instance.GoldCardIcon;
			}
			else if (__instance.CardData.Id == "corpse")
			{
				color = ColorManager.instance.CorpseCard;
				color2 = ColorManager.instance.CorpseCard2;
				iconColor = ColorManager.instance.CorpseCardIcon;
			}
			else if (__instance.CardData.MyCardType == CardType.Structures)
			{
				if (__instance.CardData.IsBuilding)
				{
					color = ColorManager.instance.BuildingCard;
					color2 = ColorManager.instance.BuildingCard2;
					iconColor = ColorManager.instance.BuildingCardIcon;
				}
				else
				{
					color = ColorManager.instance.StructureCard;
					color2 = ColorManager.instance.StructureCard2;
					iconColor = ColorManager.instance.StructureCardIcon;
				}
			}
			else if (__instance.CardData.MyCardType == CardType.Ideas)
			{
				color = ColorManager.instance.BlueprintCard;
				color2 = ColorManager.instance.BlueprintCard2;
				iconColor = ColorManager.instance.BlueprintCardIcon;
			}
			else if (__instance.CardData.MyCardType == CardType.Resources)
			{
				color = ColorManager.instance.ResourceCard;
				color2 = ColorManager.instance.ResourceCard2;
				iconColor = ColorManager.instance.ResourceCardIcon;
			}
			else if (__instance.CardData.MyCardType == CardType.Food)
			{
				color = ColorManager.instance.FoodCard;
				color2 = ColorManager.instance.FoodCard2;
				iconColor = ColorManager.instance.FoodCardIcon;
			}
			else if (__instance.CardData.MyCardType == CardType.Mobs)
			{
				color = ColorManager.instance.MobCard;
				color2 = ColorManager.instance.MobCard2;
				iconColor = ColorManager.instance.MobCardIcon;
			}
			var mo = __instance.CardData.GetComponent<ModOverride>();
			if (mo != null)
			{
				color = mo.Color ?? color;
				color2 = mo.Color2 ?? color2;
				iconColor = mo.IconColor ?? iconColor;
			}
			__instance.CombatStatusCircle.color = __instance.CombatCircleColor;
			__instance.CombatStatusIcon.color = Color.red;
			if (__instance.IsHit)
			{
				Color color4 = (__instance.CombatStatusCircle.color = (__instance.CombatStatusIcon.color = Color.white));
				color = (color2 = (iconColor = Color.white));
			}
			__instance.CardRenderer.GetPropertyBlock(propBlock, 2);
			propBlock.SetColor("_Color", (Color)color);
			propBlock.SetColor("_Color2", (Color)color2);
			propBlock.SetColor("_IconColor", (Color)iconColor);
			propBlock.SetFloat("_Foil", (__instance.CardData.IsFoil || __instance.CardData.Id == "gold" || __instance.CardData.Id == "goblet" || __instance.CardData.Id == "shell") ? 1f : 0f);
			if (__instance.IconRenderer.sprite != null)
			{
				propBlock.SetTexture("_IconTex", __instance.IconRenderer.sprite.texture);
			}
			else
			{
				propBlock.SetTexture("_IconTex", SpriteManager.instance.EmptyTexture.texture);
			}
			__instance.CardRenderer.SetPropertyBlock(propBlock, 2);
			__instance.SpecialText.color = (Color)color;
			__instance.SpecialIcon.color = (Color)iconColor;
			__instance.IconRenderer.color = (Color)iconColor;
			__instance.CoinText.color = (Color)color;
			__instance.CoinIcon.color = (Color)iconColor;
			__instance.CardNameText.color = (Color)iconColor;
			return false;
		}
	}
}