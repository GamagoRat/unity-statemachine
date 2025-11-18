using System;
using System.Collections.Generic;
using UnityEngine;

namespace States
{
	public class Stater<TStateIdentifier> where TStateIdentifier : struct, IConvertible
	{
		private string debugName;
		private static bool enableDebugLog;

		private StaterState<TStateIdentifier> currentState;

		private static TStateIdentifier defaultState = Activator.CreateInstance<TStateIdentifier>();

		private Dictionary<TStateIdentifier, StaterState<TStateIdentifier>> states = new Dictionary<TStateIdentifier, StaterState<TStateIdentifier>>();

        public TStateIdentifier CurState => this.currentState?.CurrentState ?? Stater<TStateIdentifier>.defaultState;
        public float StateTime => this.currentState?.StepTime ?? 0.0f;

        public Stater(string debugName)
		{
			this.debugName = debugName;
		}

		public Stater(string debugName, bool enableDebugLog)
		{
			this.debugName = debugName;
			Stater<TStateIdentifier>.enableDebugLog = enableDebugLog;
		}

		public StaterState<TStateIdentifier> AddState(TStateIdentifier stateIdentifier)
		{
			var staterState = new StaterState<TStateIdentifier>(stateIdentifier);
			this.states.Add(stateIdentifier, staterState);
			this.currentState ??= staterState;
            return staterState;
		}

		public StaterState<TStateIdentifier> GetState(TStateIdentifier stateIdentifier)
        {
            this.states.TryGetValue(stateIdentifier, out var result);
			return result;
		}

		public void Go(TStateIdentifier stateID)
		{
			StaterState<TStateIdentifier> state = this.GetState(stateID);
			if (state == null)
			{
				Debug.LogError($"{this.debugName}: CurrentState not found: {stateID.ToString()}");
				return;
			}

			if (Stater<TStateIdentifier>.enableDebugLog)
			{
                Debug.Log($"{this.debugName}: {this.currentState?.CurrentState.ToString() ?? "-"} -> {stateID}");
			}

			this.currentState?.Exit();
			this.currentState = state;
			this.currentState.Enter();
		}

		public void Trigger(string triggerID)
		{
			if (Stater<TStateIdentifier>.enableDebugLog)
            {
                Debug.Log($"{this.debugName}: Trigger [{triggerID}]");
            }

			this.currentState?.Trigger(triggerID);
		}

		public void Step(float dt)
		{
			if (this.currentState == null)
			{
				return;
			}

			if (this.currentState.StepDuration >= 0.0f
				&& this.currentState.HasAfterState
				&& this.currentState.StepTime >= this.currentState.StepDuration)
			{
				this.Go(this.currentState.NextState);
			} 
            else
			{
				this.currentState.Step(dt);
			}
		}
	}   
}
