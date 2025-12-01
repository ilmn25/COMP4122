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
        private NetworkVariable<bool> isActivated = new NetworkVariable<bool>();
        private NetworkVariable<float> currentProgress = new NetworkVariable<float>();
        private NetworkVariable<ulong> interactingPlayerId = new NetworkVariable<ulong>(ulong.MaxValue);
        
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
            isActivated.OnValueChanged += OnActivationStateChanged;
            currentProgress.OnValueChanged += OnProgressChanged;
            interactingPlayerId.OnValueChanged += OnInteractingPlayerChanged;
            
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
            Debug.Log($"Interact called - Activated: {isActivated.Value}, InteractingPlayer: {interactingPlayerId.Value}, LocalPlayer: {NetworkManager.Singleton.LocalClientId}");
            
            if (isActivated.Value)
            {
                Debug.Log("Generator already activated, showing message");
                ShowAlreadyActivatedMessage();
                return;
            }
            
            if (interactingPlayerId.Value != ulong.MaxValue && 
                interactingPlayerId.Value != NetworkManager.Singleton.LocalClientId)
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
            
            if (isActivated.Value) 
            {
                Debug.Log("Server: Generator already activated, rejecting request");
                return;
            }
            
            if (interactingPlayerId.Value != ulong.MaxValue && interactingPlayerId.Value != clientId) 
            {
                Debug.Log($"Server: Generator occupied by {interactingPlayerId.Value}, rejecting request from {clientId}");
                return;
            }
            
            Debug.Log($"Server: Setting interacting player to {clientId}");
            interactingPlayerId.Value = clientId;
        }
                
        private void OnInteractingPlayerChanged(ulong oldPlayerId, ulong newPlayerId)
        {
            Debug.Log($"Interacting player changed from {oldPlayerId} to {newPlayerId}");
            
            if (newPlayerId == NetworkManager.Singleton.LocalClientId && !isActivated.Value)
            {
                Debug.Log("Local player is now interacting, starting activation");
                StartLocalActivation();
            }
            else if (newPlayerId == ulong.MaxValue && _activationCoroutine != null)
            {
                Debug.Log("Interacting player cleared, stopping activation");
                StopCoroutine(_activationCoroutine);
                _activationCoroutine = null;
                if (!isActivated.Value)
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
            float localProgress = currentProgress.Value;
            float progressIncrement = checkInterval / activationTime * 100f;
            
            while (localProgress < 100f)
            {
                yield return new WaitForSeconds(checkInterval);
                
                if (IsLocalPlayerInRange())
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
        
        private bool IsLocalPlayerInRange()
        {
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (localPlayer != null)
            {
                return Vector3.Distance(localPlayer.transform.position, transform.position) <= interactionRange;
            }
            return false;
        }
        
        [ServerRpc]
        private void UpdateProgressServerRpc(int progress)
        {
            currentProgress.Value = progress;
        }
        
        [ServerRpc]
        private void CancelActivationServerRpc()
        {
            interactingPlayerId.Value = ulong.MaxValue;
            currentProgress.Value = 0f;
        }
        
        [ServerRpc]
        private void CompleteActivationServerRpc()
        {
            isActivated.Value = true;
            currentProgress.Value = 100f;
            interactingPlayerId.Value = ulong.MaxValue;
        }
        
        private void ShowAlreadyActivatedMessage()
        {
            if (progressTextUI != null)
                StartCoroutine(ShowTemporaryMessage("ALREADY ACTIVATED", Color.red, 2f));
        }
        
        private void ShowOccupiedMessage()
        {
            if (progressTextUI != null)
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
            if (progressTextUI != null)
            {
                progressTextUI.text = text;
                progressTextUI.color = color;
            }
        }
        
        private void OnActivationStateChanged(bool oldValue, bool newValue)
        {
            UpdateDisplay();
        }
        
        private void OnProgressChanged(float oldValue, float newValue)
        {
            if (interactingPlayerId.Value == NetworkManager.Singleton.LocalClientId && !isActivated.Value)
            {
                UpdateProgressDisplay(Mathf.RoundToInt(newValue));
            }
        }
        
        private void UpdateProgressDisplay(int progress)
        {
            if (progressTextUI != null && !isActivated.Value)
            {
                // change color based on progress
                Color progressColor;
                if (progress < 33) progressColor = Color.red;
                else if (progress < 66) progressColor = Color.yellow;
                else progressColor = Color.green;
                
                progressTextUI.text = progress + "%";
                progressTextUI.color = progressColor;
            }
        }
        
        private void UpdateDisplay()
        {
            if (progressTextUI != null)
            {
                if (isActivated.Value)
                {
                    progressTextUI.text = "ACTIVATED";
                    progressTextUI.color = Color.green;
                }
                else if (interactingPlayerId.Value == NetworkManager.Singleton.LocalClientId)
                {
                    UpdateProgressDisplay(Mathf.RoundToInt(currentProgress.Value));
                }
                else
                {
                    progressTextUI.text = "";
                }
            }
        }
        
        [ServerRpc]
        public void ResetGeneratorServerRpc()
        {
            isActivated.Value = false;
            currentProgress.Value = 0f;
            interactingPlayerId.Value = ulong.MaxValue;
        }
        
        public bool IsActivated()
        {
            return isActivated.Value;
        }
        
        public bool IsActivating()
        {
            return _activationCoroutine != null;
        }
                
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            UpdateDisplay();
        }
        
        public override void OnDestroy()
        {
            if (isActivated != null)
                isActivated.OnValueChanged -= OnActivationStateChanged;
            if (currentProgress != null)
                currentProgress.OnValueChanged -= OnProgressChanged;
            if (interactingPlayerId != null)
                interactingPlayerId.OnValueChanged -= OnInteractingPlayerChanged;
            
            base.OnDestroy();
        }
    }
}