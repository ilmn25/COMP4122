using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Resources.Scripts;
using Random = UnityEngine.Random;

public class NetworkSpawner : NetworkBehaviour
{

    [Header("Spawn Database")]
    public SpawnDatabase spawnDatabase;

    private GameObject prefab;
    private int minCount;
    private int maxCount;
    private List<Vector3> spawnPoints;

    [Header("Spawn Bound")]
    public BoxCollider2D spawnArea;

    [Header("Placement Constraints")]
    public float minDistanceBetween = 1f;
    public float minDistanceFromPlayers = 2f;

    [Header("Collision Handling")]
    public Vector2 spawnTestBoxSize = new Vector2(0.6f, 0.6f);
    public LayerMask obstacleLayer; // for walls
    public int overlapBufferSize = 16; // increased pickable distance
    private Collider2D[] _overlapBuffer;

    public int maxAttempts = 20;

    void Awake()
    {
        _overlapBuffer = new Collider2D[overlapBufferSize];
    }
    

    // Listen for start game event from UI
    void OnEnable() => UI.onStartPressed += SpawnAll;
    void OnDisable() => UI.onStartPressed -= SpawnAll;

    void SpawnAll(){

        if (!IsServer) return;
        if (spawnDatabase == null || spawnDatabase.entries == null) return;

        foreach (var prefab in spawnDatabase.entries)
        {
            if (prefab == null) continue;
            var spawnable = prefab.GetComponent<Spawnable>();
            if (spawnable == null) continue;
            SpawnObjects(prefab, spawnable);
        }
    }

    void SpawnObjects(GameObject prefab, Spawnable spawnable)
    {
        List<Vector3> spawnPoints = spawnable.spawnPoints;
        int min = Mathf.Max(0, spawnable.minCount); // for safety
        int max = Mathf.Max(min, spawnable.maxCount);
        int count = Random.Range(min, max + 1);
        if (count == 0) return;

        List<Vector3> spawned = new List<Vector3>();

        float minSqr = minDistanceBetween * minDistanceBetween;
        float minPlayerSqr = minDistanceFromPlayers * minDistanceFromPlayers;

        // 1. If inspector spawn points exist
        if (spawnPoints != null && spawnPoints.Count > 0 )
        {
            int num = Mathf.Min(count, spawnPoints.Count); // limit to available points

            int[] idx = new int[spawnPoints.Count];
            for(int i = 0; i < spawnPoints.Count; i++) idx[i] = i;

            // Shuffle and pick unique points
            for(int i = 0; i < num; i++)
            {
                int r = Random.Range(i, spawnPoints.Count);
                int tmp = idx[i]; idx[i] = idx[r]; idx[r] = tmp;

                Vector3 pos = spawnPoints[idx[i]];

                // Skip if overlapping with others or close to player
                if (!IsClearByOverlap(pos)) continue;
                if (IsTooCloseToPlayers(pos, minPlayerSqr)) continue;
                
                if (minDistanceBetween > 0f)
                {
                    bool tooClose = false;
                    for (int j = 0; j < spawned.Count; j++)
                    {
                        if ((spawned[j] - pos).sqrMagnitude < minSqr) { tooClose = true; break; }
                    }
                    if (tooClose) continue;
                    
                }

    
                GameObject obj = GameObject.Instantiate(prefab, pos, Quaternion.identity);
                NetworkObject net = obj.GetComponent<NetworkObject>();
                if (net != null) net.Spawn();

                spawned.Add(pos);
            }

            return;
        }

        // 2. No spawn points, so use spawn bound
        if (spawnArea == null) return;

        Bounds bound = spawnArea.bounds;
        float zPos = bound.center.z;

        int attempts = 0;
        while (spawned.Count < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 pos = new Vector3(
                Random.Range(bound.min.x, bound.max.x),
                Random.Range(bound.min.y, bound.max.y),
                zPos
            );

            if (!IsClearByOverlap(pos)) continue;
            if (IsTooCloseToPlayers(pos, minPlayerSqr)) continue;

            if (minDistanceBetween > 0f)
            {
                bool tooClose = false;
                for (int i = 0; i < spawned.Count; i++)
                {
                    if ((spawned[i] - pos).sqrMagnitude < minSqr) { tooClose = true; break; }
                }
                if (tooClose) continue;
            }

            GameObject obj = GameObject.Instantiate(prefab, pos, Quaternion.identity);
            NetworkObject net = obj.GetComponent<NetworkObject>();
            if (net != null) net.Spawn();

            spawned.Add(pos);
        }

        return;
    }

    bool IsClearByOverlap(Vector3 pos)
    {
        Vector2 center = new Vector2(pos.x, pos.y);

        if ((int)obstacleLayer != 0)
        {
            int foundLayer = Physics2D.OverlapBoxNonAlloc(center, spawnTestBoxSize, 0f, _overlapBuffer, (int)obstacleLayer);
            if (foundLayer > 0)
            {
                // any collider returned on obstacleLayer blocks spawn
                return false;
            }
        }

        int foundAll = Physics2D.OverlapBoxNonAlloc(center, spawnTestBoxSize, 0f, _overlapBuffer, ~0);
        
        if (foundAll > 0)
        {
            for (int i = 0; i < foundAll; i++)
            {
                var c = _overlapBuffer[i];
                if (c == null) continue;
                
                if (c.TryGetComponent<Pickable>(out _)) return false; // object detected is pickable

            }
        }

        return true;
    }

    bool IsTooCloseToPlayers(Vector3 pos, float minPlayerSqr)
    {
        if (minDistanceFromPlayers <= 0f || NetworkManager.Singleton == null) return false;

        Vector2 p2 = new Vector2(pos.x, pos.y);
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (client.PlayerObject == null) continue;
            Vector3 playerPos3 = client.PlayerObject.transform.position;
            Vector2 playerPos = new Vector2(playerPos3.x, playerPos3.y);
            if ((playerPos - p2).sqrMagnitude < minPlayerSqr) return true;
        }
        return false;
    }
}