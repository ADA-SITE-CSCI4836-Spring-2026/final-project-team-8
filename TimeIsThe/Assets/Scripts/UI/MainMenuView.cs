using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : UIView
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    private void Awake()
    {
        playButton?.onClick.AddListener(OnPlayClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);
    }

    private void OnDestroy()
    {
        playButton?.onClick.RemoveListener(OnPlayClicked);
        quitButton?.onClick.RemoveListener(OnQuitClicked);
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
}
