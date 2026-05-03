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

    [Header("Kill Reward")]
    [SerializeField] protected float timeReward = 10f;  // seconds restored to player on kill

    [Header("Movement")]
    [SerializeField] protected float patrolSpeed = 2f;
    [SerializeField] protected float chaseSpeed  = 4f;

    // ── State ────────────────────────────────────────────────────────────────
    public enum AIState { Patrol, Chase, Attack }
    public AIState State { get; protected set; } = AIState.Patrol;

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
        ApplyStats();

        // Don't touch Agent properties here — agent may not be on NavMesh yet.
        // Properties are applied in Start() after the NavMesh is ready.
        Agent.updateRotation = false;
        Agent.angularSpeed   = 0f;
    }

    protected virtual void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerStats     = player.GetComponentInChildren<PlayerStats>();
            _playerTransform = player.transform;

            if (_playerStats == null)
                Debug.LogWarning($"[{name}] Found 'Player' tag but no PlayerStats component on it or its children.");
        }
        else
        {
            Debug.LogWarning($"[{name}] No GameObject tagged 'Player' found. Enemy AI disabled.");
            enabled = false;
            return;
        }

        // Apply speed/distance now — agent should be on NavMesh by Start()
        if (IsAgentReady)
        {
            Agent.speed            = patrolSpeed;
            Agent.stoppingDistance = attackRange * 0.9f;
        }
        else
        {
            Debug.LogWarning($"[{name}] NavMeshAgent is not on a NavMesh at Start. " +
                             "Place the enemy on baked NavMesh geometry or wait for runtime bake.");
        }
    }

    /// <summary>True when the NavMeshAgent is active, enabled, and placed on a NavMesh.</summary>
    private bool IsAgentReady => Agent != null && Agent.isActiveAndEnabled && Agent.isOnNavMesh;

    protected virtual void Update()
    {
        if (!IsAlive) return;
        if (!IsAgentReady) return;   // not on NavMesh yet — skip AI

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
        if (!IsAgentReady) return;

        switch (State)
        {
            case AIState.Patrol:
                OnPatrolTick();
                FaceMovementDirection();
                break;

            case AIState.Chase:
                Agent.SetDestination(_playerTransform.position);
                FaceMovementDirection();
                break;

            case AIState.Attack:
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
        if (IsAgentReady) { Agent.speed = chaseSpeed; Agent.isStopped = false; }
    }

    private void EnterAttack()
    {
        State = AIState.Attack;
        if (IsAgentReady) Agent.isStopped = true;
    }

    private void EnterPatrol()
    {
        State = AIState.Patrol;
        if (IsAgentReady) { Agent.speed = patrolSpeed; Agent.isStopped = false; }
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

        // Line-of-sight raycast — cast from eye height, ignore self
        Vector3 eyePos    = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = _playerTransform.position + Vector3.up * 1.0f;
        Vector3 dir       = (targetPos - eyePos).normalized;
        float   rayDist   = Vector3.Distance(eyePos, targetPos);

        // Use QueryTriggerInteraction.Ignore so trigger colliders don't block LOS
        RaycastHit[] hits = Physics.RaycastAll(eyePos, dir, rayDist,
                                               sightBlockMask,
                                               QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            // Skip self and own children
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            // Hit the player or a child of the player — can see them
            if (hit.transform == _playerTransform || hit.transform.IsChildOf(_playerTransform))
                return true;

            // Hit something else — line of sight blocked
            return false;
        }

        // No hits at all — clear line of sight (open air)
        return dist <= sightRange && angle <= sightAngle * 0.5f;
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

        if (_playerStats == null)
        {
            Debug.LogWarning($"[{name}] Cannot apply damage — PlayerStats reference is null.");
            yield break;
        }

        // Re-check range — player may have moved away during the animation
        if (IsAlive && Vector3.Distance(transform.position, _playerTransform.position) <= attackRange * 1.5f)
        {
            _playerStats.TakeHit(attackDamage);
            Debug.Log($"[{name}] Hit player for {attackDamage}s. Player time remaining: {_playerStats.TimeRemaining:F1}s");
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
        if (IsAgentReady) { Agent.isStopped = true; }
        Agent.enabled = false;
        Anim?.SetTrigger(DeadHash);
        EventBus.Publish(new EnemyDiedEvent(gameObject));

        // Restore time to player
        if (_playerStats != null && timeReward > 0f)
        {
            _playerStats.AddTime(timeReward);
            Debug.Log($"[{name}] Killed — restored {timeReward}s to player. Remaining: {_playerStats.TimeRemaining:F1}s");
        }

        // Disable collider immediately so player can't keep hitting it
        GetComponent<Collider>().enabled = false;

        // Fade out then destroy
        StartCoroutine(FadeOutAndDestroy(2.5f, 1.0f));
    }

    /// <summary>
    /// Waits <paramref name="delay"/> seconds (letting death animation play),
    /// then fades all Renderers out over <paramref name="fadeDuration"/> seconds,
    /// then destroys the GameObject.
    /// </summary>
    private System.Collections.IEnumerator FadeOutAndDestroy(float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);

        // Collect all renderers and switch their materials to fade mode
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Cache original colors and switch to transparent rendering mode
        var originalColors = new UnityEngine.Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
                SetMaterialFade(renderers[i].material);
            }
        }

        // Fade alpha from 1 → 0
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
                {
                    Color c = originalColors[i];
                    c.a = alpha;
                    renderers[i].material.color = c;
                }
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    /// <summary>Switches a Standard or URP/Lit material to transparent rendering mode at runtime.</summary>
    private static void SetMaterialFade(Material mat)
    {
        // URP Lit shader
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f);           // 0 = Opaque, 1 = Transparent
            mat.SetFloat("_Blend",   0f);           // Alpha blend
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
        }
        // Standard shader fallback
        else if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 2f);              // Fade mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }

    // ── Animator ──────────────────────────────────────────────────────────────

    private void UpdateAnimator()
    {
        if (Anim == null) return;
        float speed = (!IsAgentReady || Agent.isStopped) ? 0f : Agent.velocity.magnitude;
        Anim.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Smoothly rotates to face the current movement direction from the NavMeshAgent.</summary>
    private void FaceMovementDirection()
    {
        Vector3 velocity = Agent.velocity;
        velocity.y = 0f;
        if (velocity.sqrMagnitude < 0.01f) return;  // not moving, keep current rotation
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                               Quaternion.LookRotation(velocity.normalized),
                                               10f * Time.deltaTime);
    }

    /// <summary>Smoothly rotates to face a specific world position (used during attack).</summary>
    private void FaceTarget(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0f;
        if (dir == Vector3.zero) return;
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                               Quaternion.LookRotation(dir),
                                               10f * Time.deltaTime);
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
