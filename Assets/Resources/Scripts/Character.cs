using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

namespace Resources.Scripts
{
    public partial class Character : NetworkBehaviour
    {
        [NonSerialized] public Vector2 Direction; 
        private const int Speed = 5;
        private const int Acceleration = 5;
        private const int Deceleration = 3;
        private float _currentSpeed;
        private Vector3 _directionBuffer;
        private Animator _animator;

        private void Awake()
        { 
            _animator = GetComponent<Animator>();
            StartCoroutine(FootprintSpawnTimer());
            return;
        
            IEnumerator FootprintSpawnTimer()
            {
                while (true)
                {
                    yield return new WaitForSeconds(0.3f);
                    if (_currentSpeed > 0.1f)
                    {
                        ObjectPool.GetObject(ID.Footprint).transform.position = transform.position;
                        Audio.PlaySfx(UnityEngine.Random.Range(0,2) == 0? AudioClipID.Footsteps1 : AudioClipID.Footsteps2);
                    }
                } 
            }
        }

        private void Update()
        {
            if (!IsOwner) return; // added
            HandleMovement();
            HandleAnimation();
        }

        private void HandleAnimation()
        {
            bool isMoving = Direction != Vector2.zero;

            if (isMoving)
            {
                _animator.speed = _currentSpeed / Speed;
                _animator.Play("Move", 0);  
            }
            else
            {
                _animator.speed = 1;
                _animator.Play("Idle", 0);
            }

            if (Direction.x < 0)    
                _animator.Play("Left", 1); 
            else if (Direction.x > 0)
                _animator.Play("Right", 1);
        }
    }
    public partial class Character //everything collision and movement related
    {
        private readonly Vector2 _colliderOffset = new (0, 0.3f);
        private readonly Vector2 _colliderSize = new (0.55f, 0.35f);   
        private const float SlideToward = 0.06f; // 直接问我
        private const float SlideAlong = 0.03f;
        private static readonly Collider2D[] ColliderArray = new Collider2D[1];
        
        private void HandleMovement()
        {
            if (Direction != Vector2.zero)
            {
                float speedAdjust = Direction.x != 0 && Direction.y != 0 ? Speed * 0.7071f : Speed;
                _currentSpeed = Mathf.Lerp(_currentSpeed, speedAdjust, Time.fixedDeltaTime * Acceleration);
                _directionBuffer =  new Vector3(Direction.x, Direction.y, 0); // deceleration in previous frame's direction
            }
            else
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, 0, Time.fixedDeltaTime * Deceleration);
            }
            
            transform.position = GetNonCollidePosition(transform.position + _directionBuffer * (Time.deltaTime * _currentSpeed)); 
            return;
            
            bool IsNotCollide(Vector2 pos)
            {
                return Physics2D.OverlapBoxNonAlloc(pos + _colliderOffset, _colliderSize, 0, ColliderArray, Main.MaskStatic) == 0; 
            }
            
            Vector3 GetNonCollidePosition(Vector3 targetPos)
            { 
                if (IsNotCollide(targetPos)) return targetPos;
                
                //! go to possible directions when moving diagonally against a wall
                if (Direction.x != 0 && IsNotCollide(new Vector3(targetPos.x, transform.position.y, 0))) 
                { 
                    return new Vector3(targetPos.x, transform.position.y, 0);
                }
                if (Direction.y != 0 && IsNotCollide(new Vector3(transform.position.x, targetPos.y, 0)))
                {
                    return new Vector3(transform.position.x, targetPos.y, 0);
                }
                
                //! slide against wall if possible
                Vector3 posA, posB; 
                if (Direction.x != 0)
                {
                    posA = posB = new Vector3(transform.position.x + SlideToward * Direction.x, transform.position.y, 0);
                    posA.y += -SlideAlong;
                    posB.y += SlideAlong;
                    if (IsNotCollide(posA) && !IsNotCollide(posB)) return posA;
                    if (!IsNotCollide(posA) && IsNotCollide(posB)) return posB;
                }
                else
                {
                    posA = posB = new Vector3(transform.position.x, transform.position.y + SlideToward * Direction.y, 0);
                    posA.x += -SlideAlong;
                    posB.x += SlideAlong;
                    if (IsNotCollide(posA) && !IsNotCollide(posB)) return posA;
                    if (!IsNotCollide(posA) && IsNotCollide(posB)) return posB;
                }
                
                // cant move there
                return transform.position;
            }
 
        } 
    }
}

