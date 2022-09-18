using System;
using System.Collections.Generic;
using System.Linq;

namespace BerryLoaderNS
{
	public static class EnumHelper
	{
		public static List<string> CustomCardTypes = new List<string>();
		public static List<string> CustomBlueprintGroups = new List<string>();

		public static CardType CreateCardType(string str)
		{
			var vanilla = Enum.GetValues(typeof(CardType)).Length;
			var custom = CustomCardTypes.Count;
			if (CustomCardTypes.Contains(str))
				return (CardType)vanilla + CustomCardTypes.IndexOf(str);

			CustomCardTypes.Add(str);
			return (CardType)vanilla + custom;
		}

		public static BlueprintGroup CreateBlueprintGroup(string str)
		{
			var vanilla = Enum.GetValues(typeof(BlueprintGroup)).Length;
			var custom = CustomBlueprintGroups.Count;
			if (CustomBlueprintGroups.Contains(str))
				return (BlueprintGroup)vanilla + CustomBlueprintGroups.IndexOf(str);

			CustomBlueprintGroups.Add(str);
			return (BlueprintGroup)vanilla + custom;
		}

		public static CardType ToCardType(string str)
		{
			str = str.ToLower();
			return CustomCardTypes.Contains(str) ? (CardType)Enum.GetValues(typeof(CardType)).Length + CustomCardTypes.IndexOf(str) : (CardType)Enum.Parse(typeof(CardType), str, true);
		}

		public static BlueprintGroup ToBlueprintGroup(string str)
		{
			str = str.ToLower();
			return CustomBlueprintGroups.Contains(str) ? (BlueprintGroup)Enum.GetValues(typeof(BlueprintGroup)).Length + CustomBlueprintGroups.IndexOf(str) : (BlueprintGroup)Enum.Parse(typeof(BlueprintGroup), str, true);
		}

		public static CardBagType ToCardBagType(string str)
		{
			return (CardBagType)Enum.Parse(typeof(CardBagType), str, true);
		}

		public static SetCardBag ToSetCardBag(string str)
		{
			return (SetCardBag)Enum.Parse(typeof(SetCardBag), str, true);
		}
	}
}