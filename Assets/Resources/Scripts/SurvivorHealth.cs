using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Resources.Scripts
{
    public partial class Character
    {
        [NonSerialized] public readonly int MaxHealth = 3;
        [NonSerialized] private readonly NetworkVariable<int> _currentHealth = new(3);
        public int CurrentHealth
        {
            get => _currentHealth.Value;
            private set => _currentHealth.Value = value;
        }

        private void Start()
        {
            if (IsServer) CurrentHealth = MaxHealth;
            HUD.UpdateHealth();
        }
        
        [ServerRpc]
        public void TakeDamageServerRpc(int damageAmount = 1)
        {
            if (CurrentHealth <= 0) return;
            CurrentHealth -= damageAmount; 
            if (CurrentHealth <= 0)
            {
                GetComponent<NetworkObject>().Despawn();
                Destroy(gameObject);
                Main.CurrentStatus = Status.MainMenu;
            }
        } 
    }
}