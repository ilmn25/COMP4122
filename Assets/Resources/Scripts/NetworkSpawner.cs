using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Resources.Scripts;
using Random = UnityEngine.Random;

public class NetworkSpawner : NetworkBehaviour
{
    [Header("Spawn Database")]
    public SpawnDatabase spawnDatabase;

    [Header("Spawn Bound")]
    public BoxCollider2D spawnArea;

    [Header("Placement Constraints")]
    public float minDistanceBetween = 1f;
    public float minDistanceFromPlayers = 2f;

    [Header("Collision Handling")]
    public Vector2 spawnTestBoxSize = new Vector2(0.6f, 0.6f);
    public LayerMask obstacleLayer; // for walls
    public int overlapBufferSize = 16; // increased pickable distance 

    public int maxAttempts = 20;
  

    void Start(){

        if (!IsServer) return;

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
        int amount = Random.Range(spawnable.minCount, spawnable.maxCount + 1);

        List<Vector3> spawnedObjs = new List<Vector3>();

        float minSqr = minDistanceBetween * minDistanceBetween;
        float minPlayerSqr = minDistanceFromPlayers * minDistanceFromPlayers;

        // 1. If inspector spawn points exist
        if (spawnPoints.Count > 0 )
        {
            int num = Mathf.Min(amount, spawnPoints.Count); // limit to available points

            int[] idx = new int[spawnPoints.Count];
            for(int i = 0; i < spawnPoints.Count; i++) idx[i] = i;

            // Shuffle and pick unique points
            for(int i = 0; i < num; i++)
            {
                int r = Random.Range(i, spawnPoints.Count);
                (idx[i], idx[r]) = (idx[r], idx[i]);

                Vector3 pos = spawnPoints[idx[i]];

                // Skip if overlapping with others or close to player
                if (IsOverlap(pos)) continue;
                if (IsNearPlayer(pos, minPlayerSqr)) continue;
                
                bool tooClose = false;
                foreach (var item in spawnedObjs)
                {
                    if ((item - pos).sqrMagnitude < minSqr)
                    { 
                        tooClose = true;  
                        break;
                    }
                }
                if (tooClose) continue;

                Instantiate(prefab, pos, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
                spawnedObjs.Add(pos);
            }

            return;
        }

        // 2. No spawn points, so use spawn bound
        if (spawnArea == null) return;

        Bounds bound = spawnArea.bounds;
        float zPos = bound.center.z;

        int attempts = 0;
        while (spawnedObjs.Count < amount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 pos = new Vector3(
                Random.Range(bound.min.x, bound.max.x),
                Random.Range(bound.min.y, bound.max.y),
                zPos
            );

            if (IsOverlap(pos)) continue;
            if (IsNearPlayer(pos, minPlayerSqr)) continue;

            if (minDistanceBetween > 0f)
            {
                bool tooClose = false;
                foreach (var t in spawnedObjs)
                {
                    if ((t - pos).sqrMagnitude < minSqr) { tooClose = true; break; }
                }
                if (tooClose) continue;
            }

            Instantiate(prefab, pos, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
            spawnedObjs.Add(pos);
        }
    }

    bool IsOverlap(Vector3 pos)
    {
        Collider2D[] overlapBuffer = new Collider2D[overlapBufferSize];
        Vector2 center = new Vector2(pos.x, pos.y);

        if (obstacleLayer != 0)
        {
            int foundLayer = Physics2D.OverlapBoxNonAlloc(center, spawnTestBoxSize, 0f, overlapBuffer, obstacleLayer);
            if (foundLayer > 0)
            {
                // any collider returned on obstacleLayer blocks spawn
                return true;
            }
        }

        int foundAll = Physics2D.OverlapBoxNonAlloc(center, spawnTestBoxSize, 0f, overlapBuffer, ~0);
        
        if (foundAll > 0)
        {
            for (int i = 0; i < foundAll; i++)
            {
                var c = overlapBuffer[i];
                if (c == null) continue;
                
                if (c.TryGetComponent<Interactable>(out _)) return true; // object detected is pickable

            }
        }

        return false;
    }

    bool IsNearPlayer(Vector3 pos, float minPlayerSqr)
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