﻿using BotEngine.Common;
using System.Collections.Generic;
using System.Linq;
using Sanderling.Motor;
using Sanderling.Parse;
using System;
using WindowsInput.Native;
using Sanderling.Interface.MemoryStruct;
using Sanderling.ABot.Parse;
using Bib3;

namespace Sanderling.ABot.Bot.Task
{
	public static class Hotkeys
	{
		public static HotkeyTask LaunchDrones =
			new HotkeyTask(VirtualKeyCode.VK_F, VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT);

		public static HotkeyTask ReturnDrones =
			new HotkeyTask(VirtualKeyCode.VK_R, VirtualKeyCode.SHIFT);
		public static HotkeyTask EngageDrones =
			new HotkeyTask(VirtualKeyCode.VK_F);
	}
	public class CombatTask : IBotTask
	{
		const int TargetCountMax = 4;

		public Bot bot;

		public bool Completed { private set; get; }

		public IEnumerable<IBotTask> Component
		{
			get
			{
				var memoryMeasurementAtTime = bot?.MemoryMeasurementAtTime;
				var memoryMeasurementAccu = bot?.MemoryMeasurementAccu;

				var memoryMeasurement = memoryMeasurementAtTime?.Value;

				if (!memoryMeasurement.ManeuverStartPossible())
					yield break;

				var listOverviewEntryToAttack =
					memoryMeasurement?.WindowOverview?.FirstOrDefault()?.ListView?.Entry?.Where(entry => entry?.MainIcon?.Color?.IsRed() ?? false)
					?.OrderBy(entry => bot.AttackPriorityIndex(entry))
					?.ThenBy(entry => entry?.DistanceMax ?? int.MaxValue)
					?.ToArray();

				var targetSelected =
					memoryMeasurement?.Target?.FirstOrDefault(target => target?.IsSelected ?? false);

				var shouldAttackTarget =
					listOverviewEntryToAttack?.Any(entry => entry?.MeActiveTarget ?? false) ?? false;

				var setModuleWeapon =
					memoryMeasurementAccu?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? false);

				if (null != targetSelected)
					if (shouldAttackTarget)
						//yield return new HotkeyTask(VirtualKeyCode.F1);
						yield return bot.EnsureIsActive(setModuleWeapon);
					else
						yield return targetSelected.ClickMenuEntryByRegexPattern(bot, "unlock");

				var droneListView = memoryMeasurement?.WindowDroneView?.FirstOrDefault()?.ListView;

				var droneGroupWithNameMatchingPattern = new Func<string, DroneViewEntryGroup>(namePattern =>
						droneListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(group => group?.LabelTextLargest()?.Text?.RegexMatchSuccessIgnoreCase(namePattern) ?? false));

				var droneGroupInBay = droneGroupWithNameMatchingPattern("bay");
				var droneGroupInLocalSpace = droneGroupWithNameMatchingPattern("local space");

				var droneInBayCount = droneGroupInBay?.Caption?.Text?.CountFromDroneGroupCaption();
				var droneInLocalSpaceCount = droneGroupInLocalSpace?.Caption?.Text?.CountFromDroneGroupCaption();

				//	assuming that local space is bottommost group.
				var setDroneInLocalSpace =
					droneListView?.Entry?.OfType<DroneViewEntryItem>()
					?.Where(drone => droneGroupInLocalSpace?.RegionCenter()?.B < drone?.RegionCenter()?.B)
					?.ToArray();

				var droneInLocalSpaceSetStatus =
					setDroneInLocalSpace?.Select(drone => drone?.LabelText?.Select(label => label?.Text?.StatusStringFromDroneEntryText()))?.ConcatNullable()?.WhereNotDefault()?.Distinct()?.ToArray();

				var droneInLocalSpaceIdle =
					droneInLocalSpaceSetStatus?.Any(droneStatus => droneStatus.RegexMatchSuccessIgnoreCase("idle")) ?? false;

				if (shouldAttackTarget)
				{
					if (0 < droneInBayCount && droneInLocalSpaceCount < 4)
						yield return Hotkeys.LaunchDrones;

					if (droneInLocalSpaceIdle)
						yield return Hotkeys.EngageDrones;
				}

				var overviewEntryLockTarget =
					listOverviewEntryToAttack?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false) || (entry?.MeTargeting ?? false)));

				if (null != overviewEntryLockTarget && !(TargetCountMax <= memoryMeasurement?.Target?.Length))
					yield return overviewEntryLockTarget.ClickWithModifier(bot, VirtualKeyCode.CONTROL);


				if (!(0 < listOverviewEntryToAttack?.Length))
					if (0 < droneInLocalSpaceCount)
						yield return Hotkeys.ReturnDrones;
					else
						Completed = true;
			}
		}

		public IEnumerable<MotionParam> Effects => null;
	}
}
