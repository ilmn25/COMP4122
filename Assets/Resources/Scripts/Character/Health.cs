using System;
using Unity.Netcode;

namespace Resources.Scripts
{
    public partial class Character
    {
        [NonSerialized] public readonly NetworkVariable<int> MaxHealth = new(3);
        [NonSerialized] public readonly NetworkVariable<int> CurrentHealth = new(3); 

        private void Start()
        {
            CurrentHealth.OnValueChanged += HUD.UpdateHealth;
            MaxHealth.OnValueChanged += HUD.UpdateHealth;
        }
        
        [ServerRpc]
        public void TakeDamageServerRpc(int damageAmount = 1)
        {
            if (CurrentHealth.Value <= 0) return;
            CurrentHealth.Value -= damageAmount; 
            if (CurrentHealth.Value <= 0)
            {
                GetComponent<NetworkObject>().Despawn();
                Destroy(gameObject);
                Main.CurrentStatus = Status.MainMenu;  
            }
        } 
    }
}