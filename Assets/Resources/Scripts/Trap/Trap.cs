using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public abstract class Trap : NetworkBehaviour
    {
        private static readonly Collider2D[] ColliderArray = new Collider2D[8];
        public Vector2 colliderOffset;
        public Vector2 colliderSize = Vector2.one;
            
        protected void Scan()
        {
            int hitCount = Physics2D.OverlapBoxNonAlloc(transform.position + new Vector3(colliderOffset.x, colliderOffset.y, 0), 
                colliderSize, 0, ColliderArray, LayerMask.GetMask("Player"));

            for (int i = 0; i < hitCount; i++)
            {
                OnTouch(ColliderArray[i].GetComponent<Character>());
            }
        }
 
        protected abstract void OnTouch(Character character);
    }
}