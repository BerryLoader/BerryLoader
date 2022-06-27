using System;

namespace BerryLoaderNS
{
	public static class EnumHelper
	{
		public static T ToTCardType<T>(string str)
			=> (T)Enum.Parse(typeof(T), str, true);

		public static CardType ToCardType(string str)
			=> ToTCardType<CardType>(str);

		public static BlueprintGroup ToBlueprintGroup(string str)
			=> ToTCardType<BlueprintGroup>(str);

		public static CardBagType ToCardBagType(string str)
			=> ToTCardType<CardBagType>(str);

		public static SetCardBag ToSetCardBag(string str)
			=> ToTCardType<SetCardBag>(str);
	}
}