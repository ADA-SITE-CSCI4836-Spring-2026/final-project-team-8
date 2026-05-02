using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthBarGlow : MonoBehaviour
{
    [Header("References")]
    public Image fill;
    public Image glowInner;
    public Image glowOuter;
    public Image fillSheen;
    public TextMeshProUGUI label;

    [Header("Settings")]
    public float maxHealth = 100f;
    public float smoothSpeed = 3.5f;      // higher = snappier lerp
    public float sheenSpeed = 0.4f;       // how fast the sheen scrolls
    public bool showLabel = true;

    [Header("Glow Intensity")]
    public float glowInnerAlpha = 0.25f;
    public float glowOuterAlpha = 0.09f;

    [Header("Vignette")]
    public Image vignetteOverlay;         // full screen dark overlay Image
    public float vignetteMaxAlpha = 0.35f;

    [Header("Timer")]
    public float timeLimit = 60f;         // total seconds
    public bool timerRunning = false;

    public System.Action onTimerExpired;  // hook your game logic up here

    private float _targetFill  = 1f;
    private float _currentFill = 1f;
    private float _sheenOffset = 0f;
    private float _currentHealth;
    private float _timeRemaining;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _currentHealth = maxHealth;
        _currentFill   = 1f;
        _targetFill    = 1f;
        ApplyFill(1f);
        SetGlowActive(false);
    }

    void Start()
    {
        SetGlowActive(true);
        maxHealth      = timeLimit;
        _timeRemaining = timeLimit;
        SetHealth(timeLimit);
    }

    void Update()
    {
        // Tick the timer
        if (timerRunning)
        {
            _timeRemaining -= Time.deltaTime;
            _timeRemaining  = Mathf.Max(_timeRemaining, 0f);
            SetHealth(_timeRemaining);

            if (_timeRemaining <= 0f)
            {
                timerRunning = false;
                onTimerExpired?.Invoke();
            }
        }

        // Smooth lerp fill toward target
        _currentFill = Mathf.Lerp(_currentFill, _targetFill,
                                   smoothSpeed * Time.unscaledDeltaTime);

        // Snap when close enough to avoid infinite float creep
        if (Mathf.Abs(_currentFill - _targetFill) < 0.001f)
            _currentFill = _targetFill;

        ApplyFill(_currentFill);
        UpdateSheen();
        UpdateVignette();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void SetHealth(float newHealth)
    {
        _currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        _targetFill    = _currentHealth / maxHealth;

        if (showLabel && label != null)
        {
            int seconds = Mathf.CeilToInt(_currentHealth);
            int mins    = seconds / 60;
            int secs    = seconds % 60;
            label.text  = mins > 0 ? $"{mins}:{secs:00}" : $"{secs}s";
        }
    }

    public void SetMaxHealth(float newMax)
    {
        maxHealth = newMax;
        SetHealth(_currentHealth);
    }

    // ── Timer controls ────────────────────────────────────────────────────────

    public void StartTimer() => timerRunning = true;
    public void StopTimer()  => timerRunning = false;

    public void ResetTimer()
    {
        _timeRemaining = timeLimit;
        timerRunning   = false;
        SetHealth(timeLimit);
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    void ApplyFill(float amount)
    {
        // Main fill
        fill.fillAmount = amount;
        Color healthColor = GetHealthColor(amount);
        fill.color = healthColor;

        // Glow layers track fill amount
        glowInner.fillAmount = amount;
        glowOuter.fillAmount = amount;

        // Tint glow to match fill color, fade with health
        Color innerColor = healthColor;
        Color outerColor = healthColor;
        innerColor.a = glowInnerAlpha * amount;
        outerColor.a = glowOuterAlpha * amount;
        glowInner.color = innerColor;
        glowOuter.color = outerColor;

        // Sheen tracks fill too
        fillSheen.fillAmount = amount;
    }

    void UpdateSheen()
    {
        _sheenOffset = (_sheenOffset + sheenSpeed * Time.unscaledDeltaTime) % 1f;

        float sheenAlpha = 0.08f + 0.08f * Mathf.Sin(_sheenOffset * Mathf.PI * 2f);
        Color c = fillSheen.color;
        c.a = sheenAlpha * _currentFill;  // hide sheen when bar is nearly empty
        fillSheen.color = c;
    }

    void UpdateVignette()
    {
        if (vignetteOverlay == null) return;

        float fraction = _currentFill;
        if (fraction < 0.3f)
        {
            float pulse   = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3.5f);
            float urgency = 1f - (fraction / 0.3f);  // 0 at 30%, 1 at 0%
            Color c = vignetteOverlay.color;
            c.a = urgency * vignetteMaxAlpha * pulse;
            vignetteOverlay.color = c;
        }
        else
        {
            Color c = vignetteOverlay.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime);
            vignetteOverlay.color = c;
        }
    }

    // Smooth green → yellow → red across the full range
    Color GetHealthColor(float fraction)
    {
        Color green  = new Color(0.2f,  0.95f, 0.4f);
        Color yellow = new Color(0.95f, 0.85f, 0.1f);
        Color red    = new Color(0.95f, 0.15f, 0.15f);

        if (fraction > 0.5f)
            return Color.Lerp(yellow, green, (fraction - 0.5f) / 0.5f);
        else
            return Color.Lerp(red, yellow, fraction / 0.5f);
    }

    void SetGlowActive(bool active)
    {
        if (glowInner) glowInner.gameObject.SetActive(active);
        if (glowOuter) glowOuter.gameObject.SetActive(active);
    }
}