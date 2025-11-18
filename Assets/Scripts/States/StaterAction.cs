using System;

namespace States
{
	public class StaterAction
	{
		public enum Timing
		{
			Enter,
			Step,
			AtStep,
			OnTrigger,
			Exit
		}

		public Timing CurrentTiming { get; private set; }
        public Action Handler { get; private set; }
		public float FrameTime { get; private set; }
        public string TriggerName { get; private set; }
        public bool IsCalled { get; set; }

        private StaterAction(StaterAction.Timing timing, Action handler, float frameTime = 0.0f)
		{
			this.CurrentTiming = timing;
			this.Handler = handler;
			this.FrameTime = frameTime;
		}

		private StaterAction(StaterAction.Timing timing, Action handler, string triggerName)
		{
			this.CurrentTiming = timing;
			this.Handler = handler;
			this.TriggerName = triggerName;
		}

		public static StaterAction ENTER(Action handler)
		{
			return new StaterAction(StaterAction.Timing.Enter, handler);
		}

		public static StaterAction AT_STEP(float time, Action handler)
		{
			return new StaterAction(StaterAction.Timing.AtStep, handler, time);
		}

		public static StaterAction STEP(Action handler)
		{
			return new StaterAction(StaterAction.Timing.Step, handler);
		}

		public static StaterAction ON_TRIGGER(string triggerID, Action handler)
		{
			return new StaterAction(StaterAction.Timing.OnTrigger, handler, triggerID);
		}

		public static StaterAction EXIT(Action handler)
		{
			return new StaterAction(StaterAction.Timing.Exit, handler);
		}
	}
}
