using System;
using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public partial class Character
    {
        [NonSerialized] public readonly NetworkVariable<int> MaxHealth = new(3);
        [NonSerialized] public readonly NetworkVariable<int> CurrentHealth = new(3); 
        [NonSerialized] public readonly NetworkList<int> Inventory = new();  
        [NonSerialized] public readonly NetworkList<int> Status = new();  

        [ServerRpc (RequireOwnership = false)]
        public void ChangeHealthServerRpc(int value = -1)
        {
            if (CurrentHealth.Value <= 0 && value < 0) return;
            Audio.PlaySfx(value < 0 ? AudioClipID.Blood : AudioClipID.Item);
            CurrentHealth.Value += value; 
            if (CurrentHealth.Value < 0) CurrentHealth.Value = 0;
            if (CurrentHealth.Value > MaxHealth.Value) CurrentHealth.Value = MaxHealth.Value;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddInventoryServerRpc(int value)
        {
            Inventory.Add(value);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveInventoryServerRpc(int value)
        {
            Inventory.Remove(value);
        }
    }
}