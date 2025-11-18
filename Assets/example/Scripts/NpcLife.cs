using States;
using UnityEngine;
using UnityEngine.AI;

public enum LOCATION
{
    BED,
    WORK_PLACE,
    TABLE,
}

public enum ACTION
{
    SLEEP,
    WORK,
    EAT,
    WALK,
}

[RequireComponent(typeof(NavMeshAgent))]
public class NpcLife : MonoBehaviour
{
    [Header("Locations")]
    [SerializeField] private Transform bedTransform;
    [SerializeField] private Transform workTransform;
    [SerializeField] private Transform eatTransform;

    [Header("Components")]
    [SerializeField] private Animator Animator;
    [SerializeField] private ParticleSystem sleepEffect;

    private NavMeshAgent agent;
    private Stater<LOCATION> locationStater;
    private Stater<ACTION> actionStater;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #region location
        locationStater = new Stater<LOCATION>("NpcLife", false);
        locationStater.AddState(LOCATION.BED).AddAction(StaterAction.ENTER(() => 
        {
            actionStater.Go(ACTION.WALK);
            agent.SetDestination(bedTransform.position);
        })).AddAction(StaterAction.STEP(() =>
        {
            if (agent.remainingDistance > float.Epsilon || actionStater.CurState != ACTION.WALK)
            {
                return;
            }
            actionStater.Go(ACTION.SLEEP);
        })).SetDuration(20.0f, LOCATION.TABLE);

        locationStater.AddState(LOCATION.WORK_PLACE).AddAction(StaterAction.ENTER(() =>
        {
            actionStater.Go(ACTION.WALK);
            agent.SetDestination(workTransform.position);
        })).AddAction(StaterAction.STEP(() =>
        {
            if (agent.remainingDistance > float.Epsilon || actionStater.CurState != ACTION.WALK)
            {
                return;
            }
            actionStater.Go(ACTION.WORK);
        })).SetDuration(15.0f, LOCATION.BED);

        locationStater.AddState(LOCATION.TABLE).AddAction(StaterAction.ENTER(() =>
        {
            actionStater.Go(ACTION.WALK);
            agent.SetDestination(eatTransform.position);
        })).AddAction(StaterAction.STEP(() =>
        {
            if (agent.remainingDistance > float.Epsilon || actionStater.CurState != ACTION.WALK)
            {
                return;
            }
            actionStater.Go(ACTION.EAT);
        })).SetDuration(12.0f, LOCATION.WORK_PLACE);
        #endregion

        #region action
        actionStater = new Stater<ACTION>("NpcAction", false);
        actionStater.AddState(ACTION.WALK).AddAction(StaterAction.ENTER(() => { Animator.SetBool("Walking", true); }))
            .AddAction(StaterAction.EXIT(() => { Animator.SetBool("Walking", false); }));

        actionStater.AddState(ACTION.EAT).AddAction(StaterAction.ENTER(() => { Animator.SetBool("Eating", true); }))
            .AddAction(StaterAction.EXIT(() => { Animator.SetBool("Eating", false); }))
            .SetDuration(5.0f, ACTION.WALK);

        actionStater.AddState(ACTION.SLEEP).AddAction(StaterAction.ENTER(() => { Animator.SetBool("Sleeping", true); sleepEffect.Play(); }))
            .AddAction(StaterAction.EXIT(() => { sleepEffect.Stop(); Animator.SetBool("Sleeping", false);  }))
            .SetDuration(10.0f, ACTION.WALK);

        actionStater.AddState(ACTION.WORK).AddAction(StaterAction.ENTER(() => { Animator.SetBool("Working", true); }))
            .AddAction(StaterAction.EXIT(() => { Animator.SetBool("Working", false); }))
            .SetDuration(8.0f, ACTION.WALK);
        #endregion

        actionStater.Go(ACTION.WALK);
        locationStater.Go(LOCATION.BED);
    }

    // Update is called once per frame
    void Update()
    {
        locationStater.Step(Time.deltaTime);
    }
}
