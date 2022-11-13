using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BerryLoaderNS
{
	public class ModOverride : MonoBehaviour
	{
		public string Name;
		public string Description;
		public Color? Color;
		public Color? Color2;
		public Color? IconColor;
		public float? ShineStrength;
		public float? BigShineStrength;
		public bool? Foil;
		public Dictionary<int, string> SubprintStatuses = new Dictionary<int, string>();
	}

	public class ModCard
	{
		public string id;
		public string nameTerm;
		public string nameOverride;
		public string descriptionTerm;
		public string descriptionOverride;
		public int value;
		public string type;
		public string icon;
		public string audio;
		public string script = "";
		[JsonExtensionData] public IDictionary<string, JToken> ExtraProps;
	}

	public class ModBlueprint
	{
		public string id;
		public string nameTerm;
		public string nameOverride;
		public int value = 1; // default? no clue if it works or not
		public string icon;
		public string group;
		public List<ModSubprint> subprints;
	}

	public class ModSubprint
	{
		public string requiredCards;
		public string cardsToRemove;
		public string resultCard;
		public string extraResultCards; //?? confusing? doesnt seem to do anything useful but yet it does??
		public float time;
		public string statusTerm;
		public string statusOverride;
	}

	public class ModBoosterpack
	{
		public string id;
		public string name;
		public string icon;
		public int minAchievementCount;
		public int cost;
		public List<ModCardBag> cardBags;
	}

	public class ModCardBag
	{
		public string type;
		public int cards;
		public string setCardBag;
		public List<ModCardChance> chances;
	}

	public class ModCardChance
	{
		public string id;
		public int chance;
		public bool hasMaxCount;
		public int maxCountToGive;
		public string Prerequisite;
	}
}