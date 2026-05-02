using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the Pause Menu canvas GameObject.
/// Listens to GamePausedEvent and shows/hides itself automatically.
///
/// Wire up the Resume and Quit buttons in the Inspector.
/// </summary>
public class PauseView : UIView
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        resumeButton?.onClick.AddListener(OnResumeClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        // Subscribe here (not OnEnable) because the canvas starts inactive
        // and OnEnable won't fire until the first Show() call
        EventBus.Subscribe<GamePausedEvent>(OnPauseChanged);

        // Start hidden
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        resumeButton?.onClick.RemoveListener(OnResumeClicked);
        quitButton?.onClick.RemoveListener(OnQuitClicked);
        EventBus.Unsubscribe<GamePausedEvent>(OnPauseChanged);
    }

    private void OnPauseChanged(GamePausedEvent evt)
    {
        if (evt.IsPaused) Show();
        else              Hide();
    }

    private void OnResumeClicked()
    {
        PauseManager.Resume();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
