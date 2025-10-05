using System;
using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    [NonSerialized] public Vector2 Direction; 
    private const int Speed = 5;
    private const int Acceleration = 5;
    private const int Deceleration = 3;
    private float _currentSpeed;
    private Vector3 _directionBuffer;
    private Animator _animator;
    private GameObject _footprintObject;

    private void Awake()
    {
        _footprintObject = UnityEngine.Resources.Load<GameObject>("Prefabs/Footprint");
        _animator = GetComponent<Animator>();
        StartCoroutine(FootprintSpawnTimer());
        return;
        
        IEnumerator FootprintSpawnTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                if (_currentSpeed > 0.1f)
                {
                    Instantiate(_footprintObject, transform.position, Quaternion.identity);
                }
            } 
        }
    }

    private void Update()
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
        transform.position += _directionBuffer * (Time.deltaTime * _currentSpeed); 
        
        HandleAnimation();
        Direction = Vector2.zero;
    }

    private void HandleAnimation()
    {
        bool isMoving = Direction != Vector2.zero;

        if (isMoving)
        {
            _animator.Play("Move", 0); 
            _animator.speed = _currentSpeed / Speed;
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
