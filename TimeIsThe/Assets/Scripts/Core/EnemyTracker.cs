using UnityEngine;

/// <summary>
/// Tracks how many enemies are alive in the scene.
/// When all enemies are dead, triggers WinView.
///
/// ObstacleSpawner calls RegisterEnemy() for each enemy it spawns.
/// EnemyBase publishes EnemyDiedEvent on death which this class listens to.
///
/// Add this component to the same GameObject as ObstacleSpawner.
/// </summary>
public class EnemyTracker : MonoBehaviour
{
    private int _totalEnemies  = 0;
    private int _deadEnemies   = 0;
    private bool _winTriggered = false;

    private void OnEnable()
    {
        EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
    }

    /// <summary>Called by ObstacleSpawner once per spawned enemy.</summary>
    public void RegisterEnemy()
    {
        _totalEnemies++;
    }

    /// <summary>Call this after all enemies have been spawned to lock the total count.</summary>
    public void FinalizeCount()
    {
        Debug.Log($"[EnemyTracker] Total enemies registered: {_totalEnemies}");

        if (_totalEnemies == 0)
        {
            Debug.LogWarning("[EnemyTracker] No enemies registered — win condition will never fire.");
        }
    }

    private void OnEnemyDied(EnemyDiedEvent evt)
    {
        if (_winTriggered) return;

        _deadEnemies++;
        Debug.Log($"[EnemyTracker] Enemy died. {_deadEnemies}/{_totalEnemies} dead.");

        if (_totalEnemies > 0 && _deadEnemies >= _totalEnemies)
            TriggerWin();
    }

    private void TriggerWin()
    {
        _winTriggered = true;
        Debug.Log("[EnemyTracker] All enemies defeated — triggering win!");

        if (WinView.Instance != null)
            WinView.Instance.TriggerWin();
        else
            Debug.LogWarning("[EnemyTracker] WinView.Instance is null — is WinView in the scene?");
    }
}
