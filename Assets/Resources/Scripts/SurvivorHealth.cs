using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Resources.Scripts
{
    public partial class Character
    {
        [NonSerialized] public readonly int MaxHealth = 3;
        [NonSerialized] private readonly NetworkVariable<int> _currentHealth = new ();

        public int CurrentHealth
        {
            get => _currentHealth.Value;
            set => _currentHealth.Value = value;
        }

        public bool isDead;

        private void Start()
        {
            CurrentHealth = MaxHealth;
            isDead = false;
        
            HUD.InitializeHealth();
            HUD.UpdateHealth();
        }

        public void TakeDamage(int damageAmount = 1)
        {
            if (isDead || CurrentHealth <= 0)
                return;
        
            CurrentHealth -= damageAmount;
        
            HUD.UpdateHealth();
        
            if (CurrentHealth <= 0)
            {
                isDead = true;
        
                HUD.UpdateHealth();
                Main.CurrentStatus = Status.MainMenu;
            }
        } 
    }
}