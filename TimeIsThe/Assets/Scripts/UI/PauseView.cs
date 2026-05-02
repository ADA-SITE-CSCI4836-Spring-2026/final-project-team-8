using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the Pause Menu canvas GameObject.
/// The canvas MUST start ACTIVE in the scene so Awake runs and subscribes.
/// The script hides itself immediately in Awake.
/// </summary>
public class PauseView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // Use CanvasGroup alpha to hide instead of SetActive,
        // so this GameObject stays active and keeps receiving events
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        resumeButton?.onClick.AddListener(OnResumeClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        EventBus.Subscribe<GamePausedEvent>(OnPauseChanged);

        SetVisible(false);
    }

    private void OnDestroy()
    {
        resumeButton?.onClick.RemoveListener(OnResumeClicked);
        quitButton?.onClick.RemoveListener(OnQuitClicked);
        EventBus.Unsubscribe<GamePausedEvent>(OnPauseChanged);
    }

    private void OnPauseChanged(GamePausedEvent evt)
    {
        SetVisible(evt.IsPaused);
    }

    private void SetVisible(bool visible)
    {
        _canvasGroup.alpha          = visible ? 1f : 0f;
        _canvasGroup.interactable   = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    private void OnResumeClicked() => PauseManager.Resume();

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
