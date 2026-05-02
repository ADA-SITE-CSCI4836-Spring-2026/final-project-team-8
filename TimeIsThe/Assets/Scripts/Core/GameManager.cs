using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public enum GameState { Boot, MainMenu, Playing, Paused, GameOver }

    public GameState CurrentState { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        CurrentState = GameState.Boot;
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

    /// <summary>Player reached age 60 — game over.</summary>
    private void OnPlayerFinalDeath(PlayerFinalDeathEvent evt)
    {
        Debug.Log($"[GameManager] Game Over — player reached age {evt.FinalAge}.");
        SetState(GameState.GameOver);
    }
}

public struct GameStateChangedEvent
{
    public GameManager.GameState NewState;
    public GameStateChangedEvent(GameManager.GameState state) => NewState = state;
}
