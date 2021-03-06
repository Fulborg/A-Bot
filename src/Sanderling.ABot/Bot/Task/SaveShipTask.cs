﻿using System.Collections.Generic;
using System.Linq;
using Sanderling.Motor;
using BotEngine.Common;
using Sanderling.ABot.Parse;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Task
{
	public class SaveShipTask : IBotTask
	{
		public const string CannotIdentifyLocalChatWindowDiagnosticText = "can not identify local chat window.";
		public const string LocalChatWindowNotFoundDiagnosticText = "local chat window not found.";

		public Bot Bot;

		public bool AllowRoam;

		public bool AllowAnomalyEnter;

		private const int AllowRoamSessionDurationMin = 60 * 7;

		private const int AllowAnomalyEnterSessionDurationMin = AllowRoamSessionDurationMin + 60 * 7;

		/// <summary>
		/// Checks that current chat contains only trusted chars
		/// </summary>
		static public bool ChatIsClean(WindowChatChannel chatWindow)
		{
			if (null == chatWindow)
				return false;

			if (chatWindow?.ParticipantView?.Scroll?.IsScrollable() ?? true)
				return false;

			var listParticipantNeutralOrEnemy =
				chatWindow?.ParticipantView?.Entry?.Where(participant => participant.IsNeutralOrEnemy())?.ToArray();

			//	we expect own char to show up there as well so there has to be one participant with neutral or enemy flag.
			return 1 == listParticipantNeutralOrEnemy?.Length;
		}

		public IEnumerable<IBotTask> Component
		{
			get
			{
				var memoryMeasurement = Bot?.MemoryMeasurementAtTime?.Value;

				var charIsLocatedInHighsec = 500 <= memoryMeasurement?.InfoPanelCurrentSystem?.SecurityLevelMilli;

				if (charIsLocatedInHighsec)
				{
					AllowRoam = true;
					AllowAnomalyEnter = AllowAnomalyEnterSessionDurationMin <=
					                    memoryMeasurement?.SessionDurationRemaining;
					yield break;
				}

				var setLocalChatWindowCandidate =
					memoryMeasurement?.WindowChatChannel
					?.Where(window => window?.Caption?.RegexMatchSuccessIgnoreCase(@"local") ?? false)
					?.ToArray();


				if (1 < setLocalChatWindowCandidate?.Length)
					yield return new DiagnosticTask(CannotIdentifyLocalChatWindowDiagnosticText);

				var localChatWindow = setLocalChatWindowCandidate?.FirstOrDefault();

				if (null == localChatWindow)
					yield return new DiagnosticTask(LocalChatWindowNotFoundDiagnosticText);

				var sessionDurationSufficient = AllowRoamSessionDurationMin <= memoryMeasurement?.SessionDurationRemaining;

				if (sessionDurationSufficient && (charIsLocatedInHighsec || ChatIsClean(localChatWindow)))
				{
					AllowRoam = true;
					AllowAnomalyEnter = AllowAnomalyEnterSessionDurationMin <= memoryMeasurement?.SessionDurationRemaining;
					yield break;
				}

				yield return new RetreatTask
				{
					Bot = Bot,
				};
			}
		}

		public IEnumerable<MotionRecommendation> ClientActions => null;
	}
}
