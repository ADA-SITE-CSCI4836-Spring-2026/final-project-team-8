using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the Lose / Game Over canvas GameObject.
/// The canvas MUST start ACTIVE in the scene so Awake runs and subscribes.
/// </summary>
public class LoseView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI finalAgeText;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        restartButton?.onClick.AddListener(OnRestartClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        EventBus.Subscribe<PlayerFinalDeathEvent>(OnFinalDeath);

        SetVisible(false);
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
