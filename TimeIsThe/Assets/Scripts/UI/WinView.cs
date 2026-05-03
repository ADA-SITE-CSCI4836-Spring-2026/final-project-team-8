using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the Win canvas GameObject.
/// The canvas MUST start ACTIVE in the scene so Awake runs.
/// Call WinView.Instance.TriggerWin() to show it.
/// </summary>
public class WinView : MonoBehaviour
{
    public static WinView Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI finalAgeText;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        Instance = this;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        playAgainButton?.onClick.AddListener(OnPlayAgainClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        playAgainButton?.onClick.RemoveListener(OnPlayAgainClicked);
        quitButton?.onClick.RemoveListener(OnQuitClicked);
    }

    public void TriggerWin()
    {
        PlayerStats stats = FindObjectOfType<PlayerStats>();

        if (winText != null)
            winText.text = "You survived!";

        if (finalAgeText != null && stats != null)
            finalAgeText.text = $"Final age: {stats.Age}";

        Time.timeScale       = 0f;
        Cursor.lockState     = CursorLockMode.None;
        Cursor.visible       = true;

        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        _canvasGroup.alpha          = visible ? 1f : 0f;
        _canvasGroup.interactable   = visible;
        _canvasGroup.blocksRaycasts = visible;
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
