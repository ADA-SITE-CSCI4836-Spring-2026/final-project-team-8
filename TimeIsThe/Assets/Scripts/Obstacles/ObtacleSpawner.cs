using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Canyon Terrain")]
    public GameObject canyonMesh;

    [Header("Prefabs")]
    public GameObject[] rockPrefabs;
    public GameObject[] treePrefabs;

    [Header("Spawn Settings")]
    public int rockCount = 60;
    public int treeCount = 20;
    public float raycastHeight = 200f;
    public LayerMask terrainLayer;

    [Header("Rock Scale")]
    public float rockMinScale = 0.3f;
    public float rockMaxScale = 2.5f;

    [Header("Tree Scale")]
    public float treeMinScale = 0.7f;
    public float treeMaxScale = 1.6f;

    [Header("Spacing")]
    public float minSpacingRocks = 1.5f;
    public float minSpacingTrees = 3.0f;

    // ── Enemy Spawning ────────────────────────────────────────────────────────

    [Header("Enemy Prefabs")]
    public GameObject skeletonPrefab;
    public GameObject golemPrefab;

    [Header("Enemy Counts")]
    public int skeletonCount = 5;
    public int golemCount    = 2;

    [Header("Enemy Spacing")]
    public float minSpacingEnemies   = 8f;   // enemies need more room than rocks
    public float minSpacingFromPlayer = 15f; // don't spawn enemies right on top of the player

    [Header("Waypoints")]
    [Tooltip("How many patrol waypoints to generate per enemy")]
    public int waypointsPerEnemy = 3;
    [Tooltip("Radius around the enemy spawn point to place waypoints")]
    public float waypointRadius  = 8f;

    // ── NavMesh ───────────────────────────────────────────────────────────────

    [Header("NavMesh")]
    [Tooltip("Assign the NavMeshSurface component that covers the canyon terrain")]
    public NavMeshSurface navMeshSurface;

    // ── Internal ──────────────────────────────────────────────────────────────

    private Transform    _playerTransform;
    private Bounds       _bounds;
    private EnemyTracker _enemyTracker;

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        if (canyonMesh == null)
        {
            Debug.LogError("Canyon Mesh is not assigned!");
            return;
        }

        MeshRenderer[] allRenderers = canyonMesh.GetComponentsInChildren<MeshRenderer>();
        if (allRenderers.Length == 0)
        {
            Debug.LogError("No MeshRenderers found on canyon or its children!");
            return;
        }

        _bounds = allRenderers[0].bounds;
        foreach (MeshRenderer r in allRenderers)
            _bounds.Encapsulate(r.bounds);

        MeshCollider mc = canyonMesh.GetComponentInChildren<MeshCollider>();
        if (mc == null)
        {
            Debug.LogError("No MeshCollider found on canyon or its children!");
            return;
        }

        // Cache player position so enemies don't spawn too close
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;

        // Get or add EnemyTracker
        _enemyTracker = GetComponent<EnemyTracker>();
        if (_enemyTracker == null)
            _enemyTracker = gameObject.AddComponent<EnemyTracker>();

        // 1 — Spawn obstacles first
        Debug.Log("--- Spawning Rocks ---");
        SpawnObjects(rockPrefabs, rockCount, rockMinScale, rockMaxScale, minSpacingRocks);

        Debug.Log("--- Spawning Trees ---");
        SpawnObjects(treePrefabs, treeCount, treeMinScale, treeMaxScale, minSpacingTrees);

        // Sync physics so freshly spawned obstacle colliders are visible to OverlapSphere
        Physics.SyncTransforms();

        // 2 — Bake NavMesh so enemies can path around the freshly spawned obstacles
        if (navMeshSurface == null)
        {
            navMeshSurface = canyonMesh.GetComponentInChildren<NavMeshSurface>();
            if (navMeshSurface == null)
                navMeshSurface = FindObjectOfType<NavMeshSurface>();
        }

        if (navMeshSurface == null)
        {
            navMeshSurface = canyonMesh.AddComponent<NavMeshSurface>();
            navMeshSurface.collectObjects = CollectObjects.All;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            Debug.Log("[ObstacleSpawner] No NavMeshSurface found — created one on canyon mesh.");
        }

        Debug.Log("--- Baking NavMesh ---");
        navMeshSurface.BuildNavMesh();
        Debug.Log("NavMesh baked.");

        // 3 — Spawn enemies after NavMesh is ready
        Debug.Log("--- Spawning Enemies ---");
        SpawnEnemies(skeletonPrefab, skeletonCount, "Skeletons");
        SpawnEnemies(golemPrefab,    golemCount,    "Golems");

        // 4 — Lock enemy count so tracker knows when all are dead
        _enemyTracker.FinalizeCount();
    }

    // ── Obstacle spawning (unchanged logic, refactored to use _bounds) ────────

    void SpawnObjects(GameObject[] prefabs, int count, float minScale, float maxScale, float minSpacing)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("Prefab array is empty, skipping.");
            return;
        }

        if (count == 0) return;

        int spawned     = 0;
        int attempts    = 0;
        int maxAttempts = count * 15;

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 candidate = GetRandomSurfacePoint();
            if (candidate == Vector3.zero) continue;

            if (IsTooClose(candidate, minSpacing, null)) continue;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject obj    = Instantiate(prefab, candidate, Quaternion.identity);
            obj.transform.rotation   = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            obj.transform.localScale *= Random.Range(minScale, maxScale);
            obj.transform.parent      = this.transform;

            spawned++;
        }

        Debug.Log($"Spawned {spawned}/{count} objects after {attempts} attempts");
    }

    // ── Enemy spawning ────────────────────────────────────────────────────────

    void SpawnEnemies(GameObject prefab, int count, string label)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[ObstacleSpawner] {label} prefab not assigned — skipping.");
            return;
        }

        if (count == 0) return;

        // Parent container to keep Hierarchy tidy
        GameObject container = new GameObject($"Enemies_{label}");
        container.transform.parent = this.transform;

        int spawned     = 0;
        int attempts    = 0;
        int maxAttempts = count * 20;

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 spawnPos = GetRandomNavMeshPoint();
            if (spawnPos == Vector3.zero) continue;

            // Keep away from other enemies
            if (IsTooClose(spawnPos, minSpacingEnemies, null)) continue;

            // Keep away from the player start position
            if (_playerTransform != null &&
                Vector3.Distance(spawnPos, _playerTransform.position) < minSpacingFromPlayer)
                continue;

            // Spawn enemy
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            enemy.transform.parent = container.transform;
            enemy.name = $"{label}_{spawned}";

            // Generate patrol waypoints around the spawn point
            Transform[] waypoints = GenerateWaypoints(spawnPos, waypointsPerEnemy, waypointRadius, enemy.transform);

            // Wire waypoints into the EnemyPatrol component
            EnemyPatrol patrol = enemy.GetComponent<EnemyPatrol>();
            if (patrol != null)
                patrol.SetWaypoints(waypoints);
            else
                Debug.LogWarning($"[ObstacleSpawner] {enemy.name} has no EnemyPatrol component.");

            // Register with tracker for win condition
            _enemyTracker.RegisterEnemy();

            spawned++;
        }

        Debug.Log($"[ObstacleSpawner] Spawned {spawned}/{count} {label} after {attempts} attempts");
    }

    // ── Waypoint generation ───────────────────────────────────────────────────

    Transform[] GenerateWaypoints(Vector3 center, int count, float radius, Transform parent)
    {
        GameObject waypointContainer = new GameObject("Waypoints");
        waypointContainer.transform.parent = parent.parent;

        List<Transform> waypoints = new List<Transform>();

        int attempts    = 0;
        int maxAttempts = count * 10;

        while (waypoints.Count < count && attempts < maxAttempts)
        {
            attempts++;

            // Distribute waypoints evenly around the circle with some randomness
            float angle  = (waypoints.Count / (float)count) * 360f + Random.Range(-30f, 30f);
            float dist   = Random.Range(radius * 0.4f, radius);
            Vector3 offset = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * dist,
                0f,
                Mathf.Cos(angle * Mathf.Deg2Rad) * dist
            );

            Vector3 candidate = center + offset;

            // Snap to NavMesh
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                GameObject wp = new GameObject($"WP_{waypoints.Count}");
                wp.transform.position = hit.position;
                wp.transform.parent   = waypointContainer.transform;
                waypoints.Add(wp.transform);
            }
        }

        return waypoints.ToArray();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Raycasts from above a random XZ position to find a surface point.</summary>
    Vector3 GetRandomSurfacePoint()
    {
        float   x         = Random.Range(_bounds.min.x, _bounds.max.x);
        float   z         = Random.Range(_bounds.min.z, _bounds.max.z);
        Vector3 rayOrigin = new Vector3(x, _bounds.max.y + raycastHeight, z);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            return hit.point;

        return Vector3.zero;
    }

    /// <summary>Finds a random point on the baked NavMesh within the canyon bounds.</summary>
    Vector3 GetRandomNavMeshPoint()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 surface = GetRandomSurfacePoint();
            if (surface == Vector3.zero) continue;

            // Use a generous sample radius for uneven canyon terrain
            if (NavMesh.SamplePosition(surface, out NavMeshHit hit, 8f, NavMesh.AllAreas))
                return hit.position;
        }

        Debug.LogWarning("[ObstacleSpawner] GetRandomNavMeshPoint failed — is the NavMesh baked?");
        return Vector3.zero;
    }

    /// <summary>Returns true if any existing collider is within minSpacing of point (ignoring terrain).</summary>
    bool IsTooClose(Vector3 point, float minSpacing, GameObject ignoreObj)
    {
        Collider[] nearby = Physics.OverlapSphere(point, minSpacing);
        foreach (Collider c in nearby)
        {
            if (ignoreObj != null && c.gameObject == ignoreObj) continue;

            // Ignore the canyon terrain itself — it covers the whole area
            if (c.gameObject == canyonMesh)                     continue;
            if (c.transform.IsChildOf(canyonMesh.transform))    continue;

            // Ignore trigger colliders (detection zones etc.)
            if (c.isTrigger)                                     continue;

            return true;
        }
        return false;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmos()
    {
        if (canyonMesh == null) return;

        MeshRenderer[] allRenderers = canyonMesh.GetComponentsInChildren<MeshRenderer>();
        if (allRenderers.Length == 0) return;

        Bounds b = allRenderers[0].bounds;
        foreach (MeshRenderer r in allRenderers)
            b.Encapsulate(r.bounds);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(b.center, b.size);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(b.center.x, b.max.y + raycastHeight, b.center.z), 1f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(b.center, 1f);
    }
}
