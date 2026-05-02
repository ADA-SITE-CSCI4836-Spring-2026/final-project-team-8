using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Adds waypoint patrol behaviour on top of EnemyBase.
/// When no player is detected the enemy walks between assigned waypoints via NavMesh.
/// If no waypoints are assigned it idles in place.
/// </summary>
public class EnemyPatrol : EnemyBase
{
    [Header("Patrol Waypoints")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointTolerance = 0.5f;

    private int _waypointIndex;

    /// <summary>Called by ObstacleSpawner to assign runtime-generated waypoints.</summary>
    public void SetWaypoints(Transform[] generatedWaypoints)
    {
        waypoints      = generatedWaypoints;
        _waypointIndex = 0;
        // Start heading to first waypoint immediately if already in patrol state
        if (State == AIState.Patrol)
            SetNextWaypoint();
    }

    protected override void OnPatrolEnter()
    {
        SetNextWaypoint();
    }

    protected override void OnPatrolTick()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Arrived at waypoint?
        if (!Agent.pathPending && Agent.remainingDistance <= waypointTolerance)
        {
            _waypointIndex = (_waypointIndex + 1) % waypoints.Length;
            SetNextWaypoint();
        }
    }

    private void SetNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Agent.SetDestination(waypoints[_waypointIndex].position);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}
