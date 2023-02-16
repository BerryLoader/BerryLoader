using HarmonyLib;
using Shapes;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace BerryLoaderNS
{
	public static partial class Patches
	{
		static FieldInfo GameCardPropBlock = typeof(GameCard).GetField("propBlock", BindingFlags.Instance | BindingFlags.NonPublic);

		[HarmonyPatch(typeof(Boosterpack), "Name", MethodType.Getter)]
		[HarmonyPatch(typeof(CardData), "Name", MethodType.Getter)]
		[HarmonyPatch(typeof(Blueprint), "KnowledgeName", MethodType.Getter)]
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

		[HarmonyPatch(typeof(Subprint), "StatusName", MethodType.Getter)]
		[HarmonyPrefix]
		static bool StatusOverride(Subprint __instance, ref string __result)
		{
			var ov = __instance.ParentBlueprint.GetComponent<ModOverride>();
			if (ov is not null && ov.SubprintStatuses.ContainsKey(__instance.SubprintIndex))
			{
				__result = ov.SubprintStatuses[__instance.SubprintIndex];
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
			if (ov is not null && !string.IsNullOrEmpty(ov.Description))
			{
				__result = ov.Description;
				return false;
			}
			return true;
		}

		// this could be a transpiler but meh
		[HarmonyPatch(typeof(GameCard), "SetColors")]
		[HarmonyPrefix]
		static bool SetColorsOverride(GameCard __instance, MaterialPropertyBlock ___propBlock)
		{
			// why is there so much hardcoding in this function :((
			CardData CardData = __instance.CardData;
			MaterialPropertyBlock propBlock = ___propBlock;
			Color color = ColorManager.instance.DefaultCard;
			Color color2 = ColorManager.instance.DefaultCard2;
			Color iconColor = ColorManager.instance.DefaultCardIcon;
			if (CardData.MyCardType == CardType.Rumors)
			{
				color = ColorManager.instance.RumorCard;
				color2 = ColorManager.instance.RumorCard2;
				iconColor = ColorManager.instance.RumorCardIcon;
			}
			else if (CardData.MyCardType == CardType.Locations)
			{
				color = ColorManager.instance.LocationCard;
				color2 = ColorManager.instance.LocationCard2;
				iconColor = ColorManager.instance.LocationCardIcon;
			}
			else if (CardData is Mimic mimic && !mimic.WasDetected)
			{
				color = ColorManager.instance.GoldCard;
				color2 = ColorManager.instance.GoldCard2;
				iconColor = ColorManager.instance.GoldCardIcon;
			}
			else if ((CardData is Mob mob && mob.IsAggressive) || CardData is StrangePortal || CardData is PirateBoat)
			{
				color = ColorManager.instance.AggresiveMobCard;
				color2 = ColorManager.instance.AggresiveMobCard2;
				iconColor = ColorManager.instance.AggresiveMobIcon;
			}
			else if (CardData.MyCardType == CardType.Fish)
			{
				color = ColorManager.instance.FishCard;
				color2 = ColorManager.instance.FishCard2;
				iconColor = ColorManager.instance.FishCardIcon;
			}
			else if (CardData.Id == "gold" || CardData.Id == "goblet" || CardData.Id == "key" || CardData.Id == "treasure_chest" || CardData.Id == "temple" || CardData.Id == "shell" || CardData.Id == "sacred_key" || CardData.Id == "sacred_chest" || CardData.Id == "island_relic" || CardData.Id == "compass")
			{
				color = ColorManager.instance.GoldCard;
				color2 = ColorManager.instance.GoldCard2;
				iconColor = ColorManager.instance.GoldCardIcon;
			}
			else if (CardData.Id == "corpse")
			{
				color = ColorManager.instance.CorpseCard;
				color2 = ColorManager.instance.CorpseCard2;
				iconColor = ColorManager.instance.CorpseCardIcon;
			}
			else if (CardData.MyCardType == CardType.Structures)
			{
				if (CardData.IsBuilding)
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
			else if (CardData.MyCardType == CardType.Ideas)
			{
				color = ColorManager.instance.BlueprintCard;
				color2 = ColorManager.instance.BlueprintCard2;
				iconColor = ColorManager.instance.BlueprintCardIcon;
			}
			else if (CardData.MyCardType == CardType.Resources)
			{
				color = ColorManager.instance.ResourceCard;
				color2 = ColorManager.instance.ResourceCard2;
				iconColor = ColorManager.instance.ResourceCardIcon;
			}
			else if (CardData.MyCardType == CardType.Food)
			{
				color = ColorManager.instance.FoodCard;
				color2 = ColorManager.instance.FoodCard2;
				iconColor = ColorManager.instance.FoodCardIcon;
			}
			else if (CardData.MyCardType == CardType.Mobs)
			{
				color = ColorManager.instance.MobCard;
				color2 = ColorManager.instance.MobCard2;
				iconColor = ColorManager.instance.MobCardIcon;
			}
			else if (CardData.MyCardType == CardType.Equipable)
			{
				color = ColorManager.instance.EquipableCard;
				color2 = ColorManager.instance.EquipableCard2;
				iconColor = ColorManager.instance.EquipableCardIcon;
			}
			var mo = __instance.CardData.GetComponent<ModOverride>();
			if (mo != null)
			{
				color = mo.Color ?? color;
				color2 = mo.Color2 ?? color2;
				iconColor = mo.IconColor ?? iconColor;
			}
			__instance.CombatStatusCircle.color = __instance.CombatCircleColor;
			__instance.CombatStatusCircle.color = Color.red;
			if (__instance.IsHit)
			{
				__instance.CombatStatusCircle.color = Color.white;
				color = (color2 = (iconColor = Color.white));
			}
			__instance.CardRenderer.shadowCastingMode = ((!__instance.IsEquipped) ? ShadowCastingMode.On : ShadowCastingMode.Off);
			__instance.CardRenderer.GetPropertyBlock(propBlock, 2);
			propBlock.SetColor("_Color", color);
			propBlock.SetColor("_Color2", color2);
			propBlock.SetColor("_IconColor", iconColor);
			Texture2D texture2D = null;
			if (CardData is ResourceChest)
			{
				texture2D = SpriteManager.instance.ChestIconSecondary.texture;
			}
			else if (CardData is ResourceMagnet)
			{
				texture2D = SpriteManager.instance.MagnetIconSecondary.texture;
			}
			propBlock.SetFloat("_HasSecondaryIcon", (texture2D != null) ? 1f : 0f);
			if (texture2D != null)
			{
				propBlock.SetTexture("_SecondaryTex", texture2D);
			}
			float value2 = mo?.ShineStrength ?? ((CardData is Equipable) ? 0.3f : 1f);
			propBlock.SetFloat("_BigShineStrength", mo?.BigShineStrength ?? ((CardData is Equipable) ? 0f : 1f));
			propBlock.SetFloat("_ShineStrength", value2);
			propBlock.SetFloat("_Foil", (mo?.Foil ?? CardData.IsFoil || CardData.Id == "gold" || CardData.Id == "goblet" || CardData.Id == "shell" || CardData is Equipable) ? 1f : 0f);
			if (__instance.IconRenderer.sprite != null)
			{
				propBlock.SetTexture("_IconTex", __instance.IconRenderer.sprite.texture);
			}
			else
			{
				propBlock.SetTexture("_IconTex", SpriteManager.instance.EmptyTexture.texture);
			}
			__instance.CardRenderer.SetPropertyBlock(propBlock, 2);
			__instance.SpecialText.color = color;
			__instance.SpecialIcon.color = iconColor;
			__instance.IconRenderer.color = iconColor;
			__instance.CoinText.color = color;
			__instance.CoinIcon.color = iconColor;
			__instance.EquipmentButton.Color = color;
			__instance.CardNameText.color = iconColor;
			return false;
		}
	}
}