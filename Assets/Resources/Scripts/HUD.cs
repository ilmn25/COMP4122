using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts
{
    public class HUD : MonoBehaviour
    { 
        private static readonly List<Image> HealthImages = new ();
        private static int CurrentHealth => Main.TargetPlayer.CurrentHealth; 
        private static int MaxHealth => Main.TargetPlayer.MaxHealth; 
        
        public void Awake()
        {
            for (int i = 0; i < 5; i++) CreateHealthIcon(); 
            void CreateHealthIcon()
            {
                GameObject obj = Instantiate(UnityEngine.Resources.Load<GameObject>("Prefabs/HeartHUD"), transform);
                HealthImages.Add(obj.GetComponent<Image>());
            }
        }
    
        public static void UpdateHealth()
        {
            for (int i = 0; i < HealthImages.Count; i++)
            {
                if (i < CurrentHealth) HealthImages[i].sprite = Cache.LoadSprite("HeartFull");
                else if (i < MaxHealth) HealthImages[i].sprite = Cache.LoadSprite("HeartEmpty");
                else HealthImages[i].sprite = Cache.LoadSprite("Empty");
            }
        }
    }
}