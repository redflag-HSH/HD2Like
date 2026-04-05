using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Moves the enemy through a WaypointPath in order, looping indefinitely.
/// The enemy stops briefly at each waypoint before moving to the next.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Patrol : State
{
    [Header("Patrol Settings")]
    [SerializeField] WaypointPath waypointPath;
    [SerializeField] float patrolSpeed = 2.5f;
    [SerializeField] float waitAtWaypoint = 1.5f;
    [SerializeField] float arrivalDistance = 0.5f;

    [Header("Detection")]
    [SerializeField] float detectionRadius = 12f;

    NavMeshAgent _agent;
    LayerMask    _playerLayer;
    int   _waypointIndex;
    float _waitTimer;
    bool  _waiting;
    float _normalSpeed;

    void Awake()
    {
        _agent       = GetComponent<NavMeshAgent>();
        _playerLayer = GetComponent<AttackerMonster>().GetPlayerLayer();
    }

    // ─── State interface ──────────────────────────────────────────

    public override void Enter()
    {
        if (waypointPath == null || waypointPath.Count == 0)
        {
            Debug.LogWarning("[Patrol] No WaypointPath assigned or path is empty.");
            return;
        }

        _normalSpeed = _agent.speed;
        _agent.speed = patrolSpeed;
        _agent.isStopped = false;
        _waiting = false;
        _waitTimer = 0f;

        GoToCurrentWaypoint();
        Debug.Log("[Patrol] Entered patrol state.");
    }

    public override void Perform()
    {
        if (waypointPath == null || waypointPath.Count == 0) return;

        if (_waiting)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= waitAtWaypoint)
            {
                _waiting = false;
                AdvanceWaypoint();
                GoToCurrentWaypoint();
            }
            return;
        }

        Collider[] hits = new Collider[1];
        if (Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, hits, _playerLayer) > 0)
        {
            Attacker attacker = gameObject.AddComponent<Attacker>();
            attacker.SetTarget(hits[0].transform);
            machine.ChangeState(attacker);
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance <= arrivalDistance)
        {
            _agent.isStopped = true;
            _waiting = true;
            _waitTimer = 0f;
        }
    }

    public override void Exit()
    {
        _agent.speed = _normalSpeed;
        _agent.isStopped = false;
        _agent.ResetPath();
        Debug.Log("[Patrol] Exiting patrol state.");
    }

    // ─── Helpers ──────────────────────────────────────────────────

    void GoToCurrentWaypoint()
    {
        _agent.isStopped = false;
        _agent.SetDestination(waypointPath.GetWaypoint(_waypointIndex).position);
    }

    void AdvanceWaypoint() => _waypointIndex = (_waypointIndex + 1) % waypointPath.Count;

    // ─── Gizmos ───────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (waypointPath == null) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < waypointPath.Count; i++)
        {
            Transform wp = waypointPath.GetWaypoint(i);
            if (wp != null)
                Gizmos.DrawWireSphere(wp.position, arrivalDistance);
        }
    }
}
