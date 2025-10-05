using UnityEngine;

namespace Resources.Scripts
{
    public class UI
    { 
        public static void Start()
        {
            Main.UIHostButton.onClick.AddListener(OnHostButtonClicked);
            Main.UIQuitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        private static void OnHostButtonClicked()
        {
            Main.TargetPlayer = ObjectPool.GetObject(ID.Player).GetComponent<Character>();
            Main.CurrentStatus = Status.Game; 
            Main.UIMainMenuObject.gameObject.SetActive(false);
            Environment.SetEnvironment(EnvPreset.Night);
        }
        private static void OnQuitButtonClicked()
        {
            Application.Quit(); // works only for build, not in editor
        }
    }
}