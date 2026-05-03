using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Attach to the Win canvas GameObject.
/// The canvas MUST start ACTIVE in the scene so Awake runs.
/// Call WinView.Instance.TriggerWin() to show it.
/// </summary>
public class WinView : MonoBehaviour
{
    public static WinView Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button startAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Blur")]
    [SerializeField] private PostProcessVolume blurVolume;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI finalAgeText;

    [SerializeField] private GameObject panel;

    private void Awake()
    {
        Instance = this;

        startAgainButton.onClick.AddListener(OnStartAgainClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        startAgainButton.onClick.RemoveListener(OnStartAgainClicked);
        mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }

    public void TriggerWin()
    {
        WinManager.HasWon = true;

        PlayerStats stats = FindObjectOfType<PlayerStats>();

        if (winText != null)
            winText.text = "You survived!";

        if (finalAgeText != null && stats != null)
            finalAgeText.text = $"Final age: {stats.Age}";

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (panel != null)
            panel.SetActive(visible);
        else
            gameObject.SetActive(visible);

        blurVolume.gameObject.SetActive(visible);
    }

    private void OnStartAgainClicked()
    {
        WinManager.HasWon = false;
        Time.timeScale = 1f;
        PlayerPrefs.DeleteKey("PlayerAge");
        PlayerPrefs.Save();
        SceneLoader.Instance.ReloadCurrentScene();
    }

    private void OnMainMenuClicked()
    {
        WinManager.HasWon = false;
        SceneLoader.Instance.LoadScene("MainMenu");
        PauseManager.Resume();
    }
}
