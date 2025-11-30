using UnityEngine;

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
        private bool _isOpen = false;
        private GameObject _colliderObject;
        private PasswordLockUI _passwordUI;
        private Character _currentCharacter;
        
        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _colliderObject = transform.Find("Collider").gameObject;
            
            GameObject uiObject = GameObject.Find("UI");
            if (uiObject != null)
            {
                _passwordUI = uiObject.GetComponentInChildren<PasswordLockUI>(true);
            }
            
            if (_passwordUI == null)
            {
                _passwordUI = FindObjectOfType<PasswordLockUI>(true);
            }
            
            UpdateDoorVisuals();
        }
        
        public override void Interact(Character character)
        {
            _currentCharacter = character;
            
            if (!_isOpen)
            {
                TryOpenDoor(character);
            }
            else
            {
                CloseDoor();
            }
            
            Audio.PlaySfx(AudioClipID.Item);
        }
        
        private void TryOpenDoor(Character character)
        {
            switch (doorType)
            {
                case DoorType.Normal:
                    OpenDoor();
                    break;
                    
                case DoorType.Secure:
                    ShowPasswordUI();
                    break;
            }
        }
        
        private void ShowPasswordUI()
        {
            if (_passwordUI != null)
            {
                _passwordUI.Initialize(doorPassword, this, OnPasswordAttempt);
                _passwordUI.ShowPanel();
            }
        }
        
        private void OnPasswordAttempt(bool isCorrect)
        {
            if (isCorrect)
            {
                OpenDoor();
                _passwordUI?.ClosePanel();
            }
        }
        
        private void OpenDoor()
        {
            _isOpen = true;
            UpdateDoorVisuals();
            
            if (_colliderObject != null)
            {
                _colliderObject.SetActive(false);
            }
        }
        
        private void CloseDoor()
        {
            _isOpen = false;
            UpdateDoorVisuals();
            
            if (_colliderObject) _colliderObject.SetActive(true);
        }
        
        private void UpdateDoorVisuals()
        {
            if (_isOpen) _spriteRenderer.sprite = Cache.LoadSprite(isFaceFront ? "Door2" : "Door1");
            else _spriteRenderer.sprite = Cache.LoadSprite(isFaceFront ? "Door1" : "Door2");
        }
        
        public void SetDoorState(bool open)
        {
            if (open) OpenDoor();
            else CloseDoor();
        }

        public bool IsOpen()
        {
            return _isOpen;
        }
    }
}