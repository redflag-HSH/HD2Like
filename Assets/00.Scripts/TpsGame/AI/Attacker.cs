using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AI chases the player and attacks when in range.
/// Returns to idle/patrol when the player leaves detection range.
///
/// Each phase is an Action delegate assigned to _tick.
/// Perform() just calls _tick() — no flag chains.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Attacker : State
{
    [Header("Detection")]
    [SerializeField] float detectionRadius = 12f;
    [SerializeField] LayerMask playerLayer;

    [Header("Chase")]
    [SerializeField] float chaseSpeed = 5f;

    [Header("Attack")]
    [SerializeField] float attackRange    = 1.5f;
    [SerializeField] float attackInterval = 0.8f;
    [SerializeField] int   attackDamage   = 20;

    NavMeshAgent _agent;
    Transform    _player;
    float        _timer;
    float        _normalSpeed;
    Action       _tick;

    // ─── Unity ────────────────────────────────────────────────────

    void Awake() => _agent = GetComponent<NavMeshAgent>();

    // ─── State interface ──────────────────────────────────────────

    public override void Enter()
    {
        _timer       = 0f;
        _normalSpeed = _agent.speed;
        _agent.speed = chaseSpeed;

        _player = FindPlayer();
        if (_player == null)
        {
            Debug.LogWarning("[Attacker] No player found on Enter.");
            BeginPhase(TickIdle, stopAgent: true);
            return;
        }

        BeginPhase(TickChasing, stopAgent: false);
        Debug.Log("[Attacker] Chasing player.");
    }

    public override void Perform() => _tick?.Invoke();

    public override void Exit()
    {
        _agent.speed     = _normalSpeed;
        _agent.isStopped = false;
        _agent.ResetPath();
        Debug.Log("[Attacker] Exiting attack state.");
    }

    // ─── Phase delegates ──────────────────────────────────────────

    void TickIdle()
    {
        _player = FindPlayer();
        if (_player != null)
        {
            BeginPhase(TickChasing, stopAgent: false);
            _agent.speed = chaseSpeed;
        }
    }

    void TickChasing()
    {
        if (_player == null || !PlayerInRange(detectionRadius))
        {
            BeginPhase(TickIdle, stopAgent: true);
            Debug.Log("[Attacker] Lost player — idling.");
            return;
        }

        _agent.SetDestination(_player.position);

        if (PlayerInRange(attackRange))
            BeginPhase(TickAttacking, stopAgent: true);
    }

    void TickAttacking()
    {
        if (_player == null || !PlayerInRange(detectionRadius))
        {
            BeginPhase(TickIdle, stopAgent: true);
            return;
        }

        if (!PlayerInRange(attackRange))
        {
            BeginPhase(TickChasing, stopAgent: false);
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= attackInterval)
        {
            _timer = 0f;
            DamagePlayer();
        }
    }

    // ─── Transitions ──────────────────────────────────────────────

    void BeginPhase(Action phase, bool stopAgent)
    {
        _tick            = phase;
        _timer           = 0f;
        _agent.isStopped = stopAgent;
    }

    // ─── Pure queries ─────────────────────────────────────────────

    bool PlayerInRange(float radius) =>
        _player != null &&
        Vector3.Distance(transform.position, _player.position) <= radius;

    static Transform FindPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return null;
        return players[UnityEngine.Random.Range(0, players.Length)].transform;
    }

    // ─── Side-effect helpers ──────────────────────────────────────

    void DamagePlayer()
    {
        if (_player != null && _player.TryGetComponent<IDamageable>(out var d))
            d.Damage(attackDamage, IDamageable.DamageType.meele);
    }

    // ─── Gizmos ───────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
