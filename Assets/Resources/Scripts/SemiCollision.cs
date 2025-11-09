using System;
using Unity.Netcode;
using UnityEngine;

namespace Resources.Scripts
{
    public class SemiCollision : NetworkBehaviour
    {
        private static readonly Collider2D[] ColliderArray = new Collider2D[6];
        private readonly float _force = 0.7f;
        private Vector2 _velocity;
        private Vector3 _offset;
        private float _radius;

        public void Start()
        {
            CircleCollider2D circleCollider = gameObject.GetComponent<CircleCollider2D>();
            _offset = circleCollider.offset;
            Debug.Log(_offset);
            _radius = circleCollider.radius;
        }

        private void FixedUpdate()
        {
            int size = Physics2D.OverlapCircleNonAlloc(transform.position + _offset, _radius + 0.1f, ColliderArray, Main.MaskStatic);
            for (int i = 0; i < size; i++)
            {
                Collider2D col = ColliderArray[i];
                if (!col || col.gameObject == gameObject) continue;
                KnockBack(col.transform.position, _force, true);
                break;
            }

            if (_velocity != Vector2.zero)
            {
                Vector2 testPositionA = transform.position + (Vector3) _velocity * Time.deltaTime;
                Vector2 testPositionB = new Vector2(testPositionA.x, transform.position.y);
                Vector2 testPositionC = new Vector2(transform.position.x, testPositionA.y);
                if (IsNotCollide(testPositionA)) transform.position = testPositionA; 
                else if (IsNotCollide(testPositionB)) transform.position = testPositionB;
                else if (IsNotCollide(testPositionC)) transform.position = testPositionC;
                else _velocity = Vector2.zero;
                _velocity = Vector2.MoveTowards(_velocity, Vector2.zero, 30 * Time.deltaTime);
            } 
            return;
            
            void KnockBack(Vector3 position, float force, bool isAway)
            {
                _velocity += (Vector2)(isAway? transform.position - position : transform.position + position).normalized * force;
            }
            bool IsNotCollide(Vector2 pos)
            {
                return Physics2D.OverlapCircleNonAlloc(pos, _radius, ColliderArray, Main.MaskCollide) == 0; 
            }
        }
    }
}