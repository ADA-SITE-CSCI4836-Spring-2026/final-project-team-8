using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Base class for all enemies. Handles:
///   - NavMeshAgent movement
///   - Line-of-sight detection
///   - Chase / patrol state machine
///   - Melee attack with cooldown
///   - Per-enemy configurable damage (set in subclass defaults or Inspector)
///
/// Subclasses override DefaultStats() to provide their type-specific values.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public abstract class EnemyBase : MonoBehaviour
{
    // ── Inspector (overridable per-instance) ─────────────────────────────────
    [Header("Health")]
    [SerializeField] protected float maxHealth = 50f;

    [Header("Detection")]
    [SerializeField] protected float sightRange      = 12f;
    [SerializeField] protected float sightAngle      = 90f;
    [SerializeField] protected float loseTargetRange = 18f;
    [SerializeField] protected LayerMask sightBlockMask = ~0;

    [Header("Attack")]
    [SerializeField] protected float attackRange    = 1.8f;
    [SerializeField] protected float attackDamage   = 10f;
    [SerializeField] protected float attackCooldown = 1.5f;

    [Header("Movement")]
    [SerializeField] protected float patrolSpeed = 2f;
    [SerializeField] protected float chaseSpeed  = 4f;

    // ── State ────────────────────────────────────────────────────────────────
    public enum AIState { Patrol, Chase, Attack }
    public AIState State { get; private set; } = AIState.Patrol;

    public float CurrentHealth { get; private set; }
    public bool  IsAlive       => CurrentHealth > 0f;

    // ── Internal refs ────────────────────────────────────────────────────────
    protected NavMeshAgent  Agent  { get; private set; }
    protected Animator      Anim   { get; private set; }

    private PlayerStats  _playerStats;
    private Transform    _playerTransform;
    private float        _attackTimer;

    // Animator hashes
    private static readonly int SpeedHash  = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DeadHash   = Animator.StringToHash("Dead");

    // ─────────────────────────────────────────────────────────────────────────

    protected virtual void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim  = GetComponent<Animator>();

        CurrentHealth = maxHealth;

        // Apply subclass defaults (can still be overridden in Inspector)
        ApplyStats();

        Agent.speed = patrolSpeed;
        Agent.stoppingDistance = attackRange * 0.9f;
    }

    protected virtual void Start()
    {
        // Find player at start — works even if player is DontDestroyOnLoad
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerStats     = player.GetComponent<PlayerStats>();
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning($"[{name}] No GameObject tagged 'Player' found. Enemy AI disabled.");
            enabled = false;
        }
    }

    protected virtual void Update()
    {
        if (!IsAlive) return;

        _attackTimer -= Time.deltaTime;

        UpdateState();
        ExecuteState();
        UpdateAnimator();
    }

    // ── State machine ─────────────────────────────────────────────────────────

    private void UpdateState()
    {
        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        switch (State)
        {
            case AIState.Patrol:
                if (CanSeePlayer(dist))
                    EnterChase();
                break;

            case AIState.Chase:
                if (dist <= attackRange)
                    EnterAttack();
                else if (!CanSeePlayer(dist) && dist > loseTargetRange)
                    EnterPatrol();
                break;

            case AIState.Attack:
                if (dist > attackRange * 1.2f)   // small buffer to avoid flickering
                    EnterChase();
                break;
        }
    }

    private void ExecuteState()
    {
        switch (State)
        {
            case AIState.Patrol:
                OnPatrolTick();
                break;

            case AIState.Chase:
                Agent.SetDestination(_playerTransform.position);
                break;

            case AIState.Attack:
                // Face the player, don't move
                Agent.SetDestination(transform.position);
                FaceTarget(_playerTransform.position);

                if (_attackTimer <= 0f)
                    PerformAttack();
                break;
        }
    }

    // ── State transitions ─────────────────────────────────────────────────────

    private void EnterChase()
    {
        State = AIState.Chase;
        Agent.speed = chaseSpeed;
        Agent.isStopped = false;
    }

    private void EnterAttack()
    {
        State = AIState.Attack;
        Agent.isStopped = true;
    }

    private void EnterPatrol()
    {
        State = AIState.Patrol;
        Agent.speed = patrolSpeed;
        Agent.isStopped = false;
        OnPatrolEnter();
    }

    // ── Detection ─────────────────────────────────────────────────────────────

    private bool CanSeePlayer(float dist)
    {
        if (dist > sightRange) return false;

        // Angle check
        Vector3 dirToPlayer = (_playerTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > sightAngle * 0.5f) return false;

        // Line-of-sight raycast (aim at chest height)
        Vector3 eyePos    = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = _playerTransform.position + Vector3.up * 1.0f;

        if (Physics.Raycast(eyePos, (targetPos - eyePos).normalized, out RaycastHit hit,
                            sightRange, sightBlockMask))
        {
            return hit.transform.IsChildOf(_playerTransform) || hit.transform == _playerTransform;
        }

        return false;
    }

    // ── Attack ────────────────────────────────────────────────────────────────

    private void PerformAttack()
    {
        _attackTimer = attackCooldown;
        Anim?.SetTrigger(AttackHash);

        // Damage is applied via animation event OR directly here.
        // Using direct application with a small delay via coroutine for hit-feel.
        StartCoroutine(ApplyDamageDelayed(0.3f));
    }

    private System.Collections.IEnumerator ApplyDamageDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Re-check range — player may have moved away during the animation
        if (IsAlive && _playerStats != null &&
            Vector3.Distance(transform.position, _playerTransform.position) <= attackRange * 1.5f)
        {
            _playerStats.TakeHit(attackDamage);
        }
    }

    // ── Health ────────────────────────────────────────────────────────────────

    public virtual void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        if (!IsAlive)
            OnDeath();
    }

    protected virtual void OnDeath()
    {
        Agent.isStopped = true;
        Anim?.SetTrigger(DeadHash);
        EventBus.Publish(new EnemyDiedEvent(gameObject));

        // Disable collider so player can't keep hitting it
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 3f);
    }

    // ── Animator ──────────────────────────────────────────────────────────────

    private void UpdateAnimator()
    {
        if (Anim == null) return;
        float speed = Agent.isStopped ? 0f : Agent.velocity.magnitude;
        Anim.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void FaceTarget(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0f;
        if (dir == Vector3.zero) return;
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                               Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }

    // ── Overridable patrol hooks ──────────────────────────────────────────────

    /// <summary>Called once when entering patrol state.</summary>
    protected virtual void OnPatrolEnter() { }

    /// <summary>Called every frame during patrol state. Override to implement patrol logic.</summary>
    protected virtual void OnPatrolTick() { }

    /// <summary>
    /// Called in Awake. Override in subclasses to set default stat values
    /// before the Inspector values are applied.
    /// </summary>
    protected virtual void ApplyStats() { }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    protected virtual void OnDrawGizmosSelected()
    {
        // Sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Lose target range
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);

        // FOV lines
        Gizmos.color = Color.cyan;
        Vector3 leftBound  = Quaternion.Euler(0, -sightAngle * 0.5f, 0) * transform.forward * sightRange;
        Vector3 rightBound = Quaternion.Euler(0,  sightAngle * 0.5f, 0) * transform.forward * sightRange;
        Gizmos.DrawRay(transform.position, leftBound);
        Gizmos.DrawRay(transform.position, rightBound);
    }
}

// ── Shared events ─────────────────────────────────────────────────────────────

public struct EnemyDiedEvent
{
    public GameObject Enemy;
    public EnemyDiedEvent(GameObject enemy) => Enemy = enemy;
}
