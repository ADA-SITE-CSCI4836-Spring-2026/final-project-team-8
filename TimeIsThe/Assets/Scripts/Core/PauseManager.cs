using UnityEngine;

/// <summary>
/// Handles pause (Escape / P) and cursor lock.
///
/// - Locks and hides the cursor on start so mouse movement drives the camera
///   orbit instead of moving the OS cursor around the screen.
/// - Escape or P toggles pause: Time.timeScale = 0, cursor unlocked.
/// - Resumes: Time.timeScale = 1, cursor re-locked.
///
/// Add this to any persistent GameObject in the scene (e.g. GameManager's
/// GameObject, or a dedicated PauseManager object).
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    private void Start()
    {
        // Lock cursor immediately so the mouse orbits the camera
        // instead of moving freely over the screen
        LockCursor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            TogglePause();
    }

    public static void TogglePause()
    {
        if (IsPaused) Resume();
        else          Pause();
    }

    public static void Pause()
    {
        IsPaused          = true;
        Time.timeScale    = 0f;
        UnlockCursor();
        Debug.Log("[PauseManager] Publishing GamePausedEvent(true)");
        EventBus.Publish(new GamePausedEvent(true));
    }

    public static void Resume()
    {
        IsPaused          = false;
        Time.timeScale    = 1f;
        LockCursor();
        Debug.Log("[PauseManager] Publishing GamePausedEvent(false)");
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

    // Ensure timeScale and cursor are restored if the object is destroyed
    // (e.g. exiting Play mode while paused)
    private void OnDestroy()
    {
        Time.timeScale   = 1f;
        UnlockCursor();
    }
}

public struct GamePausedEvent
{
    public bool IsPaused;
    public GamePausedEvent(bool paused) => IsPaused = paused;
}
