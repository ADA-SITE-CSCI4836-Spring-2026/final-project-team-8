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
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private Button mainMenuButton;
    public GameObject pauseMenu;

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        restartGameButton.onClick.AddListener(OnRestartGameClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Hide on start regardless of active state in scene
        pauseMenu.SetActive(false);
    }

    public void Show()
    {
        pauseMenu.SetActive(true);
    }

    public void Hide()
    {
        pauseMenu.SetActive(false);
    }

    private void OnContinueClicked() => PauseManager.Resume();

    private void OnRestartGameClicked()
    {
        Time.timeScale = 1f;
        PlayerPrefs.DeleteKey("PlayerAge");
        PlayerPrefs.Save();
        PauseManager.Resume();
        SceneLoader.Instance.ReloadCurrentScene();
    }

    private void OnMainMenuClicked()
    {
        SceneLoader.Instance.LoadScene("MainMenu");
        PauseManager.Resume();
    }
}
