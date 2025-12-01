using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public class Chest : Interactable
    {
        public ItemID id; 
        private readonly NetworkVariable<bool> _looted =  new ();

        public override void Interact(Character character)
        { 
            if (id == ItemID.Null || _looted.Value)
            {
                Dialogue.Run(new DialogueData {Text = "It's empty."});
            }
            else if (id == ItemID.Trap)
            {
                SetLootedServerRpc();
                Dialogue.Run(new DialogueData {Text = "Ahh!"}, () =>
                {
                    character.ChangeHealthServerRpc();
                });
            }
            else
            { 
                SetLootedServerRpc();
                Dialogue.Run(new DialogueData {Text = "Theres something inside."}, () =>
                {
                    Audio.PlaySfx(AudioClipID.Item);
                    character.Inventory.Add((int)id);
                }); 
            } 
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetLootedServerRpc()
        {
            _looted.Value = true;
        }
    }
}