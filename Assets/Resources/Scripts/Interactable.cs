using Unity.Netcode;

namespace Resources.Scripts
{
    public class Interactable : NetworkBehaviour
    {
        public ItemID id;

        // Called when a character picks this object (server should handle actual despawn)
        public void OnPickedUp(Character character)
        {
            character.Inventory.Add((int)id);
            Audio.PlaySfx(AudioClipID.Item);
            
            // network-aware removal:
            NetworkObject net = GetComponent<NetworkObject>();
            if (net.IsSpawned && NetworkManager.Singleton.IsServer) net.Despawn();
            else Destroy(gameObject);
        }
    }
}