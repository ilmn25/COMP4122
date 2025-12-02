using Unity.Netcode;
using Unity.VisualScripting;

namespace Resources.Scripts
{
    public class Item : Interactable
    {
        public ItemID id;

        public override void Interact(Character character)
        {
            character.AddInventoryServerRpc((int)id);
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
            character.AddInventoryServerRpc((int)id);
            Audio.PlaySfx(AudioClipID.Item);
            
            // network-aware removal:
            NetworkObject net = GetComponent<NetworkObject>();
            if (net.IsSpawned && NetworkManager.Singleton.IsServer) net.Despawn();
            else Destroy(gameObject);
        }
         
    }
}