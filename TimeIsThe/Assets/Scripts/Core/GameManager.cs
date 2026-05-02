using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public enum GameState { Boot, MainMenu, Playing, Paused, GameOver }

    public GameState CurrentState { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        CurrentState = GameState.Boot;

        // Clear persisted age on fresh boot so Editor play sessions always start at age 20.
        // In a real build this would only run on first launch — for now wiping on every boot
        // is the correct behaviour since there's no main menu "continue" flow yet.
        PlayerPrefs.DeleteKey("PlayerAge");
        PlayerPrefs.Save();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerAgedEvent>(OnPlayerAged);
        EventBus.Subscribe<PlayerFinalDeathEvent>(OnPlayerFinalDeath);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerAgedEvent>(OnPlayerAged);
        EventBus.Unsubscribe<PlayerFinalDeathEvent>(OnPlayerFinalDeath);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        EventBus.Publish(new GameStateChangedEvent(newState));
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    /// <summary>Player died but still has ages left — respawn at new age.</summary>
    private void OnPlayerAged(PlayerAgedEvent evt)
    {
        // Scene reload handles respawn; SceneLoader reloads the active scene.
        // Stats are already reset inside PlayerStats.ApplyAgeStats().
        Debug.Log($"[GameManager] Player aged to {evt.NewAge}. MaxTime={evt.NewMaxTime:F1}s  Damage={evt.NewDamage:F1}");
        SceneLoader.Instance.ReloadCurrentScene();
    }

    /// <summary>Player reached age 60 — game over, reload scene from scratch.</summary>
    private void OnPlayerFinalDeath(PlayerFinalDeathEvent evt)
    {
        Debug.Log($"[GameManager] Game Over — player reached age {evt.FinalAge}.");
        SetState(GameState.GameOver);

        // Small delay so any game over UI can show before reload
        StartCoroutine(ReloadAfterDelay(2.5f));
    }

    private System.Collections.IEnumerator ReloadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneLoader.Instance.ReloadCurrentScene();
    }
}

public struct GameStateChangedEvent
{
    public GameManager.GameState NewState;
    public GameStateChangedEvent(GameManager.GameState state) => NewState = state;
}
