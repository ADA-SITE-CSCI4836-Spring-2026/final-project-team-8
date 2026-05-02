using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDView : UIView
{
    [Header("Time (HP)")]
    [SerializeField] private Slider timeBar;
    [SerializeField] private TextMeshProUGUI timeText;   // e.g. "87s"

    [Header("Age")]
    [SerializeField] private TextMeshProUGUI ageText;    // e.g. "Age 23"

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerTimeChangedEvent>(OnTimeChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerTimeChangedEvent>(OnTimeChanged);
    }

    private void OnTimeChanged(PlayerTimeChangedEvent evt)
    {
        // Time bar
        if (timeBar != null)
            timeBar.value = evt.MaxTime > 0f ? evt.TimeRemaining / evt.MaxTime : 0f;

        // Time label — show whole seconds
        if (timeText != null)
            timeText.text = $"{Mathf.CeilToInt(evt.TimeRemaining)}s";

        // Age label
        if (ageText != null)
            ageText.text = $"Age {evt.Age}";
    }
}
