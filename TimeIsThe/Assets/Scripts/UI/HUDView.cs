using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDView : UIView
{
    [Header("Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthText;

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
    }

    private void OnHealthChanged(PlayerHealthChangedEvent evt)
    {
        float ratio = evt.Max > 0f ? evt.Current / evt.Max : 0f;

        if (healthBar != null)
            healthBar.value = ratio;

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(evt.Current)} / {Mathf.CeilToInt(evt.Max)}";
    }
}
