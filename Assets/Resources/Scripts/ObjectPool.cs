using System.Collections.Generic;
using UnityEngine;

namespace Resources.Scripts
{
    public enum ID
    {
        Null, Footprint, Player
    }
    
    public class ObjectPool
    {
        private static readonly Dictionary<string, Queue<GameObject>> Pool = new ();
    
        public static GameObject GetObject(ID prefabName)
        {
            GameObject obj;
            if (Pool.ContainsKey(prefabName.ToString()) && Pool[prefabName.ToString()].Count > 0)
            { 
                obj = Pool[prefabName.ToString()].Dequeue();
                obj.SetActive(true);
            }
            else
            {
                obj = Object.Instantiate(UnityEngine.Resources.Load<GameObject>($"Prefabs/{prefabName}"));
                obj.name = prefabName.ToString();
            } 

            return obj;
        }

        public static void ReturnObject(GameObject obj)
        { 
            obj.SetActive(false);
        
            if (!Pool.ContainsKey(obj.name))
                Pool[obj.name] = new Queue<GameObject>();

            Pool[obj.name].Enqueue(obj);
        }
    }
}