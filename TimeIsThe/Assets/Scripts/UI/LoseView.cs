using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the Lose / Game Over canvas GameObject.
/// Shows when the player reaches age 60 (PlayerFinalDeathEvent).
///
/// Wire up the Restart and Quit buttons in the Inspector.
/// Optionally assign a TextMeshProUGUI for the final age display.
/// </summary>
public class LoseView : UIView
{
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI finalAgeText;  // e.g. "You reached age 60"

    private void Awake()
    {
        restartButton?.onClick.AddListener(OnRestartClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        // Subscribe in Awake — canvas starts inactive so OnEnable won't fire
        EventBus.Subscribe<PlayerFinalDeathEvent>(OnFinalDeath);

        // Start hidden
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        restartButton?.onClick.RemoveListener(OnRestartClicked);
        quitButton?.onClick.RemoveListener(OnQuitClicked);
        EventBus.Unsubscribe<PlayerFinalDeathEvent>(OnFinalDeath);
    }

    private void OnFinalDeath(PlayerFinalDeathEvent evt)
    {
        if (finalAgeText != null)
            finalAgeText.text = $"You reached age {evt.FinalAge}.\nTime ran out.";

        // Pause time so the scene doesn't reload underneath the menu
        Time.timeScale = 0f;

        // Unlock cursor so buttons are clickable
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        Show();
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        PlayerPrefs.DeleteKey("PlayerAge");
        PlayerPrefs.Save();
        SceneLoader.Instance.ReloadCurrentScene();
    }

    private void OnQuitClicked()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
