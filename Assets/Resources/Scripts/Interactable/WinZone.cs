using UnityEngine;
using Unity.Netcode;

namespace Resources.Scripts
{
    public class WinZone : Interactable
    {
        public override void Interact(Character character)
        {
            if (character.IsOwner)
            {
                TriggerWinServerRpc(character.OwnerClientId);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void TriggerWinServerRpc(ulong clientId)
        {
            TriggerWinClientRpc(clientId);
        }
        
        [ClientRpc]
        private void TriggerWinClientRpc(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                WinUI.Instance?.ShowWinUI();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Character character = other.GetComponent<Character>();
            if (character != null && character.IsOwner)
            {
                Interact(character);
            }
        }
    }
}