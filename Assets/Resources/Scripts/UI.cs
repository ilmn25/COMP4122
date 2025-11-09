using Resources.Scripts.Utility;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System;
using UnityEngine.UI;

namespace Resources.Scripts
{
    public static partial class UI
    { 
        private static bool _isUnityServicesInitialized;
        private static bool _isProcessing = false; // prevent multiple trigger on button at once
        public static event Action OnStartPressed; // event for starting the game
        private static readonly Color StartEnabledColor = Color.white;
        private static readonly Color StartDisabledColor = new Color(0.6f, 0.6f, 0.6f);
        
        public static void Start()
        {
            Main.UIHostButton.onClick.AddListener(OnHostButtonClicked);
            Main.UIJoinButton.onClick.AddListener(OnJoinButtonClicked);
            Main.UIQuitButton.onClick.AddListener(OnQuitButtonClicked);
            Main.UIEnterButton.onClick.AddListener(OnEnterButtonClicked);
            Main.UIStartButton.onClick.AddListener(OnStartButtonClicked);

            // Hide both UI panels initially
            Main.UIHostObject.SetActive(false);
            Main.UIJoinObject.SetActive(false);
            
            // Initialize Unity Services
            InitializeUnityServices();
            
            // Listen for when players connect
            if (NetworkManager.Singleton) // safety check 
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        
        private static async void InitializeUnityServices()
        { 
            try
            {
                if (_isUnityServicesInitialized) return;
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                _isUnityServicesInitialized = true;
                Debug.Log("Unity Services initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e}");
            }
        }
        
        private static void OnClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                NetworkObject player = NetworkManager.Singleton.LocalClient?.PlayerObject;
                if (player)
                {
                    Main.TargetPlayer = player.GetComponent<Character>();
                    Main.CurrentStatus = Status.Game;
                    Main.UIMainMenuObject.SetActive(false);
                    Main.UIHostObject.SetActive(false);
                    Main.UIJoinObject.SetActive(false);
                    Environment.SetEnvironment(EnvPreset.Night);
                }
                else
                {
                    Debug.LogError("Player object is null.");
                }
            }

            UpdateStartButtonState();
        }
        
