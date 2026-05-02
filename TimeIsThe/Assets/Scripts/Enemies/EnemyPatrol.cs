using UnityEngine;

public class EnemyPatrol : EnemyBase
{
    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waypointTolerance = 0.2f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask playerLayer;

    private int _currentWaypointIndex;
    private Transform _target;

    protected override void Awake()
    {
        base.Awake();
        _currentWaypointIndex = 0;
    }

    protected override void Tick()
    {
        DetectPlayer();

        if (_target != null)
            ChaseTarget();
        else
            Patrol();
    }

    private void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
        _target = hits.Length > 0 ? hits[0].transform : null;
    }

    private void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform destination = waypoints[_currentWaypointIndex];
        MoveTowards(destination.position);

        if (Vector3.Distance(transform.position, destination.position) <= waypointTolerance)
            _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Length;
    }

    private void ChaseTarget()
    {
        MoveTowards(_target.position);
    }

    private void MoveTowards(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(new Vector3(destination.x, transform.position.y, destination.z));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.15f);
            if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
