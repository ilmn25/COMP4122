using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts
{
    public class HUD
    { 
        private static readonly List<Image> HealthImages = new List<Image>();

        public static void InitializeHealth(int maxHealth)
        {
            Debug.Log($"初始化血量显示: {maxHealth}颗心");
            ClearHealthDisplay();
            for (int i = 0; i < maxHealth; i++)
            {
                CreateHealthImage();
            }
        
            UpdateHealth(maxHealth);
            Debug.Log($"血量显示初始化完成，创建了{HealthImages.Count}颗心");
        }
    
        private static void ClearHealthDisplay()
        {
            foreach (Image healthImage in HealthImages)
            {
                GameObject.Destroy(healthImage.gameObject);
            }
            HealthImages.Clear();
        }
    
        private static void CreateHealthImage()
        {
            GameObject obj = GameObject.Instantiate(UnityEngine.Resources.Load<GameObject>("Prefabs/heart"),
                Main.HUDObject.transform);
            HealthImages.Add(obj.GetComponent<Image>());
        }
    
        public static void UpdateHealth(int currentHealth)
        {
            for (int i = 0; i < HealthImages.Count; i++)
            {
                HealthImages[i].sprite = i < currentHealth ? Cache.LoadSprite("Heart") : Cache.LoadSprite("Icon");
            }
        }
    }
}