using UnityEngine;

/// <summary>
/// Initializes the StateMachine with SabotagePatrol as the default state.
/// SabotagePatrol internally handles the Sabotage transition on its own.
/// </summary>
[RequireComponent(typeof(StateMachine))]
public class SabotageMonster : MonoBehaviour, IDamageable
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

    public bool ObjectAhead(float range, out GameObject obj)
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range, objectLayer))
        {
            obj = hit.collider.gameObject;
            return true;
        }
        obj = null;
        return false;
    }

    public void Damage(int damage, IDamageable.DamageType type)
    {
        // For now, just log the damage. You can expand this with health and death logic.
        Debug.Log($"[SabotageMonster] Took {damage} damage.");
    }
}
