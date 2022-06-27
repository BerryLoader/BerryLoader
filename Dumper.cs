using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using UnityEngine;

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
				JObject b = new JObject
				{
					["name"] = booster.Name,
					["id"] = booster.BoosterId,
					["minAchievementCount"] = booster.MinAchievementCount,
					["cardBags"] = (JToken)booster.CardBags
						.Select(cardBag => new JObject
						{
							["type"] = cardBag.CardBagType.ToString(),
							["cardsInPack"] = cardBag.CardsInPack,
							["setCardBag"] = cardBag.SetCardBag.ToString(),
							["chances"] = (JToken)cardBag.Chances
								.Select(cardChance => new JObject
								{
									["id"] = cardChance.Id,
									["chance"] = cardChance.Chance,
									["hasMaxCount"] = cardChance.HasMaxCount,
									["maxCountToGive"] = cardChance.MaxCountToGive,
									["prerequisiteCardId"] = cardChance.PrerequisiteCardId
								}),
							["setPackCards"] = (JToken)cardBag.SetPackCards.Select(s => s)
						})
				};
				BerryLoader.L.LogInfo(b.ToString());
			}
		}

		public static void DumpBlueprints()
		{
			foreach (var blueprint in WorldManager.instance.BlueprintPrefabs)
			{
				BerryLoader.L.LogInfo($"dumping {blueprint.Id}");
				JObject b = new JObject
				{
					["name"] = blueprint.Name,
					["id"] = blueprint.Id,
					["blueprintGroup"] = blueprint.BlueprintGroup.ToString(),
					["stackPostText"] = blueprint.StackPostText,
					["subprints"] = (JToken)blueprint.Subprints
						.Select(sp => new JObject
						{
							["requiredCards"] = new JArray(sp.RequiredCards),
							["cardsToRemove"] = new JArray(sp.CardsToRemove),
							["resultCard"] = sp.ResultCard,
							["extraResultCards"] = new JArray(sp.ExtraResultCards),
							["time"] = sp.Time,
							["status"] = sp.StatusName
						})
				};
				BerryLoader.L.LogInfo(b.ToString());
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