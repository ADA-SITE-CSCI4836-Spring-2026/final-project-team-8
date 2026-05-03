using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;

public class LoseView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Blur")]
    [SerializeField] private PostProcessVolume blurVolume;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI finalAgeText;

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    private void Awake()
    {
        startAgainButton.onClick.AddListener(OnStartAgainClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        EventBus.Subscribe<PlayerFinalDeathEvent>(OnFinalDeath);

        SetVisible(false);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayerFinalDeathEvent>(OnFinalDeath);
    }

    private void OnFinalDeath(PlayerFinalDeathEvent evt)
    {
        GameOverManager.HasLost = true;

        Debug.Log($"[LoseView] OnFinalDeath received. Age={evt.FinalAge}");

        if (finalAgeText != null)
            finalAgeText.text = $"You reached age {evt.FinalAge}.\nTime ran out.";

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
        GameOverManager.HasLost = false;
        Time.timeScale = 1f;
        PlayerPrefs.DeleteKey("PlayerAge");
        PlayerPrefs.Save();
        SceneLoader.Instance.ReloadCurrentScene();
    }

    private void OnMainMenuClicked()
    {
        GameOverManager.HasLost = false;
        SceneLoader.Instance.LoadScene("MainMenu");
        PauseManager.Resume();
    }
}
