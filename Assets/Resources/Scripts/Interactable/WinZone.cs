using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace Resources.Scripts
{
    // no time to clean up this script
    public class WinZone : Interactable
    {
        private HashSet<ulong> playersReached = new HashSet<ulong>();
        private NetworkVariable<bool> winTriggered = new NetworkVariable<bool>(false);
        
        public override void Interact(Character character)
        {
            if (character.IsOwner)
            {
                RegisterPlayerReachedServerRpc(character.OwnerClientId);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RegisterPlayerReachedServerRpc(ulong clientId)
        {
            if (playersReached.Contains(clientId))
                return;
                
            playersReached.Add(clientId);
            
            CheckWinCondition();
        }
        
        private void CheckWinCondition()
        {
            if (winTriggered.Value) return;
            
            int totalPlayers = NetworkManager.Singleton.ConnectedClients.Count;
            
            if (playersReached.Count >= totalPlayers && totalPlayers > 0)
            {
                winTriggered.Value = true;
                Cutscene.Scene.Value = 3;
            }
        }
         
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            if (IsServer && !winTriggered.Value)
            {
                playersReached.Remove(clientId);
                CheckWinCondition();
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
        
        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            base.OnNetworkDespawn();
        }
    }
}
