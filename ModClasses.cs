using System.Collections.Generic;
using UnityEngine;

namespace BerryLoaderNS
{
	public class BerryLoaderMod
	{
		public ModManifest manifest;
		public virtual void Init() { }
		public virtual void PreInjection() { }
		public virtual void PostInjection() { }
	}

	public class ModManifest
	{
		public string id;
		public string name;
		public string repo;
		public string version;
		public string developer;
		public string description;
		public Dictionary<string, string> dependencies;
	}

	class ModOverride : MonoBehaviour
	{
		public string Name;
		public string Description;
	}

	public class ModCard
	{
		public string id;
		public string name;
		public string description;
		public int value;
		public string type;
		public string icon;
		public string audio;
		public string gameCardScript = "";
		public string cardDataScript = "";
	}

	public class ModBlueprint
	{
		public string id;
		public string name;
		// public string description; // probably unnecessary? these dont show up in the cardopedia
		public int value = 1; // default? no clue if it works or not
		public string icon;
		public string group; // unused till i figure stuff out
		public string stackText;
		public List<ModSubprint> subprints;
	}

	public class ModSubprint
	{
		public string requiredCards;
		public string cardsToRemove;
		public string resultCard;
		public string extraResultCards; //?? confusing? doesnt seem to do anything useful but yet it does??
		public float time;
		public string status; // translation
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