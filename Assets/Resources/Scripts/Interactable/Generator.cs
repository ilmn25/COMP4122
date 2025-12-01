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
        private readonly NetworkVariable<float> _currentProgress = new NetworkVariable<float>();
        private readonly NetworkVariable<ulong> _interactingPlayerId = new NetworkVariable<ulong>(ulong.MaxValue);
        
        // local state
        private Coroutine _activationCoroutine;
            
        private void Start()
        {
            Debug.Log("Generator Start called!");
            
            // 初始化UI引用
            if (generatorCanvas == null)
                generatorCanvas = GetComponentInChildren<Canvas>();
            
            if (progressTextUI == null && generatorCanvas != null)
                progressTextUI = generatorCanvas.GetComponentInChildren<TextMeshProUGUI>();
            
            // set UI
            if (generatorCanvas != null)
            {
                generatorCanvas.renderMode = RenderMode.WorldSpace;
                if (generatorCanvas.worldCamera == null && Camera.main != null)
                {
                    generatorCanvas.worldCamera = Camera.main;
                }
                Debug.Log("UI Canvas setup complete!");
            }
            else
            {
                Debug.LogError("Canvas not found on Generator!");
            }
            
            // listen to network variable changes
            IsActivated.OnValueChanged += OnActivationStateChanged;
            _currentProgress.OnValueChanged += OnProgressChanged;
            _interactingPlayerId.OnValueChanged += OnInteractingPlayerChanged;
            
            UpdateDisplay();
        }
        
        private void LateUpdate()
        {
            if (generatorCanvas != null && Camera.main != null)
            {
                // Billboarding
                generatorCanvas.transform.LookAt(
                    generatorCanvas.transform.position + Camera.main.transform.forward,
                    Vector3.up
                );
            }
        }

        public override void Interact(Character character)
        {
            
            if (IsActivated.Value)
            {
                Debug.Log("Generator already activated, showing message");
                ShowAlreadyActivatedMessage();
                return;
            }
            
            if (_interactingPlayerId.Value != ulong.MaxValue && 
                _interactingPlayerId.Value != NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("Generator occupied by other player");
                ShowOccupiedMessage();
                return;
            }
            
            Debug.Log("Requesting activation from server");
            RequestActivationServerRpc(NetworkManager.Singleton.LocalClientId);
        }
            
        [ServerRpc(RequireOwnership = false)]
        private void RequestActivationServerRpc(ulong clientId)
        {
            Debug.Log($"Server received activation request from client {clientId}");
            
            if (IsActivated.Value) 
            {
                Debug.Log("Server: Generator already activated, rejecting request");
                return;
            }
            
            if (_interactingPlayerId.Value != ulong.MaxValue && _interactingPlayerId.Value != clientId) 
            {
                Debug.Log($"Server: Generator occupied by {_interactingPlayerId.Value}, rejecting request from {clientId}");
                return;
            }
            
            Debug.Log($"Server: Setting interacting player to {clientId}");
            _interactingPlayerId.Value = clientId;
        }
                
        private void OnInteractingPlayerChanged(ulong oldPlayerId, ulong newPlayerId)
        {
            Debug.Log($"Interacting player changed from {oldPlayerId} to {newPlayerId}");
            
            if (newPlayerId == NetworkManager.Singleton.LocalClientId && !IsActivated.Value)
            {
                Debug.Log("Local player is now interacting, starting activation");
                StartLocalActivation();
            }
            else if (newPlayerId == ulong.MaxValue && _activationCoroutine != null)
            {
                Debug.Log("Interacting player cleared, stopping activation");
                StopCoroutine(_activationCoroutine);
                _activationCoroutine = null;
                if (!IsActivated.Value)
                {
                    UpdateDisplay();
                }
            }
        }
        
        private void StartLocalActivation()
        {
            if (_activationCoroutine != null) return;
            
            _activationCoroutine = StartCoroutine(ActivationProgress());
        }
        
        private IEnumerator ActivationProgress()
        {
            Audio.PlaySfx(AudioClipID.Item);
            float localProgress = _currentProgress.Value;
            float progressIncrement = checkInterval / activationTime * 100f;
            
            while (localProgress < 100f)
            {
                yield return new WaitForSeconds(checkInterval);
                
                if (Vector3.Distance(Main.TargetPlayer.transform.position, transform.position) <= interactionRange)
                {
                    localProgress += progressIncrement;
                    UpdateProgressServerRpc(Mathf.RoundToInt(localProgress));
                }
                else
                {
                    CancelActivationServerRpc();
                    yield break;
                }
            }
            
            CompleteActivationServerRpc();
        }
         
        
        [ServerRpc]
        private void UpdateProgressServerRpc(int progress)
        {
            _currentProgress.Value = progress;
        }
        
        [ServerRpc]
        private void CancelActivationServerRpc()
        {
            _interactingPlayerId.Value = ulong.MaxValue;
            _currentProgress.Value = 0f;
        }
        
        [ServerRpc]
        private void CompleteActivationServerRpc()
        {
            IsActivated.Value = true;
            _currentProgress.Value = 100f;
            _interactingPlayerId.Value = ulong.MaxValue;
        }
        
        private void ShowAlreadyActivatedMessage()
        {
            StartCoroutine(ShowTemporaryMessage("ALREADY ACTIVATED", Color.red, 2f));
        }
        
        private void ShowOccupiedMessage()
        {
            StartCoroutine(ShowTemporaryMessage("OCCUPIED BY PLAYER", Color.yellow, 2f));
        }
        
        private IEnumerator ShowTemporaryMessage(string message, Color color, float duration)
        {
            SetTextDisplay(message, color);
            yield return new WaitForSeconds(duration);
            UpdateDisplay();
        }
        
        // UI display 
        private void SetTextDisplay(string text, Color color)
        { 
            progressTextUI.text = text;
            progressTextUI.color = color;
        }
        
        private void OnActivationStateChanged(bool oldValue, bool newValue)
        {
            UpdateDisplay();
        }
        
        private void OnProgressChanged(float oldValue, float newValue)
        {
            if (_interactingPlayerId.Value == NetworkManager.Singleton.LocalClientId && !IsActivated.Value)
            {
                UpdateProgressDisplay(Mathf.RoundToInt(newValue));
            }
        }
        
        private void UpdateProgressDisplay(int newValue)
        {
            if (!IsActivated.Value)
            {
                // change color based on progress
                Color progressColor;
                if (newValue < 33) progressColor = Color.red;
                else if (newValue < 66) progressColor = Color.yellow;
                else progressColor = Color.green;
                
                progressTextUI.text = newValue + "%";
                progressTextUI.color = progressColor;
            }
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
                UpdateProgressDisplay(Mathf.RoundToInt(_currentProgress.Value));
            }
            else
            {
                progressTextUI.text = "";
            }
        }
          
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            UpdateDisplay();
        }
    }
}