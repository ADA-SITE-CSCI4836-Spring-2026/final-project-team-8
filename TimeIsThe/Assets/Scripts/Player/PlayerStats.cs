using UnityEngine;

/// <summary>
/// Owns all age/time-as-health mechanics.
///
/// Age range   : MIN_AGE (20) → MAX_AGE (60)
/// Time (HP)   : lerps from START_TIME (120s) down to END_TIME (30s) as age increases
/// Damage dealt: lerps from MIN_DAMAGE up to MAX_DAMAGE as age increases
/// On time = 0 : age++ → respawn (unless age reached MAX_AGE → game over)
/// </summary>
public class PlayerStats : MonoBehaviour
{
    // ── Age constants ────────────────────────────────────────────────────────
    public const int MIN_AGE    = 20;
    public const int MAX_AGE    = 60;
    public const int AGE_PER_DEATH = 10;  // age increase on each death

    // ── Time-as-health constants ─────────────────────────────────────────────
    public const float START_TIME  = 120f;   // seconds at MIN_AGE
    public const float END_TIME    =  30f;   // seconds at MAX_AGE
    public const float HIT_PENALTY =  10f;   // seconds lost per hit

    // ── Damage constants ─────────────────────────────────────────────────────
    public const float MIN_DAMAGE = 10f;     // damage at MIN_AGE
    public const float MAX_DAMAGE = 40f;     // damage at MAX_AGE

    // ── Runtime state ────────────────────────────────────────────────────────
    public int   Age            { get; private set; }
    public float TimeRemaining  { get; private set; }
    public float MaxTime        { get; private set; }
    public float Damage         { get; private set; }
    public bool  IsAlive        => TimeRemaining > 0f;

    /// <summary>True during a dash — all incoming damage is ignored.</summary>
    public bool  IsInvincible   { get; private set; }

    // ── Invincibility API ─────────────────────────────────────────────────────

    /// <summary>Called by PlayerDash to grant invincibility frames.</summary>
    public void SetInvincible(bool invincible)
    {
        IsInvincible = invincible;
        EventBus.Publish(new PlayerInvincibilityChangedEvent(invincible));
    }

    // ── Age progress [0,1] ───────────────────────────────────────────────────
    private float AgeT => Mathf.InverseLerp(MIN_AGE, MAX_AGE, Age);

    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Load persisted age so it survives scene reloads
        // PlayerPrefs key "PlayerAge" is written on death and cleared on game over
        Age = PlayerPrefs.GetInt("PlayerAge", MIN_AGE);
        Age = Mathf.Clamp(Age, MIN_AGE, MAX_AGE);
        ApplyAgeStats();

        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;
    }

    private void Update()
    {
        if (!IsAlive) return;

        TickTime(Time.deltaTime);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Called by enemies when they hit the player. Amount defaults to HIT_PENALTY.</summary>
    public void TakeHit(float amount = HIT_PENALTY)
    {
        if (!IsAlive || IsInvincible) return;
        DeductTime(amount);
    }

    /// <summary>Add time back (power-up, etc.).</summary>
    public void AddTime(float seconds)
    {
        if (!IsAlive) return;
        TimeRemaining = Mathf.Min(MaxTime, TimeRemaining + seconds);
        PublishTimeChanged();
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void TickTime(float delta)
    {
        DeductTime(delta);
    }

    private void DeductTime(float amount)
    {
        TimeRemaining = Mathf.Max(0f, TimeRemaining - amount);
        PublishTimeChanged();

        if (TimeRemaining <= 0f)
            HandleDeath();
    }

    private void HandleDeath()
    {
        Age += AGE_PER_DEATH;

        if (Age >= MAX_AGE)
        {
            Age = MAX_AGE;
            // Clear saved age so next game starts fresh
            PlayerPrefs.DeleteKey("PlayerAge");
            PlayerPrefs.Save();
            EventBus.Publish(new PlayerFinalDeathEvent(Age));
            return;
        }

        PlayerPrefs.SetInt("PlayerAge", Age);
        PlayerPrefs.Save();

        ApplyAgeStats();
        TeleportToSpawn();
        EventBus.Publish(new PlayerAgedEvent(Age, MaxTime, Damage));
    }

    /// <summary>Recalculates MaxTime and Damage from current Age via lerp.</summary>
    private void ApplyAgeStats()
    {
        float t = AgeT;
        MaxTime       = Mathf.Lerp(START_TIME, END_TIME,    t);
        Damage        = Mathf.Lerp(MIN_DAMAGE,  MAX_DAMAGE, t);
        TimeRemaining = MaxTime;
    }

    private void TeleportToSpawn()
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = _spawnPosition;
        transform.rotation = _spawnRotation;
        if (cc != null) cc.enabled = true;
    }

    private void PublishTimeChanged()
    {
        EventBus.Publish(new PlayerTimeChangedEvent(TimeRemaining, MaxTime, Age));
    }
}

// ── Events ────────────────────────────────────────────────────────────────────

/// <summary>Fired every frame/hit when remaining time changes.</summary>
public struct PlayerTimeChangedEvent
{
    public float TimeRemaining;
    public float MaxTime;
    public int   Age;
    public PlayerTimeChangedEvent(float remaining, float max, int age)
    {
        TimeRemaining = remaining;
        MaxTime       = max;
        Age           = age;
    }
}

/// <summary>Fired when the player runs out of time but still has lives left (age &lt; 60).</summary>
public struct PlayerAgedEvent
{
    public int   NewAge;
    public float NewMaxTime;
    public float NewDamage;
    public PlayerAgedEvent(int age, float maxTime, float damage)
    {
        NewAge     = age;
        NewMaxTime = maxTime;
        NewDamage  = damage;
    }
}

/// <summary>Fired when the player reaches MAX_AGE — triggers game over.</summary>
public struct PlayerFinalDeathEvent
{
    public int FinalAge;
    public PlayerFinalDeathEvent(int age) => FinalAge = age;
}

// Kept for backward compatibility with any existing subscribers
public struct PlayerDiedEvent { }

public struct PlayerInvincibilityChangedEvent
{
    public bool IsInvincible;
    public PlayerInvincibilityChangedEvent(bool invincible) => IsInvincible = invincible;
}
