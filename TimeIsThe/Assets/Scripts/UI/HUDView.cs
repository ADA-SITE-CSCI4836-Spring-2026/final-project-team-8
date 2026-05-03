using UnityEngine;
using TMPro;

/// <summary>
/// Displays the player's current age and remaining lives on the HUD.
/// The time bar is managed separately — this script only owns the age text.
///
/// Add this component to the HUD Canvas GameObject.
/// Assign the AgeText field in the Inspector.
/// </summary>
public class HUDView : MonoBehaviour
{
    [Header("Age")]
    [SerializeField] private TextMeshProUGUI ageText;  // e.g. "Age 30  (3 lives left)"

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
        // Pull current state immediately in case PlayerStats.Awake already
        // fired before this component subscribed
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats != null)
            RefreshAge(stats.Age);
    }

    private void OnTimeChanged(PlayerTimeChangedEvent evt)
    {
        RefreshAge(evt.Age);
    }

    private void RefreshAge(int age)
    {
        if (ageText == null) return;

        int deathsLeft = (PlayerStats.MAX_AGE - age) / PlayerStats.AGE_PER_DEATH;
        ageText.text = $"Age {age}  ({deathsLeft} {(deathsLeft == 1 ? "life" : "lives")} left)";
    }
}
