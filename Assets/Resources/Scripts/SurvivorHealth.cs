using UnityEngine;

namespace Resources.Scripts
{
    public class SurvivorHealth : MonoBehaviour
    {
        [Header("血量设置")]
        public int maxHealth = 3;
        public int currentHealth;
        public bool isDead = false;

        private void Start()
        {
            currentHealth = maxHealth;
            isDead = false;
        
            HUD.InitializeHealth(maxHealth);
            HUD.UpdateHealth(currentHealth);
        }

        public void TakeDamage(int damageAmount = 1)
        {
            if (isDead || currentHealth <= 0)
                return;
        
            currentHealth -= damageAmount;
        
            HUD.UpdateHealth(currentHealth);
        
            if (currentHealth <= 0)
            {
                isDead = true;
                currentHealth = 0;
        
                HUD.UpdateHealth(0);
                Main.CurrentStatus = Status.MainMenu;
            }
        } 
    }
}