        private static void OnClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Main.UIHostObject.SetActive(false);
                Main.UIJoinObject.SetActive(false);
                Main.TargetPlayer = null;
                Main.CurrentStatus = Status.MainMenu;
                Main.UIMainMenuObject.SetActive(true);
                Main.ViewportObject.transform.position = new Vector3(0, 0, -1000);
            }

            else if (NetworkManager.Singleton.IsHost){
                
                var network = NetworkManager.Singleton;
                int connected = network.ConnectedClients != null ? network.ConnectedClients.Count : 0;

                if (connected < 2)
                {
                    Main.UIMainMenuObject.SetActive(false);
                    Main.UIHostObject.SetActive(true);
                    Main.UIJoinObject.SetActive(false);
                }
            }

            UpdateStartButtonState();
        }

        private static void OnHostButtonClicked()
        {
            if (_isProcessing) return;
            _isProcessing = true;
            CoroutineTask task = new CoroutineTask(Slide(false, 0f, Main.UIMainMenuObject, new Vector3(0, -10, 0)));
            task.Finished += _ =>
            {
                HostGame();
            };
            return;
            
            async void HostGame()
            {
                try
                {
                    // Ensure Unity Services are initialized
                    if (!_isUnityServicesInitialized)
                    {
                        Debug.Log("Waiting for Unity Services to initialize...");
                        await InitializeUnityServicesAsync();
                    }

                    Debug.Log("Creating relay allocation...");

                    // Create relay allocation for up to 4 players
                    Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

                    // Get join code that clients will use
                    string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                    Debug.Log($"Relay allocation created. Join code: {joinCode}");

                    // Configure transport to use relay (convert allocation to RelayServerData)
                    var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                    transport.SetRelayServerData(
                        allocation.RelayServer.IpV4,
                        (ushort)allocation.RelayServer.Port,
                        allocation.AllocationIdBytes,
                        allocation.Key,
                        allocation.ConnectionData
                    );

                    // Start host
                    bool success = NetworkManager.Singleton.StartHost();

                    if (success)
                    {
                        Main.UIMainMenuObject.SetActive(false);
                        Main.UIHostObject.SetActive(true);
 
                        Main.UIHostID.text = joinCode; // Show join code instead of IP

                        Debug.Log($"Host started successfully with join code: {joinCode}");
                        UpdateStartButtonState();
                    }
                    else
                    {
                        Debug.LogError("Failed to start host");
                        // Reset to main menu state
                        Main.TargetPlayer = null;
                        Main.CurrentStatus = Status.MainMenu;
                        Main.UIMainMenuObject.SetActive(true);
                    }
                }
                catch (RelayServiceException e)
                {
                    Debug.LogError($"Relay service error: {e}");
                    // Reset to main menu on error
                    Main.TargetPlayer = null;
                    Main.CurrentStatus = Status.MainMenu;
                    Main.UIMainMenuObject.SetActive(true);
                }
                finally
                {
                    _isProcessing = false;
                }
            }
        }



        private static async void OnStartButtonClicked()
        {
            if (_isProcessing) return;
            _isProcessing = true;

            try
            {
                OnStartPressed?.Invoke();
                Main.UIHostObject.SetActive(false);
            }
            finally
            {
                _isProcessing = false;
            } 
        }

        private static async System.Threading.Tasks.Task InitializeUnityServicesAsync()
        {
            if (_isUnityServicesInitialized) return;
            
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                _isUnityServicesInitialized = true;
                Debug.Log("Unity Services initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e}");
                throw;
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private static void UpdateStartButtonState()
        {
            if (Main.UIStartButton == null) return;

            bool enable = false;
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                int connectedClients = NetworkManager.Singleton.ConnectedClients.Count;
                enable = connectedClients >= 2; // at least 2 players to start
            }

            Main.UIStartButton.interactable = enable;
            var colors = Main.UIStartButton.colors;
            colors.normalColor = enable ? StartEnabledColor : StartDisabledColor;
            Main.UIStartButton.colors = colors;
        }

        private static void OnJoinButtonClicked()
        {
            // Hide main menu & Show the join UI panel
            Main.UIMainMenuObject.SetActive(false);
            Main.UIJoinObject.SetActive(true);
            
            Main.UIJoinPrompt.text = "Enter room code:";
        }

        private static async void OnEnterButtonClicked()
        {
            if (_isProcessing) return;
            _isProcessing = true;

            try
            {
                // Ensure Unity Services are initialized
                if (!_isUnityServicesInitialized)
                {
                    Debug.Log("Waiting for Unity Services to initialize...");
                    await InitializeUnityServicesAsync();
                }
                
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(Main.UIInputField.text.Trim());
                
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
            catch (RelayServiceException e)
            {
                Debug.LogError($"Failed to join relay: {e}");
                Debug.LogError("Make sure the room code is correct and the host is online.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Unexpected error: {e}");
            }
            finally{
                _isProcessing = false;
            }
        }

        private static void OnQuitButtonClicked()
        {
            Application.Quit(); // works only for build, not in editor
        }
        
        private static System.Collections.IEnumerator ConnectionTimeoutHandler()
        {
            float timeout = 10f; // 10 second timeout
            float elapsed = 0f;
            
            while (elapsed < timeout && !NetworkManager.Singleton.IsConnectedClient)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // If we're still not connected after timeout
            if (!NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.LogError("Connection timed out. Unable to reach host.");
                
                // Stop the client and reset to main menu
                NetworkManager.Singleton.Shutdown();
                
                // Reset UI state
                Main.TargetPlayer = null;
                Main.CurrentStatus = Status.MainMenu;
                Main.UIMainMenuObject.SetActive(true);
            }
        }
    }
}