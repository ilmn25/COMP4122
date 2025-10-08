using Resources.Scripts;
using UnityEngine;
using Unity.Netcode;
using Resources.Scripts;
public class Pickable : NetworkBehaviour
{
    public string ItemId;
    public Sprite Icon;

    // Called when a character picks this object (server should handle actual despawn)
    public void OnPickedUp(Character picker)
    {
        if (picker == null) return;

        picker._inventory.Add(ItemId);

        // network-aware removal:
        var net = GetComponent<NetworkObject>();
        if (net != null && net.IsSpawned)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                net.Despawn();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}