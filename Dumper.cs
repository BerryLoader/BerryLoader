using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System;

namespace BerryLoaderNS
{
	public static class Dumper
	{
		public static void DumpTextures()
		{
			BerryLoader.L.LogInfo("dumping textures..");
			foreach (var cd in WorldManager.instance.CardDataPrefabs)
			{
				BerryLoader.L.LogInfo($"dumping {cd.Id}.png");
				if (cd.Icon != null)
				{
					var tx = duplicateTexture(cd.Icon.texture);
					File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "dumps", $"card_{cd.Id}.png"), tx.EncodeToPNG());
				}
				else
					BerryLoader.L.LogInfo($"no texture found for {cd.Id}");
			}
			foreach (var bp in WorldManager.instance.BoosterPackPrefabs)
			{
				BerryLoader.L.LogInfo($"dumping {bp.BoosterId}.png");
				if (bp.BoosterId != null)
				{
					var tx = duplicateTexture(bp.BoosterpackIcon.texture);
					File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "dumps", $"booster_{bp.BoosterId}.png"), tx.EncodeToPNG());
				}
				else
					BerryLoader.L.LogInfo($"no texture found for {bp.BoosterId}");
			}
			BerryLoader.L.LogInfo("finished dumping");
		}

		public static void DumpBoosters()
		{
			foreach (var booster in WorldManager.instance.BoosterPackPrefabs)
			{
				BerryLoader.L.LogInfo($"dumping {booster.Name}");
				JObject b = new JObject();
				b["name"] = booster.Name;
				b["id"] = booster.BoosterId;
				b["minAchievementCount"] = booster.MinAchievementCount;
				JArray c = new JArray();
				foreach (CardBag cardBag in booster.CardBags)
				{
					JObject bag = new JObject();
					bag["type"] = cardBag.CardBagType.ToString();
					bag["cardsInPack"] = cardBag.CardsInPack;
					bag["setCardBag"] = cardBag.SetCardBag.ToString();
					JArray chances = new JArray();
					JArray setPackCards = new JArray();
					foreach (CardChance cardChance in cardBag.Chances)
					{
						JObject cc = new JObject();
						cc["id"] = cardChance.Id;
						cc["chance"] = cardChance.Chance;
						cc["hasMaxCount"] = cardChance.HasMaxCount;
						cc["maxCountToGive"] = cardChance.MaxCountToGive;
						cc["prerequisiteCardId"] = cardChance.PrerequisiteCardId;
						chances.Add(cc);
					}
					foreach (string s in cardBag.SetPackCards)
					{
						setPackCards.Add(s);
					}
					bag["chances"] = chances;
					bag["setPackCards"] = setPackCards;
					c.Add(bag);
				}
				b["cardBags"] = c;
				File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "dumps", $"booster_{booster.BoosterId}.json"), b.ToString());
			}
		}

		public static void DumpBlueprints()
		{
			foreach (var blueprint in WorldManager.instance.BlueprintPrefabs)
			{
				BerryLoader.L.LogInfo($"dumping {blueprint.Id}");
				JObject b = new JObject();
				b["name"] = blueprint.Name;
				b["id"] = blueprint.Id;
				b["blueprintGroup"] = blueprint.BlueprintGroup.ToString();
				b["needsExactMatch"] = blueprint.NeedsExactMatch;
				JArray s = new JArray();
				foreach (var sp in blueprint.Subprints)
				{
					JObject p = new JObject();
					p["requiredCards"] = new JArray(sp.RequiredCards);
					p["cardsToRemove"] = new JArray(sp.CardsToRemove);
					p["resultCard"] = sp.ResultCard;
					p["extraResultCards"] = new JArray(sp.ExtraResultCards);
					p["resultAction"] = sp.ResultAction;
					p["time"] = sp.Time;
					p["status"] = sp.StatusName;
					s.Add(p);
				}
				b["subprints"] = s;
				File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "dumps", $"{blueprint.Id}.json"), b.ToString());
			}
		}

		// https://stackoverflow.com/a/44734346
		public static Texture2D duplicateTexture(Texture2D source)
		{
			RenderTexture renderTex = RenderTexture.GetTemporary(
						source.width,
						source.height,
						0,
						RenderTextureFormat.Default,
						RenderTextureReadWrite.Linear);

			Graphics.Blit(source, renderTex);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = renderTex;
			Texture2D readableText = new Texture2D(source.width, source.height);
			readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
			readableText.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(renderTex);
			return readableText;
		}
	}
}