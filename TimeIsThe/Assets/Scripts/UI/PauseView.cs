using UnityEngine;
using UnityEngine.UI;

public class PauseView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    public GameObject pausePanel; // Assign in Inspector
    // private CanvasGroup _canvasGroup;

    private void Awake()
    {
        Debug.Log("[PauseView] Awake called");

        // _canvasGroup = GetComponent<CanvasGroup>();
        // if (_canvasGroup == null)
        //     _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        resumeButton?.onClick.AddListener(OnResumeClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        // EventBus.Subscribe<GamePausedEvent>(OnPauseChanged);
        Debug.Log("[PauseView] Subscribed to GamePausedEvent");

        SetVisible(false);
    }

    void Update() {
        // For testing: Toggle pause with the P key
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            bool isCurrentlyPaused = PauseManager.IsPaused;
            Debug.Log($"[PauseView] P key pressed. Current pause state: {isCurrentlyPaused}");
            if (isCurrentlyPaused)
                PauseManager.Resume();
            else
                PauseManager.Pause();
        }
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<GamePausedEvent>(OnPauseChanged);
    }

    private void OnPauseChanged(GamePausedEvent evt)
    {
        Debug.Log($"[PauseView] OnPauseChanged received. IsPaused={evt.IsPaused}");
        SetVisible(evt.IsPaused);
    }

    private void SetVisible(bool visible)
    {
        Debug.Log($"[PauseView] SetVisible({visible})");
        // _canvasGroup.alpha          = visible ? 1f : 0f;
        // _canvasGroup.interactable   = visible;
        // _canvasGroup.blocksRaycasts = visible;
        pausePanel.SetActive(visible);
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
