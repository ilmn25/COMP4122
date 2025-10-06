using UnityEngine;
using Unity.Netcode;

namespace Resources.Scripts
{
    public class UI
    { 
        public static void Start()
        {
            Main.UIHostButton.onClick.AddListener(OnHostButtonClicked);
            Main.UIJoinButton.onClick.AddListener(OnJoinButtonClicked);
            Main.UIQuitButton.onClick.AddListener(OnQuitButtonClicked);
            Main.UIEnterButton.onClick.AddListener(OnEnterButtonClicked);

            // Hide both UI panels initially
            Main.UIHostObject.SetActive(false);
            Main.UIJoinObject.SetActive(false);
            
            // Listen for when players connect
            if (NetworkManager.Singleton != null) // safety check 
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
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
        }

        private static void OnHostButtonClicked()
        {
            // Configure host to use port 7778 BEFORE starting
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData("0.0.0.0", 7778); // Host listens on port 7778
                Debug.Log("Host configured to listen on port 7778");
            }
            
            bool success = NetworkManager.Singleton.StartHost();

            if (success) 
            {
                
                Main.UIMainMenuObject.SetActive(false);
                Main.UIHostObject.SetActive(true);

                string hostIP = GetLocalIPAddress();
                
                Main.UIHostPrompt.text = "Invite your friends, with this IP address:";
                Main.UIHostID.text = $"{hostIP}:7778"; // Show port in display
            }
            else
            {
                Debug.LogError("Failed to start host. Please try again");
                // Reset to main menu state
                Main.TargetPlayer = null;
                Main.CurrentStatus = Status.MainMenu;
                Main.UIMainMenuObject.SetActive(true);
            }
        }

        private static void OnJoinButtonClicked()
        {
            // Hide main menu & Show the join UI panel
            Main.UIMainMenuObject.SetActive(false);
            Main.UIJoinObject.SetActive(true);
            
            Main.UIJoinPrompt.text = "Input IP address you want to join:";
        }

        private static void OnEnterButtonClicked()
        {
            
            // Check if UIInputField exists
            if (Main.UIInputField == null)
            {
                Debug.LogError("UIInputField is null!");
                return;
            }
            
            string targetIP = Main.UIInputField.text.Trim();
            
            if (string.IsNullOrEmpty(targetIP))
            {
                Debug.LogError("Please enter an IP address");
                return;
            }
            
            if (!IsValidIPAddress(targetIP))
            {
                Debug.LogError("Invalid IP address format");
                return;
            }
            
            // Check NetworkManager
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager.Singleton is null!");
                return;
            }
            
            // Set connection and start client (same as before)
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData(targetIP, 7778);
            }
            else
            {
                Debug.LogError("UnityTransport component not found on NetworkManager!");
                return;
            }
            
            Main.Instance.StartCoroutine(ConnectionTimeoutHandler());
            NetworkManager.Singleton.StartClient();
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