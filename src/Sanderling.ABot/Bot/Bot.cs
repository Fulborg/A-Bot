﻿using Bib3;
using Sanderling.Parse;
using BotEngine.Interface;
using System.Linq;
using System.Collections.Generic;
using System;
using Sanderling.ABot.Bot.Task;
using Sanderling.ABot.Bot.Memory;
using Sanderling.ABot.Bot.Strategies;
using Sanderling.ABot.Serialization;

namespace Sanderling.ABot.Bot
{

	internal interface IStrategy
	{
		IEnumerable<IBotTask> GetTasks(Bot bot);
	}
	public class Bot
	{
		static public readonly Func<long> GetTimeMilli = Bib3.Glob.StopwatchZaitMiliSictInt;

		public BotStepInput StepLastInput { private set; get; }

		public PropertyGenTimespanInt64<BotStepResult> StepLastResult { private set; get; }

		private IStrategy strategy = new CorporationMissionTaker();

		int motionId;

		int stepIndex;
		/// <summary>
		/// Current measurements
		/// </summary>
		public FromProcessMeasurement<IMemoryMeasurement> MemoryMeasurementAtTime { private set; get; }

		readonly public Accumulator.MemoryMeasurementAccumulator MemoryMeasurementAccu = new Accumulator.MemoryMeasurementAccumulator();

		readonly public OverviewMemory OverviewMemory = new OverviewMemory();

		readonly IDictionary<long, int> MouseClickLastStepIndexFromUIElementId = new Dictionary<long, int>();

		readonly IDictionary<Accumulation.IShipUiModule, int> ToggleLastStepIndexFromModule = new Dictionary<Accumulation.IShipUiModule, int>();

		public KeyValuePair<Deserialization, Config> ConfigSerialAndStruct { private set; get; }

		public long? MouseClickLastAgeStepCountFromUIElement(Interface.MemoryStruct.IUIElement uiElement)
		{
			if (null == uiElement)
				return null;

			var interactionLastStepIndex = MouseClickLastStepIndexFromUIElementId?.TryGetValueNullable(uiElement.Id);

			return stepIndex - interactionLastStepIndex;
		}

		public long? ToggleLastAgeStepCountFromModule(Accumulation.IShipUiModule module) =>
			module == null ? null :
			stepIndex - ToggleLastStepIndexFromModule?.TryGetValueNullable(module);


		void MemorizeStepInput(BotStepInput input)
		{
			ConfigSerialAndStruct = (input?.ConfigSerial?.String).DeserializeIfDifferent(ConfigSerialAndStruct);

			MemoryMeasurementAtTime = input?.FromProcessMemoryMeasurement?.MapValue(measurement => measurement?.Parse());

			MemoryMeasurementAccu.Accumulate(MemoryMeasurementAtTime);

			OverviewMemory.Aggregate(MemoryMeasurementAtTime);
		}

		void MemorizeStepResult(BotStepResult stepResult)
		{
			var setMotionMouseWaypointUIElement =
				stepResult?.ListMotion
				?.Select(motion => motion?.MotionParam)
				?.Where(motionParam => 0 < motionParam?.MouseButton?.Count())
				?.Select(motionParam => motionParam?.MouseListWaypoint)
				?.ConcatNullable()?.Select(mouseWaypoint => mouseWaypoint?.UIElement)?.WhereNotDefault();

			foreach (var mouseWaypointUIElement in setMotionMouseWaypointUIElement.EmptyIfNull())
				MouseClickLastStepIndexFromUIElementId[mouseWaypointUIElement.Id] = stepIndex;
		}

		public BotStepResult Step(BotStepInput input)
		{
			var beginTimeMilli = GetTimeMilli();

			StepLastInput = input;

			Exception exception = null;

			var listMotion = new List<MotionRecommendation>();

			IBotTask[][] outputListTaskPath = null;

			try
			{
				MemorizeStepInput(input);

				outputListTaskPath = ((IBotTask)new BotTask { Component = strategy.GetTasks(this) })
					?.EnumeratePathToNodeFromTreeDFirst(node => node?.Component)
					?.Where(taskPath => (taskPath?.LastOrDefault()).ShouldBeIncludedInStepOutput())
					?.TakeSubsequenceWhileUnwantedInferenceRuledOut()
					?.ToArray();

				foreach (var moduleToggle in outputListTaskPath.ConcatNullable().OfType<ModuleToggleTask>()
					.Select(moduleToggleTask => moduleToggleTask?.module).WhereNotDefault())
					ToggleLastStepIndexFromModule[moduleToggle] = stepIndex;

				foreach (var effect in outputListTaskPath.EmptyIfNull().SelectMany(taskPath =>
					(taskPath?.LastOrDefault()?.ApplicableEffects()).EmptyIfNull()))
				{
					listMotion.Add(new MotionRecommendation
					{
						Id = motionId++,
						MotionParam = effect,
					});
				}
			}
			catch (Exception e)
			{
				exception = e;
			}

			var stepResult = new BotStepResult
			{
				Exception = exception,
				ListMotion = listMotion?.ToArrayIfNotEmpty(),
				OutputListTaskPath = outputListTaskPath,
			};

			MemorizeStepResult(stepResult);

			StepLastResult = new PropertyGenTimespanInt64<BotStepResult>(stepResult, beginTimeMilli, GetTimeMilli());

			++stepIndex;

			return stepResult;
		}
		
		//IEnumerable<IBotTask> EnumerateConfigDiagnostics()
		//{
		//	var configDeserializeException = ConfigSerialAndStruct.Key?.Exception;

		//	if (null != configDeserializeException)
		//		yield return new DiagnosticTask { MessageText = "error parsing configuration: " + configDeserializeException.Message };
		//	else
		//		if (null == ConfigSerialAndStruct.Value)
		//		yield return new DiagnosticTask { MessageText = "warning: no configuration supplied." };
		//}
	}
}
