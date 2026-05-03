using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause menu view.
/// PauseManager holds a direct reference to this and calls Show()/Hide() directly.
/// This avoids any EventBus subscription timing issues with inactive GameObjects.
/// </summary>
public class PauseView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;
    public GameObject pauseMenu;

    private void Awake()
    {
        resumeButton?.onClick.AddListener(OnResumeClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        // Hide on start regardless of active state in scene
        pauseMenu.SetActive(false);
    }

    public void Show()
    {
        Debug.Log("[PauseView] Showing menu");
        pauseMenu.SetActive(true);
    }

    public void Hide()
    {
        Debug.Log("[PauseView] Hiding menu");
        pauseMenu.SetActive(false);
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
