using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;
    
    [Header("血量显示")]
    public GameObject healthContainer;
    public Image healthPrefab;
    public Sprite fullHealthSprite;
    public Sprite emptyHealthSprite;
    
    private List<Image> healthImages = new List<Image>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("HUDManager实例创建完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("HUDManager Start方法执行");
        // 检查组件引用
        if (healthContainer == null)
            Debug.LogError("Health Container未分配！");
        if (healthPrefab == null)
            Debug.LogError("Health Prefab未分配！");
        if (fullHealthSprite == null)
            Debug.LogError("Full Health Sprite未分配！");
        if (emptyHealthSprite == null)
            Debug.LogError("Empty Health Sprite未分配！");
    }

    // 初始化血量显示
    public void InitializeHealth(int maxHealth)
    {
        Debug.Log($"初始化血量显示: {maxHealth}颗心");
        
        // 清空现有的血量显示
        ClearHealthDisplay();
        
        // 创建新的血量显示
        for (int i = 0; i < maxHealth; i++)
        {
            CreateHealthImage();
        }
        
        UpdateHealth(maxHealth);
        Debug.Log($"血量显示初始化完成，创建了{healthImages.Count}颗心");
    }
    
    private void ClearHealthDisplay()
    {
        foreach (Image healthImage in healthImages)
        {
            if (healthImage != null)
                Destroy(healthImage.gameObject);
        }
        healthImages.Clear();
    }
    
    private void CreateHealthImage()
    {
        if (healthPrefab != null && healthContainer != null)
        {
            Image healthImage = Instantiate(healthPrefab, healthContainer.transform);
            healthImages.Add(healthImage);
            Debug.Log("创建了一颗心");
        }
        else
        {
            Debug.LogError("Health prefab or container not assigned!");
        }
    }
    
    public void UpdateHealth(int currentHealth)
    {
        Debug.Log($"更新血量显示: {currentHealth}颗心");
        
        for (int i = 0; i < healthImages.Count; i++)
        {
            if (healthImages[i] != null)
            {
                healthImages[i].sprite = i < currentHealth ? fullHealthSprite : emptyHealthSprite;
            }
        }
    }
}