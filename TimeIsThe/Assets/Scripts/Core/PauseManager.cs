using UnityEngine;

/// <summary>
/// Handles pause input and directly shows/hides the PauseView.
/// Assign the PauseView reference in the Inspector.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private PauseView pauseView;

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
        pauseView?.Hide();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            TogglePause();
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else          Pause();
    }

    public void Pause()
    {
        IsPaused       = true;
        Time.timeScale = 0f;
        UnlockCursor();

        pauseView?.Show();
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
        IsPaused       = false;
        Time.timeScale = 1f;
        LockCursor();

        pauseView?.Hide();
        Debug.Log("[PauseManager] Resumed — hiding pause menu.");

        EventBus.Publish(new GamePausedEvent(false));
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
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
