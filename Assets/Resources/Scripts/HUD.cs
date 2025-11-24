using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts
{
    public enum ItemID { Card }
    public class HUD : MonoBehaviour
    { 
        private static readonly List<Image> Health = new ();
        private static readonly List<Image> Inventory = new ();
        
        public void Awake()
        { 
            Transform parent = transform.Find("Health"); 
            for (int i = 0; i < 5; i++) {
                GameObject obj = Instantiate(UnityEngine.Resources.Load<GameObject>("Prefabs/Icon"), parent);
                Health.Add(obj.GetComponent<Image>());
            } 
            parent = transform.Find("Inventory"); 
            for (int i = 0; i < 4; i++) {
                GameObject obj = Instantiate(UnityEngine.Resources.Load<GameObject>("Prefabs/Icon"), parent);
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

        public static void UpdateInventory(NetworkListEvent<int> changeEvent)
        {
            for (int i = 0; i < Inventory.Count; i++)
                Inventory[i].sprite = Cache.LoadSprite(i < Main.TargetPlayer.Inventory.Count? ((ItemID) Main.TargetPlayer.Inventory[i]).ToString() : "Empty");
        }
    }
}