using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AI patrols between waypoints.
/// If a player enters detectionRadius it flees to runPoint, then disappears.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class SabotagePatrol : State
{
    [Header("Patrol")]
    [SerializeField] WaypointPath waypointPath;
    [SerializeField] float waypointReachDistance = 0.5f;

    [Header("Flee")]
    [SerializeField] float detectionRadius = 6f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Transform runPoint;
    [SerializeField] float fleeSpeed = 8f;

    NavMeshAgent _agent;
    int _currentWaypointIndex;
    float _normalSpeed;
    bool _fleeing;
    Transform _fleeingPoint;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        _fleeingPoint = GetComponent<SabotageMonster>().GetFleePoint();
        waypointPath = GetComponent<SabotageMonster>().GetWaypointPath();
        _fleeing = false;
        _normalSpeed = _agent.speed;
        _agent.isStopped = false;

        if (waypointPath == null || waypointPath.Count == 0)
        {
            Debug.LogWarning("[SabotagePatrol] No waypoints assigned.");
            return;
        }

        MoveToCurrentWaypoint();
    }

    public override void Perform()
    {
        if (_fleeing)
        {
            if (_fleeingPoint != null &&
                Vector3.Distance(transform.position, _fleeingPoint.position) <= waypointReachDistance)
                gameObject.SetActive(false);
            return;
        }

        if (DetectPlayer())
        {
            StartFlee();
            return;
        }

        if (Random.value < 0.01f)
        {
            machine.ChangeState(gameObject.AddComponent<Sabotage>());
            return;
        }

        if (waypointPath == null || waypointPath.Count == 0) return;

        if (!_agent.pathPending && _agent.remainingDistance <= waypointReachDistance)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % waypointPath.Count;
            MoveToCurrentWaypoint();
        }
    }

    public override void Exit()
    {
        _agent.speed = _normalSpeed;
        _agent.isStopped = true;
        _agent.ResetPath();
    }

    bool DetectPlayer()
    {
        return Physics.CheckSphere(transform.position, detectionRadius, playerLayer);
    }

    void StartFlee()
    {
        _fleeing = true;
        _agent.speed = fleeSpeed;

        if (runPoint != null)
            _agent.SetDestination(runPoint.position);
        else
            gameObject.SetActive(false);
    }

    void MoveToCurrentWaypoint()
    {
        _agent.SetDestination(waypointPath.GetWaypoint(_currentWaypointIndex).position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
