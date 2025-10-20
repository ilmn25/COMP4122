using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts
{
    public class HUD
    { 
        private static readonly List<Image> HealthImages = new List<Image>();
        private static int MaxHealth => Main.TargetPlayer.MaxHealth; 
        private static int CurrentHealth => Main.TargetPlayer.CurrentHealth; 
        public static void InitializeHealth()
        {
            ClearHealthDisplay();
            for (int i = 0; i < MaxHealth; i++)
            {
                CreateHealthImage();
            }
        
            UpdateHealth();
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
            GameObject obj = GameObject.Instantiate(UnityEngine.Resources.Load<GameObject>("Prefabs/Heart"),
                Main.HUDObject.transform);
            HealthImages.Add(obj.GetComponent<Image>());
        }
    
        public static void UpdateHealth()
        {
            for (int i = 0; i < HealthImages.Count; i++)
            {
                HealthImages[i].sprite = i < CurrentHealth ? Cache.LoadSprite("Heart") : Cache.LoadSprite("Icon");
            }
        }
    }
}