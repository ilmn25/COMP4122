using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class ChaserSpawner : NetworkBehaviour
{
    [Header("Chaser Spawn Settings")]
    public GameObject chaserPrefab;
    public float spawnDelay = 60f; // Spawn after 60 seconds
    public Vector3 spawnPosition = Vector3.zero;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        StartCoroutine(SpawnChaser());
        return;

        IEnumerator SpawnChaser()
        {
            yield return new WaitForSeconds(spawnDelay);
            GameObject chaser = Instantiate(chaserPrefab, spawnPosition, Quaternion.identity);
            chaser.GetComponent<NetworkObject>().Spawn();
        }
    }
}