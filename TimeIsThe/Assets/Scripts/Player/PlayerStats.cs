using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0f;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        EventBus.Publish(new PlayerHealthChangedEvent(CurrentHealth, maxHealth));

        if (!IsAlive)
            EventBus.Publish(new PlayerDiedEvent());
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        EventBus.Publish(new PlayerHealthChangedEvent(CurrentHealth, maxHealth));
    }

    public void SetMaxHealth(float newMax, bool refillHealth = false)
    {
        maxHealth = newMax;
        if (refillHealth) CurrentHealth = maxHealth;
    }
}

public struct PlayerHealthChangedEvent
{
    public float Current;
    public float Max;
    public PlayerHealthChangedEvent(float current, float max) { Current = current; Max = max; }
}

public struct PlayerDiedEvent { }
