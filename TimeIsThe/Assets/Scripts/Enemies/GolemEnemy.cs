/// <summary>
/// Giant Golem enemy — slow, high health, high damage, wide sight.
/// Patrols waypoints and chases the player on sight.
///
/// Default stats (all overridable in Inspector):
///   Health       : 200
///   Attack damage: 25s  (time penalty — hits hard)
///   Attack range : 2.5  (long arms)
///   Sight range  : 14
///   Patrol speed : 1.2
///   Chase speed  : 2.5
/// </summary>
public class GolemEnemy : EnemyPatrol
{
    protected override void ApplyStats()
    {
        maxHealth       = 200f;
        attackDamage    = 25f;
        attackRange     = 2.5f;
        attackCooldown  = 2.5f;
        sightRange      = 14f;
        sightAngle      = 120f;
        loseTargetRange = 25f;
        patrolSpeed     = 0.8f;
        chaseSpeed      = 1f;
        timeReward      = 30f;  // golem gives 30s — high reward, hard enemy
    }
}
