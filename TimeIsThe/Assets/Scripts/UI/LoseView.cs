using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoseView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI finalAgeText;

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    private void Awake()
    {
        restartButton?.onClick.AddListener(OnRestartClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        EventBus.Subscribe<PlayerFinalDeathEvent>(OnFinalDeath);

        SetVisible(false);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayerFinalDeathEvent>(OnFinalDeath);
    }

    private void OnFinalDeath(PlayerFinalDeathEvent evt)
    {
        Debug.Log($"[LoseView] OnFinalDeath received. Age={evt.FinalAge}");

        if (finalAgeText != null)
            finalAgeText.text = $"You reached age {evt.FinalAge}.\nTime ran out.";

        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (panel != null)
            panel.SetActive(visible);
        else
            gameObject.SetActive(visible);
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
