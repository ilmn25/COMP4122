using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Netcode;

namespace Resources.Scripts
{
    public class Generator : Interactable
    {
        [Header("Generator Settings")]
        [SerializeField] private float activationTime = 5f;
        [SerializeField] private float checkInterval = 0.1f;
        [SerializeField] private float interactionRange = 2f;
        
        [Header("UI Canvas References")]
        [SerializeField] private Canvas generatorCanvas;
        [SerializeField] private TextMeshProUGUI progressTextUI;
        
        // synced state
        public readonly NetworkVariable<bool> IsActivated = new NetworkVariable<bool>();
        private readonly NetworkVariable<ulong> _interactingPlayerId = new NetworkVariable<ulong>(ulong.MaxValue);
        
        // local state
        private float _currentProgress = 0f;
        private Coroutine _activationCoroutine;
            
        private void Start()
        {   
            // set ui
            if (generatorCanvas != null)
            {
                generatorCanvas.renderMode = RenderMode.WorldSpace;
                if (generatorCanvas.worldCamera == null && Camera.main != null)
                {
                    generatorCanvas.worldCamera = Camera.main;
                }
            }
            
            // listen to network variable changes
            IsActivated.OnValueChanged += (oldValue, newValue) => UpdateDisplay();
            _interactingPlayerId.OnValueChanged += OnInteractingPlayerChanged;
            
            UpdateDisplay();
        }

        public override void Interact(Character character)
        {
            if (IsActivated.Value) return; // already activated

            // being activated
            if (_interactingPlayerId.Value != ulong.MaxValue && _interactingPlayerId.Value != NetworkManager.Singleton.LocalClientId)
            {
                StartCoroutine(ShowTemporaryMessage("OCCUPIED BY PLAYER", Color.yellow, 2f));
                return;
            }

            RequestActivationServerRpc(NetworkManager.Singleton.LocalClientId);
        }
            
        [ServerRpc(RequireOwnership = false)]
        private void RequestActivationServerRpc(ulong clientId)
        {            
            if (IsActivated.Value) return; // already activated
            if (_interactingPlayerId.Value != ulong.MaxValue && _interactingPlayerId.Value != clientId) return; // double check
        
            _interactingPlayerId.Value = clientId;
        }
                
        private void OnInteractingPlayerChanged(ulong oldPlayerId, ulong newPlayerId)
        {
            
            if (newPlayerId == NetworkManager.Singleton.LocalClientId && !IsActivated.Value) {
                if(_activationCoroutine == null) _activationCoroutine = StartCoroutine(ActivationProgress());
            } 
            
            else if (newPlayerId == ulong.MaxValue && _activationCoroutine != null)
            {
                StopCoroutine(_activationCoroutine);
                _activationCoroutine = null;
                if (!IsActivated.Value)
                {
                    UpdateDisplay();
                }
            }
        }
        
        private IEnumerator ActivationProgress()
        {
            Audio.PlaySfx(AudioClipID.Item);
            _currentProgress = 0f;
            float progressIncrement = checkInterval / activationTime * 100f;
            
            while (_currentProgress < 100f)
            {
                yield return new WaitForSeconds(checkInterval);
                
                if (Vector3.Distance(Main.TargetPlayer.transform.position, transform.position) <= interactionRange)
                {
                    _currentProgress += progressIncrement;
                    UpdateDisplay();
                }
                else
                {
                    CancelActivationServerRpc();
                    yield break;
                }
            }
            
            CompleteActivationServerRpc();
        }
         
        [ServerRpc(RequireOwnership = false)]
        private void CancelActivationServerRpc()
        {
            _interactingPlayerId.Value = ulong.MaxValue;
            _currentProgress = 0f;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void CompleteActivationServerRpc()
        {
            IsActivated.Value = true;
            _currentProgress = 100f;
            _interactingPlayerId.Value = ulong.MaxValue;
        }
    
        private IEnumerator ShowTemporaryMessage(string message, Color color, float duration)
        {
            progressTextUI.text = message;
            progressTextUI.color = color;
            yield return new WaitForSeconds(duration);
            UpdateDisplay();
        }
        
        private void OnActivationStateChanged(bool oldValue, bool newValue)
        {
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (IsActivated.Value)
            {
                progressTextUI.text = "ACTIVATED";
                progressTextUI.color = Color.green;
            }
            else if (_interactingPlayerId.Value == NetworkManager.Singleton.LocalClientId)
            {
                int progress = Mathf.RoundToInt(_currentProgress);
                Color progressColor;
                if (progress < 33) progressColor = Color.red;
                else if (progress < 66) progressColor = Color.yellow;
                else progressColor = Color.green;

                progressTextUI.text = progress + "%";
                progressTextUI.color = progressColor;
            }
            else
            {
                progressTextUI.text = "";
            }
        }
    }
}