using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AI moves to RoutineManager.MainFire and sabotages it.
/// Returns to SabotagePatrol when done.
///
/// Each phase is an Action delegate assigned to _tick.
/// Perform() just calls _tick() — no flag chains.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Sabotage : State
{
    [Header("Sabotage")]
    [SerializeField] float sabotageDuration = 3f;
    [SerializeField] float arrivalDistance = 1.5f;

    [Header("Object Attack")]
    [SerializeField] LayerMask objectLayer;
    [SerializeField] float attackRange = 1.2f;
    [SerializeField] float attackInterval = 0.5f;
    [SerializeField] int attackDamage = 10;

    [Header("Flee")]
    [SerializeField] float detectionRadius = 6f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Transform runPoint;
    [SerializeField] float fleeSpeed = 8f;

    NavMeshAgent _agent;
    Transform _target;
    GameObject _attackTarget;
    float _timer;
    float _normalSpeed;
    Action _tick;

    // ─── Unity ────────────────────────────────────────────────────

    void Awake() => _agent = GetComponent<NavMeshAgent>();

    // ─── State interface ──────────────────────────────────────────

    public override void Enter()
    {
        _timer = 0f;
        _normalSpeed = _agent.speed;

        Transform fire = ResolveMainFire();
        if (fire == null)
        {
            Debug.LogWarning("[Sabotage] MainFire not found. Returning to patrol.");
            machine.ChangeState(GetComponent<SabotagePatrol>());
            return;
        }

        _target = fire;
        BeginPhase(TickTravelling, stopAgent: false);
        _agent.SetDestination(_target.position);
        Debug.Log("[Sabotage] Moving to MainFire.");
    }

    public override void Perform() => _tick?.Invoke();

    public override void Exit()
    {
        _agent.speed = _normalSpeed;
        _agent.isStopped = false;
        Debug.Log("[Sabotage] Sabotage finished.");
    }

    // ─── Phase delegates ──────────────────────────────────────────

    void TickTravelling()
    {
        if (PlayerNearby()) { BeginFlee(); return; }
        if (ObjectAhead(out var obj)) { BeginAttack(obj); return; }
        if (ReachedDestination()) BeginPhase(TickSabotaging, stopAgent: true);
    }

    void TickSabotaging()
    {
        if (PlayerNearby()) { BeginFlee(); return; }

        _timer += Time.deltaTime;
        if (_timer >= sabotageDuration)
            machine.ChangeState(new SabotagePatrol());
    }

    void TickAttacking()
    {
        if (PlayerNearby()) { BeginFlee(); return; }

        if (_attackTarget == null)
        {
            ResumeToTarget();
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= attackInterval)
        {
            _timer = 0f;
            DamageTarget(_attackTarget);
        }
    }

    void TickFleeing()
    {
        if (ReachedDestination()) gameObject.SetActive(false);
    }

    // ─── Transitions ──────────────────────────────────────────────

    void BeginPhase(Action phase, bool stopAgent)
    {
        _tick = phase;
        _timer = 0f;
        _agent.isStopped = stopAgent;
    }

    void BeginAttack(GameObject obj)
    {
        _attackTarget = obj;
        BeginPhase(TickAttacking, stopAgent: true);
        Debug.Log($"[Sabotage] Attacking object: {obj.name}");
    }

    void BeginFlee()
    {
        BeginPhase(TickFleeing, stopAgent: false);
        _agent.speed = fleeSpeed;

        if (runPoint != null) _agent.SetDestination(runPoint.position);
        else gameObject.SetActive(false);
    }

    void ResumeToTarget()
    {
        BeginPhase(TickTravelling, stopAgent: false);
        if (_target != null) _agent.SetDestination(_target.position);
    }

    // ─── Pure queries ─────────────────────────────────────────────

    bool PlayerNearby() => Physics.CheckSphere(transform.position, detectionRadius, playerLayer);
    bool ReachedDestination() => !_agent.pathPending && _agent.remainingDistance <= arrivalDistance;

    bool ObjectAhead(out GameObject obj)
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, attackRange, objectLayer))
        {
            obj = hit.collider.gameObject;
            return true;
        }
        obj = null;
        return false;
    }

    // ─── Side-effect helpers ──────────────────────────────────────

    static Transform ResolveMainFire()
    {
        RoutineManager rm = FindFirstObjectByType<RoutineManager>();
        return rm?.MainFire;
    }

    void DamageTarget(GameObject obj)
    {
        if (obj.TryGetComponent<IDamageable>(out var d))
            d.Damage(attackDamage, IDamageable.DamageType.meele);
    }

    // ─── Gizmos ───────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
