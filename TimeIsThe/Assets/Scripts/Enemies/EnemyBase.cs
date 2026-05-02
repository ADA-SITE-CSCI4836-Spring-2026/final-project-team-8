using UnityEngine;

/// <summary>
/// Abstract base class for all enemy types.
/// Inherit from this and override the abstract members to implement specific enemy behaviour.
///
/// Enemies deal damage by calling HitPlayer() which deducts a fixed time penalty
/// (PlayerStats.HIT_PENALTY) from the player's remaining time.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 50f;

    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0f;

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        if (!IsAlive) return;
        Tick();
    }

    /// <summary>Called every frame while the enemy is alive. Override to implement AI logic.</summary>
    protected abstract void Tick();

    public virtual void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        if (!IsAlive)
            OnDeath();
    }

    protected virtual void OnDeath()
    {
        EventBus.Publish(new EnemyDiedEvent(gameObject));
        Destroy(gameObject);
    }

    /// <summary>
    /// Call this when the enemy makes contact with the player.
    /// Deducts a fixed time penalty from PlayerStats.
    /// </summary>
    protected void HitPlayer(PlayerStats player)
    {
        player.TakeHit();
    }
}

public struct EnemyDiedEvent
{
    public GameObject Enemy;
    public EnemyDiedEvent(GameObject enemy) => Enemy = enemy;
}
