using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts
{
    public class HUD : MonoBehaviour
    { 
        private static readonly List<Image> Health = new ();
        private static readonly List<Image> Inventory = new ();
        
        public void Awake()
        {
            for (int i = 0; i < 5; i++) {
                GameObject obj = Instantiate(UnityEngine.Resources.Load<GameObject>("Prefabs/HeartHUD"), transform);
                Health.Add(obj.GetComponent<Image>());
            }
            for (int i = 0; i < 5; i++) {
                GameObject obj = Instantiate(UnityEngine.Resources.Load<GameObject>("Prefabs/HeartHUD"), transform);
                Inventory.Add(obj.GetComponent<Image>());
            }
        }

        public static void UpdateHealth(int prev, int cur)
        {
            for (int i = 0; i < Health.Count; i++)
            {
                if (i < Main.TargetPlayer.CurrentHealth.Value) Health[i].sprite = Cache.LoadSprite("HeartFull");
                else if (i < Main.TargetPlayer.MaxHealth.Value) Health[i].sprite = Cache.LoadSprite("HeartEmpty");
                else Health[i].sprite = Cache.LoadSprite("Empty");
            }
        }
    }
}