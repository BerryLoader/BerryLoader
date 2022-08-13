using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace BerryLoaderNS
{
	public static class ReflectionHelper
	{
		// bit of a fun fact, noone knows who wrote this function. it originates from the early ktane modding community,
		// and it has been copied by people ever since. it is now in berryloader, enjoy
		public static IEnumerable<Type> GetSafeTypes(this Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(x => x != null);
			}
			catch (Exception)
			{
				return new List<Type>();
			}
		}

		// outdated code, probably shouldnt be used
		public static void CopyGameCardProps(GameCard modcard, GameCard original)
		{
			// probably missing stuff here lol
			var copyProps = "CardNameText IconRenderer CardData startScale CardRenderer HighlightRectangle InCombatStatus CoinIcon CoinText SpecialText SpecialIcon CombatStatusCircle CombatStatusIcon SpecialValue HighlightActive lastPosition SpawnRotation snappedToParent propBlock FaceUp NewCircle newCircleStartSize FoilParticles materialChangers IsDemoCard BounceTarget PushEnabled SetY CombatCircleColor Destroyed WasClicked IsNew newCircleTimer wiggleTimer flipTimer RotWobbleAmp RotWobbleSpeed RotWobbleSpringiness wobbleRotVelo AutoRotWobble AutoRotWobbleTimer AutoRotWobbleAmount timer rotWobbleTimer curZ TimerRunning Status CurrentTimerTime TargetTimerTime TimerAction TimerBlueprintId TimerSubprintIndex TimerActionId CurrentStatusbar removedChild curHeight TargetPosition".Split(' ');
			foreach (var prop in copyProps)
			{
				var field = typeof(GameCard).GetField(prop, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				field.SetValue(modcard, field.GetValue(original));
			}
		}

		public static void CopyCardDataProps(CardData modcarddata, CardData original)
		{
			var copyProps = "NameTerm DescriptionTerm Id PickupSound PickupSoundGroup UniqueId ParentUniqueId Value Icon MyGameCard IsFoil MyCardType IsBuilding IsCookedFood HideFromCardopedia StatusEffects creationTime IsIslandCard ExpectedValue".Split(' ');
			foreach (var prop in copyProps)
			{
				var field = typeof(CardData).GetField(prop, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				field.SetValue(modcarddata, field.GetValue(original));
			}
		}
	}
}