using UnityEngine;

/// <summary>
/// Initializes the StateMachine with SabotagePatrol as the default state.
/// SabotagePatrol internally handles the Sabotage transition on its own.
/// </summary>
[RequireComponent(typeof(StateMachine))]
public class SabotageMonster : MonoBehaviour
{
    [Header("Alert")]
    [SerializeField] float alertRadius = 6f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask objectLayer;
    [SerializeField] WaypointPath path;

    [SerializeField] Transform fleePoint;

    StateMachine _machine;

    // ─── Unity ────────────────────────────────────────────────────

    void Awake() => _machine = GetComponent<StateMachine>();

    void Start()
    {
        _machine.ChangeState(gameObject.AddComponent<SabotagePatrol>());
    }

    void Update()
    {

    }
    public WaypointPath GetWaypointPath() => path;
    public Transform GetFleePoint() => fleePoint;
    public LayerMask GetObjectLayer() => objectLayer;
}
