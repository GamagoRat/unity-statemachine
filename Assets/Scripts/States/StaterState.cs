using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace States
{
	public class StaterState<StateIdentifier> where StateIdentifier : struct, IConvertible
	{
		public readonly StateIdentifier CurrentState;

		public bool HasAfterState { get; private set; } = false;
        public StateIdentifier NextState { get; private set; }
		public bool NeedsStep { get; private set; } = false;
		public float StepTime { get; private set; }
		public float StepDuration { get; private set; }

		private List<StaterAction> actions = new List<StaterAction>();

		public StaterState(StateIdentifier currentState)
		{
			this.CurrentState = currentState;
		}

		public StaterState<StateIdentifier> AddAction(StaterAction action)
		{
			this.actions.Add(action);
			this.NeedsStep = (action.CurrentTiming == StaterAction.Timing.Step || action.CurrentTiming == StaterAction.Timing.AtStep);
			return this;
		}

		public StaterState<StateIdentifier> SetDuration(float stepDuration)
		{
			this.StepDuration = stepDuration;
			this.NeedsStep = (this.StepDuration > 0);
			return this;
		}

		public StaterState<StateIdentifier> SetDuration(float stepDuration, StateIdentifier afterState)
		{
			this.StepDuration = stepDuration;
			this.NextState = afterState;
			this.HasAfterState = true;
			this.NeedsStep = (this.StepDuration > 0.0f);
			return this;
		}

		public void Enter()
		{
			foreach (var staterFunc in this.actions)
			{
				if (staterFunc.CurrentTiming == StaterAction.Timing.Enter)
				{
					staterFunc.Handler();
				}
				staterFunc.IsCalled = false;
			}
			this.StepTime = 0.0f;
		}

		public void Step(float dt)
		{
			this.StepTime += dt;
			foreach (var staterFunc in this.actions)
			{
                switch (staterFunc.CurrentTiming)
                {
					case StaterAction.Timing.AtStep:
                        if (!staterFunc.IsCalled && this.StepTime >= staterFunc.FrameTime)
                        {
							staterFunc.IsCalled = true;
							staterFunc.Handler();
                        }
						break;
					case StaterAction.Timing.Step:
						staterFunc.Handler();
						break;
                }
			}
		}

		public void Trigger(string triggerIdentifier)
		{
			var matches = this.actions
                .Where(staterAction => staterAction.CurrentTiming == StaterAction.Timing.OnTrigger && staterAction.TriggerName == triggerIdentifier)
				.ToList();


            if (matches.Count > 0)
            {
                matches.ForEach(a => a.Handler());
            }
			else
			{
				Debug.LogWarningFormat($"{this.CurrentState.ToString()} does not handle trigger \"{triggerIdentifier}\"!");
            }
		}

		public void Exit()
        {
            foreach (var staterAction in this.actions.Where(action => action.CurrentTiming == StaterAction.Timing.Exit))
            {
                staterAction.Handler();
            }
        }
	}
}
