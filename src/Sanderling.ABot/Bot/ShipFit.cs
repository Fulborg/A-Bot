﻿using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot
{
	public class ShipFit
	{
		private ModuleInfo[] High { get; }
		private ModuleInfo[] Mid { get; }
		private ModuleInfo[] Low { get; }

		public ShipFit(IEnumerable<Accumulation.IShipUiModule> memoryModules, ModuleInfo[][] fitInfo)
		{
			var modulesByY = memoryModules.GroupBy(m => m.Region.Min1).OrderBy(g => g.Key).ToArray();
			if (modulesByY.Count() != 3)
				throw new ArgumentException("Couldn't determine 3 module groups");
			High = modulesByY[0].Select((m, i) =>
			{
				fitInfo[0][i].UiModule = m;
				return fitInfo[0][i];
			}).ToArray();
			Mid = modulesByY[1].Select((m, i) =>
			{
				fitInfo[1][i].UiModule = m;
				return fitInfo[1][i];
			}).ToArray();
			Low = modulesByY[2].Select((m, i) =>
			{
				fitInfo[2][i].UiModule = m;
				return fitInfo[2][i];
			}).ToArray();
		}

		public IEnumerable<ModuleInfo> GetAlwaysActiveModules()
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == ModuleType.Hardener)
					yield return moduleInfo;
			}
		}

		public IEnumerable<ModuleInfo> GetShieldBoostersModules()
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == ModuleType.ShieldBooster)
					yield return moduleInfo;
			}
		}

		public ModuleInfo GetWeapon()
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == ModuleType.Weapon)
					return moduleInfo;
			}

			return null;
		}

		public ModuleInfo GetMWD()
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == ModuleType.MWD)
					return moduleInfo;
			}

			return null;
		}

		public IEnumerable<ModuleInfo> GetAllByType(ModuleType type)
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == type)
					yield return moduleInfo;
			}
		}

		public class ModuleInfo
		{
			public ModuleInfo(ModuleType type, params VirtualKeyCode[] hotKey)
			{
				Type = type;
				HotKey = hotKey;
			}

			public ModuleType Type { get; }
			public VirtualKeyCode[] HotKey { get; }

			public Accumulation.IShipUiModule UiModule { get; set; }

			public ISerializableBotTask EnsureActive(Bot bot, bool shouldBeActive)
			{
				if (shouldBeActive && !(UiModule.IsActive(bot) ?? true))
					return new ModuleToggleTask(this);
				if (!shouldBeActive && (UiModule.IsActive(bot) ?? false))
					return new ModuleToggleTask(this);

				return null;
			}
		}

		public enum ModuleType
		{
			Hardener,
			Weapon,
			ShieldBooster,
			MWD,
			Etc,
		}
	}
}