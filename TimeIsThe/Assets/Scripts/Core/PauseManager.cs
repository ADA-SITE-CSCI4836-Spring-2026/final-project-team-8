using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Handles pause input and directly shows/hides the PauseView.
/// Assign the PauseView reference in the Inspector.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private PauseView pauseView;

    [Header("Blur Volume")]
    [SerializeField] private GameObject blurVolume;

    public static bool IsPaused { get; private set; }

    // Static reference so other scripts can call PauseManager.Instance.Pause() if needed
    public static PauseManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LockCursor();

        // Ensure pause menu starts hidden
        pauseView.Hide();
        blurVolume.SetActive(false);
    }

    private void Update()
    {
        if (WinManager.HasWon || GameOverManager.HasLost) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            TogglePause();
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        UnlockCursor();

        blurVolume.SetActive(true);
        pauseView.Show();
        Debug.Log("[PauseManager] Paused — showing pause menu.");

        EventBus.Publish(new GamePausedEvent(true));
    }

    public static void Resume()
    {
        if (Instance == null) return;
        Instance.ResumeInternal();
    }

    private void ResumeInternal()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        LockCursor();

        blurVolume.SetActive(false);
        pauseView.Hide();
        Debug.Log("[PauseManager] Resumed — hiding pause menu.");

        EventBus.Publish(new GamePausedEvent(false));

        // Flush input to prevent buffered actions from executing
        StartCoroutine(FlushInputNextFrame());
    }

    /// <summary>
    /// Waits one frame after unpausing to allow Unity's input system to clear.
    /// This prevents GetButtonDown/GetKeyDown from triggering on the unpause frame.
    /// </summary>
    private System.Collections.IEnumerator FlushInputNextFrame()
    {
        yield return null; // Wait one frame
        Input.ResetInputAxes();
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        UnlockCursor();
        Instance = null;
    }
}

public struct GamePausedEvent
{
    public bool IsPaused;
    public GamePausedEvent(bool paused) => IsPaused = paused;
}
