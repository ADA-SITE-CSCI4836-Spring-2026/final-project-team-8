using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the Win canvas GameObject.
/// Call WinView.Instance.TriggerWin() from wherever your win condition is detected
/// (e.g. all enemies defeated, objective reached, etc.).
///
/// Wire up the PlayAgain and Quit buttons in the Inspector.
/// </summary>
public class WinView : UIView
{
    public static WinView Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI winText;       // e.g. "You survived!"
    [SerializeField] private TextMeshProUGUI finalAgeText;  // e.g. "Final age: 30"

    private void Awake()
    {
        Instance = this;

        playAgainButton?.onClick.AddListener(OnPlayAgainClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        // Start hidden
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        playAgainButton?.onClick.RemoveListener(OnPlayAgainClicked);
        quitButton?.onClick.RemoveListener(OnQuitClicked);
    }

    /// <summary>Call this when the player wins (all enemies dead, objective complete, etc.).</summary>
    public void TriggerWin()
    {
        PlayerStats stats = FindObjectOfType<PlayerStats>();

        if (winText != null)
            winText.text = "You survived!";

        if (finalAgeText != null && stats != null)
            finalAgeText.text = $"Final age: {stats.Age}";

        // Pause time so the scene freezes on the win screen
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        Show();
    }

    private void OnPlayAgainClicked()
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
