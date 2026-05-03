using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : UIView
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject instructionsPanel;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    private void Awake()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        instructionsButton.onClick.AddListener(OnInstructionsClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(OnPlayClicked);
        quitButton.onClick.RemoveListener(OnQuitClicked);
        instructionsButton.onClick.RemoveListener(OnInstructionsClicked);
        backButton.onClick.RemoveListener(OnBackClicked);
    }

    private void OnPlayClicked()
    {
        SceneLoader.Instance.LoadScene(gameSceneName);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnInstructionsClicked()
    {
        instructionsPanel.SetActive(true);
    }

    private void OnBackClicked()
    {
        instructionsPanel.SetActive(false);
    }
}
