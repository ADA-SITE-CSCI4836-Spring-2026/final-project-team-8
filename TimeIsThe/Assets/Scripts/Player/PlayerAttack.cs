using UnityEngine;

/// <summary>
/// Handles player attack input, animation trigger, and hit detection.
///
/// On Fire1:
///   1. Triggers the Attack animation
///   2. After a short delay (matching the animation's hit frame) performs an
///      overlap sphere in front of the player and calls TakeDamage() on any
///      EnemyBase found within range.
///
/// Damage value comes from PlayerStats.Damage, which scales with age.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Hit Detection")]
    [SerializeField] private float attackRadius = 1.5f;      // radius of the hit sphere
    [SerializeField] private float attackReach  = 1.2f;      // how far in front of player
    [SerializeField] private float hitDelay     = 0.3f;      // seconds after input to apply damage (match animation hit frame)
    [SerializeField] private LayerMask enemyMask = ~0;       // set to Enemy layer in Inspector

    private Animator    _animator;
    private PlayerStats _playerStats;
    private bool        _attackPending;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        _animator    = GetComponent<Animator>();
        _playerStats = GetComponent<PlayerStats>();

        if (_animator == null)
            Debug.LogError("[PlayerAttack] No Animator found on this GameObject.", this);

        if (_playerStats == null)
            Debug.LogError("[PlayerAttack] No PlayerStats found on this GameObject.", this);
    }

    private void Update()
    {
        // Ignore all input when paused
        if (PauseManager.IsPaused) return;

        if (Input.GetButtonDown("Fire1") && !_attackPending)
        {
            _animator?.SetTrigger(AttackHash);
            StartCoroutine(ApplyHitDelayed(hitDelay));
        }
    }

    private System.Collections.IEnumerator ApplyHitDelayed(float delay)
    {
        _attackPending = true;
        yield return new WaitForSeconds(delay);

        PerformHitDetection();
        _attackPending = false;
    }

    private void PerformHitDetection()
    {
        if (_playerStats == null) return;

        // Sphere in front of the player at chest height
        Vector3 origin = transform.position
                       + Vector3.up * 1.0f
                       + transform.forward * attackReach;

        Collider[] hits = Physics.OverlapSphere(origin, attackRadius, enemyMask);

        foreach (Collider hit in hits)
        {
            // Try on the hit object and its parents (enemy root may be above the collider)
            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy == null) continue;

            enemy.TakeDamage(_playerStats.Damage);
            Debug.Log($"[PlayerAttack] Hit {enemy.name} for {_playerStats.Damage:F1} dmg. Enemy HP: {enemy.CurrentHealth:F1}");
        }
    }

    // Draw the hit sphere in the Scene view for easy tuning
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Vector3 origin = transform.position
                       + Vector3.up * 1.0f
                       + transform.forward * attackReach;
        Gizmos.DrawSphere(origin, attackRadius);
    }
}
