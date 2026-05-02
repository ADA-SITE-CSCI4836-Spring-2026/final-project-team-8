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

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        EventBus.Publish(new GameStateChangedEvent(newState));
    }
}

public struct GameStateChangedEvent
{
    public GameManager.GameState NewState;
    public GameStateChangedEvent(GameManager.GameState state) => NewState = state;
}
