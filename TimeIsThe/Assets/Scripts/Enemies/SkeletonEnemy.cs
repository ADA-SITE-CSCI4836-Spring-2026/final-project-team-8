/// <summary>
/// Skeleton enemy — fast, low health, low damage.
/// Patrols waypoints and chases the player on sight.
///
/// Default stats (all overridable in Inspector):
///   Health       : 30
///   Attack damage: 8s  (time penalty)
///   Attack range : 1.5
///   Sight range  : 10
///   Patrol speed : 2.5
///   Chase speed  : 5
/// </summary>
public class SkeletonEnemy : EnemyPatrol
{
    protected override void ApplyStats()
    {
        maxHealth     = 30f;
        attackDamage  = 8f;
        attackRange   = 1.5f;
        attackCooldown = 1.2f;
        sightRange    = 10f;
        sightAngle    = 100f;
        patrolSpeed   = 2.5f;
        chaseSpeed    = 5f;
    }
}
