using UnityEngine;
using Unity.Netcode;

public class ChaserSpawner : NetworkBehaviour
{
    [Header("Chaser Spawn Settings")]
    public GameObject chaserPrefab;
    public float spawnDelay = 60f; // Spawn after 60 seconds
    public Vector3 spawnPosition = Vector3.zero;
    
    private bool hasSpawned = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        // Delay chaser spawn
        Invoke(nameof(SpawnChaser), spawnDelay);
    }
    
    void SpawnChaser()
    {
        if (hasSpawned) return;
        
        if (chaserPrefab == null)
        {
            Debug.LogError("Chaser prefab not assigned!");
            return;
        }
        
        GameObject chaser = Instantiate(chaserPrefab, spawnPosition, Quaternion.identity);
        NetworkObject chaserNetworkObject = chaser.GetComponent<NetworkObject>();
        
        if (chaserNetworkObject != null)
        {
            chaserNetworkObject.Spawn();
            
            // Notify all clients
            NotifyChaserSpawnClientRpc();
        }
        else
        {
            Debug.LogError("Chaser prefab missing NetworkObject component!");
            Destroy(chaser); // Clean up invalid object
        }
        
        hasSpawned = true;
    }
    
    [ClientRpc]
    void NotifyChaserSpawnClientRpc()
    {
        Debug.Log("Warning! Chaser has spawned in the map!");
        // Can add client effects here: sound, screen effects, UI notifications, etc.
    }
}