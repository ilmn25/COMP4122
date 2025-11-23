using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Resources.Scripts
{
    public class Interactable : NetworkBehaviour
    {
        public string itemId;

        // Called when a character picks this object (server should handle actual despawn)
        public void OnPickedUp(Character character)
        {
            character._inventory.Add(itemId);
            Audio.PlaySfx(AudioClipID.Item);
            // network-aware removal:
            NetworkObject net = GetComponent<NetworkObject>();
            if (net.IsSpawned)
                if (NetworkManager.Singleton.IsServer) net.Despawn();
            else Destroy(gameObject);
        }
    }
}