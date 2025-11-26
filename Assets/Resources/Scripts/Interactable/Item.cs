using Unity.Netcode;

namespace Resources.Scripts
{
    public class Item : Interactable
    {
        public ItemID id;

        public override void Interact(Character character)
        {
            character.Inventory.Add((int)id);
            Audio.PlaySfx(AudioClipID.Item);
            
            // network-aware removal:
            NetworkObject net = GetComponent<NetworkObject>();
            if (net.IsSpawned && NetworkManager.Singleton.IsServer) net.Despawn();
            else Destroy(gameObject);
        }
    }
    
    public class PowerUp : Interactable
    {
        public ItemID id;

        public override void Interact(Character character)
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