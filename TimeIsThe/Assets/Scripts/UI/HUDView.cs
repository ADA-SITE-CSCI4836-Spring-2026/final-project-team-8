using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Always-visible HUD — do NOT manage this through UIManager.Show/Hide.
/// Place it in the scene active from the start so OnEnable fires immediately.
/// </summary>
public class HUDView : MonoBehaviour
{
    [Header("Time (HP)")]
    [SerializeField] private Slider timeBar;
    [SerializeField] private TextMeshProUGUI timeText;   // e.g. "87s"

    [Header("Age")]
    [SerializeField] private TextMeshProUGUI ageText;    // e.g. "Age 23"

    private void Awake()
    {
        // Ensure slider is configured for 0-1 normalised value
        if (timeBar != null)
        {
            timeBar.minValue = 0f;
            timeBar.maxValue = 1f;
            timeBar.value    = 1f;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerTimeChangedEvent>(OnTimeChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerTimeChangedEvent>(OnTimeChanged);
    }

    private void Start()
    {
        // Pull current state immediately in case events already fired before
        // this component enabled (e.g. PlayerStats Awake ran first)
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats != null)
            Refresh(stats.TimeRemaining, stats.MaxTime, stats.Age);
    }

    private void OnTimeChanged(PlayerTimeChangedEvent evt)
    {
        Refresh(evt.TimeRemaining, evt.MaxTime, evt.Age);
    }

    private void Refresh(float remaining, float max, int age)
    {
        if (timeBar != null)
            timeBar.value = max > 0f ? remaining / max : 0f;

        if (timeText != null)
            timeText.text = $"{Mathf.CeilToInt(remaining)}s";

        if (ageText != null)
        {
            // Show age and how many deaths remain before game over
            int deathsLeft = (PlayerStats.MAX_AGE - age) / PlayerStats.AGE_PER_DEATH;
            ageText.text = $"Age {age}  ({deathsLeft} {(deathsLeft == 1 ? "life" : "lives")} left)";
        }
    }
}
