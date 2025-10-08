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
    public class UI
    { 
        private static bool isUnityServicesInitialized = false;
        private static bool isProcessing = false; // prevent multiple trigger on button at once
        public static event Action onStartPressed; // event for starting the game
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
        }
        
        private static async void InitializeUnityServices()
        {
            if (isUnityServicesInitialized) return;
            
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                isUnityServicesInitialized = true;
                Debug.Log("Unity Services initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e}");
            }
        }
        
        private static void OnClientConnected(ulong clientId)
        {
            // Check if this is my connection 
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                var player = NetworkManager.Singleton.LocalClient?.PlayerObject;
                if (player != null)
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

        private static async void OnHostButtonClicked()
        {
            if (isProcessing) return;
            isProcessing = true;

            try
            {
                // Ensure Unity Services are initialized
                if (!isUnityServicesInitialized)
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
                    
                    Main.UIHostPrompt.text = "Share this code with friends:";
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
                isProcessing = false;
            }
        }

        private static async void OnStartButtonClicked()
        {
            if (isProcessing) return;
            isProcessing = true;

            try
            {
                onStartPressed?.Invoke();
                Main.UIHostObject.SetActive(false);
            }
            finally
            {
                isProcessing = false;
            }
        }
        
        private static async System.Threading.Tasks.Task InitializeUnityServicesAsync()
        {
            if (isUnityServicesInitialized) return;
            
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                isUnityServicesInitialized = true;
                Debug.Log("Unity Services initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e}");
                throw;
            }
            finally
            {
                isProcessing = false;
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
            if (isProcessing) return;
            isProcessing = true;

            try
            {
                // Check if UIInputField exists
                if (Main.UIInputField == null)
                {
                    Debug.LogError("UIInputField is null!");
                    return;
                }
                
                string joinCode = Main.UIInputField.text.Trim();
                
                if (string.IsNullOrEmpty(joinCode))
                {
                    Debug.LogError("Please enter a room code");
                    return;
                }
                
                // Ensure Unity Services are initialized
                if (!isUnityServicesInitialized)
                {
                    Debug.Log("Waiting for Unity Services to initialize...");
                    await InitializeUnityServicesAsync();
                }
                
                // Check NetworkManager
                if (NetworkManager.Singleton == null)
                {
                    Debug.LogError("NetworkManager.Singleton is null!");
                    return;
                }
                
                Debug.Log($"Attempting to join room with code: {joinCode}");
                
                // Join relay allocation using join code
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                
                Debug.Log("Successfully joined relay allocation");
                
                // Configure transport to use relay (convert joinAllocation to RelayServerData)
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(
                    joinAllocation.RelayServer.IpV4, 
                    (ushort)joinAllocation.RelayServer.Port, 
                    joinAllocation.AllocationIdBytes, 
                    joinAllocation.Key, 
                    joinAllocation.ConnectionData, 
                    joinAllocation.HostConnectionData
                );
                
                // Start connection timeout handler
                Main.Instance.StartCoroutine(ConnectionTimeoutHandler());
                
                // Start client
                bool clientStarted = NetworkManager.Singleton.StartClient();
                Debug.Log($"StartClient() returned: {clientStarted}");
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
                isProcessing = false;
            }
        }

        private static void OnQuitButtonClicked()
        {
            Application.Quit(); // works only for build, not in editor
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString(); // Returns something like "192.168.1.100"
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to get IP: {ex.Message}");
            }
            return "127.0.0.1"; // Fallback to localhost
        }
        
        private static bool IsValidIPAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
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