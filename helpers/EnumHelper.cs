using System;

namespace BerryLoaderNS
{
	public static class EnumHelper
	{
		public static CardType ToCardType(string str)
		{
			return (CardType)Enum.Parse(typeof(CardType), str, true);
		}

		public static BlueprintGroup ToBlueprintGroup(string str)
		{
			return (BlueprintGroup)Enum.Parse(typeof(BlueprintGroup), str, true);
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