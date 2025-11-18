using UnityEngine;
using States;

public class StaterExampleScript : MonoBehaviour
{
    public enum ExampleState
    {
        State1,
        State2,
        State3
    }

    private Stater<ExampleState> m_Stater;

    private void Start()
    {
        // Stater should be created and configured at start
        CreateStater();

        m_Stater.Go(ExampleState.State1); // Tells the Stater to enter State1
    }

    private void CreateStater()
    {
        m_Stater = new Stater<ExampleState>("Stater", false);

        m_Stater.AddState(ExampleState.State1).AddAction(StaterAction.ENTER(() =>
        {
            // Executed when Stater enters state State1
        })).AddAction(StaterAction.STEP(() =>
        {
            // Executed each frame as long as Stater is in State1
        })).AddAction(StaterAction.EXIT(() =>
        {
            // Executed when Stater exits state State2
        }));

        m_Stater.AddState(ExampleState.State2).AddAction(StaterAction.ENTER(() =>
        {
            // Executed when Stater enters state State2
        })).SetDuration(1.0f, ExampleState.State1); // Instructs Stater to automatically go to State1 after 1 second in State2

        m_Stater.AddState(ExampleState.State3).AddAction(StaterAction.AT_STEP(2.0f, () =>
        {
            // Executed 2 second after entered State3
        })).AddAction(StaterAction.ON_TRIGGER("Trigger", () =>
        {
            // Executed when Stater triggered "Trigger" while on State3 
        }));
    }

    private void Update()
    {
        // Be sure to call Step function each frame, or STEP instructions defined in Start won't be executed
        m_Stater.Step(Time.deltaTime);
    }

    private void ExampleUse()
    {
        m_Stater.Go(ExampleState.State2); // Go to State2, calls first State1's Exit function, then State2's Enter function

        m_Stater.Trigger("Trigger"); // Calls Trigger function if exists on current CurrentState

        ExampleState CurState = m_Stater.CurState;
    }
}
