using UnityEngine;

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

        Bounds bounds = allRenderers[0].bounds;
        foreach (MeshRenderer r in allRenderers)
            bounds.Encapsulate(r.bounds);

        MeshCollider mc = canyonMesh.GetComponentInChildren<MeshCollider>();
        if (mc == null)
        {
            Debug.LogError("No MeshCollider found on canyon or its children!");
            return;
        }

        Debug.Log("--- Spawning Rocks ---");
        SpawnObjects(rockPrefabs, rockCount, bounds, rockMinScale, rockMaxScale, minSpacingRocks);

        Debug.Log("--- Spawning Trees ---");
        SpawnObjects(treePrefabs, treeCount, bounds, treeMinScale, treeMaxScale, minSpacingTrees);
    }

    void SpawnObjects(GameObject[] prefabs, int count, Bounds bounds, float minScale, float maxScale, float minSpacing)
    {
        if (prefabs.Length == 0)
        {
            Debug.LogWarning("Prefab array is empty, skipping.");
            return;
        }

        if (count == 0)
        {
            Debug.LogWarning("Count is 0, skipping.");
            return;
        }

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = count * 15;

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;

            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 rayOrigin = new Vector3(randomX, bounds.max.y + raycastHeight, randomZ);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                // Check spacing — skip if another object is too close
                bool tooClose = false;
                Collider[] nearby = Physics.OverlapSphere(hit.point, minSpacing);
                foreach (Collider c in nearby)
                {
                    if (c.gameObject != canyonMesh && c.gameObject != hit.collider.gameObject)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose) continue;

                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject obj = Instantiate(prefab, hit.point, Quaternion.identity);

                // Random Y rotation
                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                // Random scale
                float scale = Random.Range(minScale, maxScale);
                obj.transform.localScale *= scale;

                // Keep hierarchy tidy
                obj.transform.parent = this.transform;

                spawned++;
            }
        }

        Debug.Log($"Spawned {spawned}/{count} objects after {attempts} attempts");
    }

    void OnDrawGizmos()
    {
        if (canyonMesh == null) return;

        MeshRenderer[] allRenderers = canyonMesh.GetComponentsInChildren<MeshRenderer>();
        if (allRenderers.Length == 0) return;

        Bounds bounds = allRenderers[0].bounds;
        foreach (MeshRenderer r in allRenderers)
            bounds.Encapsulate(r.bounds);

        // Yellow box showing spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        // Red sphere showing raycast origin height
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(bounds.center.x, bounds.max.y + raycastHeight, bounds.center.z), 1f);

        // Green sphere showing canyon center
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(bounds.center, 1f);
    }
}