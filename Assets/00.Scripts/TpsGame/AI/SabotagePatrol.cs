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
    [SerializeField] Transform[] waypoints;
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

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        _fleeing = false;
        _normalSpeed = _agent.speed;
        _agent.isStopped = false;

        if (waypoints == null || waypoints.Length == 0)
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
            if (!_agent.pathPending && _agent.remainingDistance <= waypointReachDistance)
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
            machine.ChangeState(new Sabotage());
            return;
        }

        if (waypoints == null || waypoints.Length == 0) return;

        if (!_agent.pathPending && _agent.remainingDistance <= waypointReachDistance)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Length;
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
        _agent.SetDestination(waypoints[_currentWaypointIndex].position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
