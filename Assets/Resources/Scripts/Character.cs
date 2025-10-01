using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    [NonSerialized] public Vector2 Direction;
    private const int Speed = 5;
    private const int Acceleration = 5;
    private float _currentSpeed;

    private void Update()
    {
        float speedAdjust = Direction.x != 0 && Direction.y != 0 ? 0.7071f * Speed : Speed;
        _currentSpeed = Mathf.Lerp(_currentSpeed, Direction != Vector2.zero ? speedAdjust : 0, Time.deltaTime * Acceleration);
        transform.position += new Vector3(Direction.x, Direction.y, 0) * (Time.deltaTime * _currentSpeed); 
        
        HandleAnimation();
        Direction = Vector2.zero;
    }

    private void HandleAnimation()
    {
        if (Direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (Direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }
}
