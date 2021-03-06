﻿using System.Collections.Generic;
using System.Linq;
using Sanderling.Motor;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class UndockTask : IBotTask
	{
		private IMemoryMeasurement MemoryMeasurement;

		public UndockTask(IMemoryMeasurement memoryMeasurement)
		{
			MemoryMeasurement = memoryMeasurement;
		}

		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionRecommendation> ClientActions
		{
			get
			{
				if (MemoryMeasurement?.IsUnDocking ?? false)
					yield break;

				if (!(MemoryMeasurement?.IsDocked ?? false))
					yield break;

				yield return MemoryMeasurement?.WindowStation?.FirstOrDefault()?.ButtonText
					.SingleOrDefault(b => !b.Text.Contains("Abort") && b.Text== "<center>Undock</center>")
					?.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left)
					.AsRecommendation();
			}
		}
	}
}
