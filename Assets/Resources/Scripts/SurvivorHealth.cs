using Resources.Scripts;
using UnityEngine;

public class SurvivorHealth : MonoBehaviour
{
    [Header("血量设置")]
    public int maxHealth = 3;
    public int currentHealth;
    public bool isDead = false;

    private void Start()
    {
        InitializeHealth();
    }

    // 初始化血量
    public void InitializeHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        
        // 确保HUDManager存在并初始化
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.InitializeHealth(maxHealth);
            HUDManager.Instance.UpdateHealth(currentHealth);
            Debug.Log("血量HUD初始化完成");
        }
        else
        {
            Debug.LogError("HUDManager实例未找到！");
            // 尝试查找HUDManager
            HUDManager hudManager = FindObjectOfType<HUDManager>();
            if (hudManager != null)
            {
                hudManager.InitializeHealth(maxHealth);
                hudManager.UpdateHealth(currentHealth);
            }
        }
    }

    // 受到伤害
    public void TakeDamage(int damageAmount = 1)
    {
        if (isDead || currentHealth <= 0)
            return;
        
        currentHealth -= damageAmount;
        
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateHealth(currentHealth);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 死亡处理
    private void Die()
    {
        isDead = true;
        currentHealth = 0;
        
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateHealth(0);
        }
        
        DisablePlayerControl();
        
        Debug.Log("求生者死亡");
    }

    private void DisablePlayerControl()
    {
        Main.CurrentStatus = Status.MainMenu;
    }
}