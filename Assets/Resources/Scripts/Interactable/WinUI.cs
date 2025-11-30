using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

namespace Resources.Scripts
{
    public class WinUI : NetworkBehaviour
    {
        public static WinUI Instance;
        
        [SerializeField] private GameObject winPanel;
        [SerializeField] private TextMeshProUGUI winText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            winPanel.SetActive(false);
            
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }
        
        public void ShowWinUI()
        {
            winPanel.SetActive(true);
            winText.text = "YOU WIN!";
        }
        
        private void RestartGame()
        {
            winPanel.SetActive(false);

            StopAllCoroutines();

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }

            Main.CanMove = false;
            Main.TargetPlayer = null;
    
            Door[] allDoors = FindObjectsOfType<Door>();
            foreach (Door door in allDoors)
            {
                door.SetDoorState(false); 
            }
            

            if (HUD.Inst != null)
                HUD.Inst.gameObject.SetActive(false);

            Environment.SetEnvironment(EnvPreset.Night);
            
            GameObject mainMenu = GameObject.Find("MainMenu");
            if (mainMenu != null)
                mainMenu.SetActive(true);
        }

        private void QuitGame()
        {
            Application.Quit();
        }
    }
}