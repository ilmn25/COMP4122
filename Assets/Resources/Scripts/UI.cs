using Resources.Scripts.Utility;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Resources.Scripts
{
    public partial class UI : MonoBehaviour
    {
        public static UI Inst;
        private const int MinPlayers = 1;
        
        public GameObject uiMainMenuObject;
        public Button uiHostButton;
        public Button uiJoinButton;
        public Button uiQuitButton;

        public GameObject uiHostObject;
        public TextMeshProUGUI uiHostID;
        public Button uiBeginButton;
        public TextMeshProUGUI uiBeginButtonText;

        public GameObject uiJoinObject;
        public TMP_InputField uiInputField;
        public Button uiEnterButton;
        
        public void Start()
        {
            Inst = this;
            uiHostButton.onClick.AddListener(OnHostButtonClicked);
            uiJoinButton.onClick.AddListener(OnJoinButtonClicked);
            uiQuitButton.onClick.AddListener(OnQuitButtonClicked);
            uiEnterButton.onClick.AddListener(OnEnterButtonClicked);
            uiBeginButton.onClick.AddListener(OnBeginButtonClicked);

            // Hide both UI panels initially
            uiHostObject.SetActive(false);
            uiJoinObject.SetActive(false);
            
            // Initialize Unity Services
            UnityServices.InitializeAsync();
            AuthenticationService.Instance.SignInAnonymouslyAsync();
             
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected; 
        }
    }
    
    public partial class UI
    { 
        private static bool _busy; 
        public static event Action OnBegin; // event for starting the game 
 
        private void OnClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Main.TargetPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Character>(); 
                Main.TargetPlayer.transform.position = new Vector3(4, 66, 0);
                Main.CanMove = true;
                uiJoinObject.SetActive(false);
                Environment.SetEnvironment(EnvPreset.Night);
            }
            UpdateBeginButtonState();
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                uiMainMenuObject.SetActive(true);
                Main.TargetPlayer = null;
                Main.CanMove = false; 
                Main.ViewportObject.transform.position = new Vector3(0, 0, -1000);
            }
            UpdateBeginButtonState();
        }

        private void OnHostButtonClicked()
        {
            if (_busy) return;
            _busy = true;
            StartCoroutine(Slide(false, 0f, uiMainMenuObject, new Vector3(0, -10, 0))); 
            _ = new CoroutineTask(Task());
            IEnumerator Task()
            {
                Environment.SetEnvironment(EnvPreset.BlackScreen);
                yield return new WaitForSeconds(3);
                OnFinished();
            }
            return;
            
            async void OnFinished()
            {
                try
                {
                    // Configure transport to use relay (convert allocation to RelayServerData)
                    Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4); 
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                        allocation.RelayServer.IpV4,
                        (ushort)allocation.RelayServer.Port,
                        allocation.AllocationIdBytes,
                        allocation.Key,
                        allocation.ConnectionData
                    );
                    NetworkManager.Singleton.StartHost();
                    uiMainMenuObject.SetActive(false);
                    uiHostObject.SetActive(true);
                    uiHostID.text = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); // join code ABCDEF
                    UpdateBeginButtonState();
                }
                catch (RelayServiceException e)
                {
                    Debug.LogError($"Relay service error: {e}"); 
                    uiMainMenuObject.SetActive(true);
                }
                finally
                {
                    _busy = false;
                }
            }
        }

        private void OnBeginButtonClicked()
        { 
            uiBeginButton.interactable = false;
            OnBegin?.Invoke();
            uiHostObject.SetActive(false);
            Cutscene.Scene.Value = 1; 
        }
         
        private void UpdateBeginButtonState()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            
            if (NetworkManager.Singleton.ConnectedClients.Count >= MinPlayers)
            {
                uiBeginButton.interactable = true;
                uiBeginButtonText.text = "Begin";
            }
            else
            {
                uiBeginButton.interactable = false;
                uiBeginButtonText.text = "Not enough\nplayers...";
            }
        }

        private void OnJoinButtonClicked()
        {
            uiMainMenuObject.SetActive(false);
            uiJoinObject.SetActive(true);
        }

        private async void OnEnterButtonClicked()
        { 
            try
            {
                if (_busy) return;
                _busy = true;
                
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(uiInputField.text.Trim());
                
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(
                    joinAllocation.RelayServer.IpV4, 
                    (ushort)joinAllocation.RelayServer.Port, 
                    joinAllocation.AllocationIdBytes, 
                    joinAllocation.Key, 
                    joinAllocation.ConnectionData, 
                    joinAllocation.HostConnectionData
                );
                
                Main.Instance.StartCoroutine(ConnectionTimeoutHandler());
                NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                Debug.LogError($"Unexpected error: {e}");
            }
            finally{
                _busy = false;
            }
        }
        
        private IEnumerator ConnectionTimeoutHandler()
        {
            float elapsed = 0f;
            
            while (elapsed < 10 && !NetworkManager.Singleton.IsConnectedClient)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (!NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.LogError("Connection timed out. Unable to reach host.");
                
                // Stop the client and reset to main menu
                NetworkManager.Singleton.Shutdown();
                
                // Reset UI state
                Main.TargetPlayer = null;
                Main.CanMove = false;
                uiMainMenuObject.SetActive(true);
            }
        }
        
        private void OnQuitButtonClicked()
        {
            Application.Quit();  
        }
    }
}