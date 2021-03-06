﻿using System.Collections.Generic;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot
{
	public interface IBotTask
	{
		IEnumerable<IBotTask> Component { get; }

		/// <summary>
		/// ClientActions to apply to the eve online client.
		/// </summary>
		IEnumerable<MotionRecommendation> ClientActions { get; }
	}

	public interface ISerializableBotTask : IBotTask
	{
		string ToJson();
	}
}