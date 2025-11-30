using UnityEngine;
using Unity.Netcode;
using System;

namespace Resources.Scripts
{
    public class Clock : Interactable
    {
        [Header("Clock Settings")]
        public float rotationAngle = 90f;
        public float rotationDuration = 0.5f;
        
        private NetworkVariable<float> currentRotation = new NetworkVariable<float>(0f);
        private bool isRotating = false;
        private Quaternion targetRotation;
        private float rotationTimer;

        // 添加角度变化事件
        public event Action<float> OnClockRotated;

        private void Update()
        {
            if (isRotating)
            {
                rotationTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(rotationTimer / rotationDuration);
                
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, progress);
                
                if (progress >= 1f)
                {
                    isRotating = false;
                    rotationTimer = 0f;
                }
            }
        }

        public override void Interact(Character character)
        {
            if (!isRotating)
            {
                RotateClockServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RotateClockServerRpc()
        {
            float newRotation = currentRotation.Value + rotationAngle;
            currentRotation.Value = newRotation;
            
            RotateClockClientRpc(newRotation);
        }

        [ClientRpc]
        private void RotateClockClientRpc(float newRotation)
        {
            targetRotation = Quaternion.Euler(0f, 0f, newRotation);
            isRotating = true;
            rotationTimer = 0f;
            
            // 触发事件
            OnClockRotated?.Invoke(newRotation);
            
            Debug.Log($"Clock rotated to {newRotation} degrees");
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetClockServerRpc()
        {
            currentRotation.Value = 0f;
            ResetClockClientRpc();
        }

        [ClientRpc]
        private void ResetClockClientRpc()
        {
            transform.rotation = Quaternion.identity;
            isRotating = false;
            rotationTimer = 0f;
        }

        public float GetCurrentRotation()
        {
            return currentRotation.Value;
        }

        public bool IsAtAngle(float targetAngle, float tolerance = 5f)
        {
            float normalizedAngle = currentRotation.Value % 360f;
            if (normalizedAngle < 0) normalizedAngle += 360f;
            
            float targetNormalized = targetAngle % 360f;
            if (targetNormalized < 0) targetNormalized += 360f;
            
            return Mathf.Abs(normalizedAngle - targetNormalized) <= tolerance;
        }
    }
}