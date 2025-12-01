using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts
{
    public enum ItemID { Null, Key, Clue, Trap}
    public enum StatusID { Slow, Stuck, }
    public class HUD : MonoBehaviour
    {
        public static HUD Inst;
        public static GameObject InteractText;
        private static readonly List<Image> Health = new ();
        private static readonly List<Image> Inventory = new ();
        private static readonly List<Image> Status = new ();
        
        public void Awake()
        {
            InteractText = GameObject.Find("InteractText");
            InteractText.SetActive(false);
            Inst = this;
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
            parent = transform.Find("Status"); 
            for (int i = 0; i < 4; i++) {
                GameObject obj = Instantiate(UnityEngine.Resources.Load<GameObject>("Prefabs/Icon"), parent);
                Status.Add(obj.GetComponent<Image>());
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
        public static void UpdateStatus(NetworkListEvent<int> changeEvent)
        {
            for (int i = 0; i < Status.Count; i++)
                Status[i].sprite = Cache.LoadSprite(i < Main.TargetPlayer.Status.Count? ((ItemID) Main.TargetPlayer.Status[i]).ToString() : "Empty");
        }
    }
}