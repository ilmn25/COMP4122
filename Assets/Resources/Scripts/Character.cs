using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;

namespace Resources.Scripts
{
    public partial class Character : NetworkBehaviour
    {
        private readonly NetworkVariable<Vector2> _direction = new (
            writePerm: NetworkVariableWritePermission.Owner
        );
        public Vector2 Direction
        {
            get => _direction.Value;
            set { if (IsOwner) _direction.Value = value; }
        }
        
        private readonly NetworkVariable<float> _currentSpeed = new (
            writePerm: NetworkVariableWritePermission.Owner
        );

        private float CurrentSpeed
        {
            get => _currentSpeed.Value;
            set { if (IsOwner) _currentSpeed.Value = value; }
        }

        private const int Speed = 4;
        private const int Acceleration = 5;
        private const int Deceleration = 3; 
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
                    if (CurrentSpeed > 0.1f)
                    {
                        ObjectPool.GetObject(ID.Footprint).transform.position = transform.position;
                        Audio.PlaySfx(UnityEngine.Random.Range(0,2) == 0? AudioClipID.Footsteps1 : AudioClipID.Footsteps2);
                    }
                } 
            }
        }

        private void Update()
        {
            if (IsOwner) HandleMovement(); 
            HandleAnimation();
        }

        private void HandleAnimation()
        {
            bool isMoving = Direction != Vector2.zero;

            if (isMoving)
            {
                _animator.speed = CurrentSpeed / Speed;
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
        private readonly Vector2 _colliderOffset = new (0, 0.2f);
        private readonly Vector2 _colliderSize = new (0.55f, 0.55f);   
        private const float SlideToward = 0.06f; // 直接问我
        private const float SlideAlong = 0.03f;
        private static readonly Collider2D[] ColliderArray = new Collider2D[1];
        
        private void HandleMovement()
        {
            if (Direction != Vector2.zero)
            {
                float speedAdjust = Direction.x != 0 && Direction.y != 0 ? Speed * 0.7071f : Speed;
                CurrentSpeed = Mathf.Lerp(CurrentSpeed, speedAdjust, Time.fixedDeltaTime * Acceleration);
                _directionBuffer =  new Vector3(Direction.x, Direction.y, 0); // deceleration in previous frame's Direction
            }
            else
            {
                CurrentSpeed = Mathf.Lerp(CurrentSpeed, 0, Time.fixedDeltaTime * Deceleration);
            }
            
            transform.position = GetNonCollidePosition(transform.position + _directionBuffer * (Time.deltaTime * CurrentSpeed)); 
            return;
            
            bool IsNotCollide(Vector2 pos)
            {
                return Physics2D.OverlapBoxNonAlloc(pos + _colliderOffset, _colliderSize, 0, ColliderArray, Main.MaskCollide) == 0; 
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

