using UnityEngine;
using Unity.Netcode;

namespace Resources.Scripts
{
    public class Door : Interactable
    {
        public enum DoorType
        {
            Normal,
            Secure      
        }
        
        public DoorType doorType;
        public bool isFaceFront;
        [Header("Password Settings")]
        public string doorPassword = "1234";
        
        private SpriteRenderer _spriteRenderer;
        private readonly NetworkVariable<bool> _isOpen = new NetworkVariable<bool>(false);
        private readonly NetworkVariable<bool> _isUnlocked = new NetworkVariable<bool>(false);
        private GameObject _colliderObject;
        private PasswordLockUI _passwordUI;
        
        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _colliderObject = transform.Find("Collider").gameObject;
            
            GameObject uiObject = GameObject.Find("UI");
            if (uiObject != null)
            {
                _passwordUI = uiObject.GetComponentInChildren<PasswordLockUI>(true);
            }
            
            _isOpen.OnValueChanged += OnDoorStateChanged;
            UpdateDoorVisuals();
        }
        
        private void OnDoorStateChanged(bool previousValue, bool newValue)
        {
            UpdateDoorVisuals();
        }
        
        public override void Interact(Character character)
        {
            if (!_isOpen.Value)
            {
                TryOpenDoor(character);
            }
            else
            {
                CloseDoorServerRpc();
            }
            
            Audio.PlaySfx(AudioClipID.Item);
        }
        
        private void TryOpenDoor(Character character)
        {
            switch (doorType)
            {
                case DoorType.Normal:
                    OpenDoorServerRpc();
                    break;
                    
                case DoorType.Secure:
                    if (character.IsOwner)
                    {
                        if (_isUnlocked.Value)
                        {
                            OpenDoorServerRpc();
                        }
                        else
                        {
                            ShowPasswordUI();
                        }
                    }
                    break;
            }
        }
        
        private void ShowPasswordUI()
        {
            if (_passwordUI != null && !_isUnlocked.Value)
            {
                _passwordUI.Initialize(doorPassword, this, OnPasswordAttempt);
                _passwordUI.ShowPanel();
            }
        }
        
        private void OnPasswordAttempt(bool isCorrect)
        {
            if (isCorrect)
            {
                UnlockDoorServerRpc();
                OpenDoorServerRpc();
                _passwordUI?.ClosePanel();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void OpenDoorServerRpc()
        {
            _isOpen.Value = true;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void CloseDoorServerRpc()
        {
            _isOpen.Value = false;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void UnlockDoorServerRpc()
        {
            _isUnlocked.Value = true;
        }
        
        private void UpdateDoorVisuals()
        {
            if (_isOpen.Value)
            {
                _spriteRenderer.sprite = Cache.LoadSprite(isFaceFront ? "Door2" : "Door1");
                if (_colliderObject != null)
                {
                    _colliderObject.SetActive(false);
                }
            }
            else
            {
                _spriteRenderer.sprite = Cache.LoadSprite(isFaceFront ? "Door1" : "Door2");
                if (_colliderObject != null)
                {
                    _colliderObject.SetActive(true);
                }
            }
        }
        
        public void SetDoorState(bool open)
        {
            if (open)
            {
                OpenDoorServerRpc();
            }
            else
            {
                CloseDoorServerRpc();
            }
        } 
    }
}