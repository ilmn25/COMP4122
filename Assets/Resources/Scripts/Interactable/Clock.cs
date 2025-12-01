using System;
using UnityEngine;
using Unity.Netcode;

namespace Resources.Scripts
{
    public class Clock : Interactable
    {
        private const int RotationAngle = 90;
        
        [NonSerialized] public readonly NetworkVariable<bool> IsCorrect = new ();
        private readonly NetworkVariable<int> _currentRotation = new ();
        public int targetRotation;
  
        private void Update()
        {
            Quaternion newRotation = Quaternion.Euler(0f, 0f, _currentRotation.Value);
            if (newRotation != transform.rotation)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * 8);
            } 
        }
        public override void Interact(Character character)
        {
            Audio.PlaySfx(AudioClipID.Item);
            RotateClockServerRpc();;
        }

        [ServerRpc(RequireOwnership = false)]
        private void RotateClockServerRpc()
        {
            _currentRotation.Value += RotationAngle;
            if (_currentRotation.Value >= 360) _currentRotation.Value -= 360;
            IsCorrect.Value = _currentRotation.Value == targetRotation;
        }
    }
}