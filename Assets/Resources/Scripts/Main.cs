using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;


namespace Resources.Scripts
{
    public enum Status
    {
        MainMenu, Game, HostUI, JoinUI, Pause
    }
    
    public class Main : MonoBehaviour
    {
        public static Main Instance { get; private set; }
        public static Status CurrentStatus = Status.MainMenu;
        public static Character TargetPlayer;
        public static GameObject ViewportObject;
        public static GameObject MainCameraObject;

        public static Light2D AmbientLight;
        public static Light2D SpotLight;
        
        public static LayerMask MaskStatic;
        public static LayerMask MaskSemi;
        public static LayerMask MaskCollide;
        
        public static GameObject HUDObject;

        // Main Menu UI Components
        public static GameObject UIMainMenuObject;
        public static Button UIHostButton;
        public static Button UIJoinButton;
        public static Button UIQuitButton;

        // Host UI Components
        public static GameObject UIHostObject;
        public static Image UITextField;
        public static TextMeshProUGUI UIHostPrompt;
        public static TextMeshProUGUI UIHostID;

        // Join UI Components
        public static GameObject UIJoinObject;
        public static TextMeshProUGUI UIJoinPrompt;
        public static TMP_InputField UIInputField;
        public static Button UIEnterButton;

        
        private void Awake()
        {
            Instance = this;
            
            // Time.fixedDeltaTime = 0.30f;
            Application.targetFrameRate = 100; // set max fps 
            QualitySettings.vSyncCount = 0;
        
            MaskStatic  = LayerMask.GetMask( "Map", "Semi"); 
            MaskCollide  = LayerMask.GetMask( "Map"); 
            MaskSemi  = LayerMask.GetMask("Semi"); 
            ViewportObject = GameObject.Find("Viewport");
            MainCameraObject = GameObject.Find("MainCamera"); 
            
            AmbientLight = GameObject.Find("AmbientLight").GetComponent<Light2D>();
            SpotLight = GameObject.Find("SpotLight").GetComponent<Light2D>(); 

            HUDObject = GameObject.Find("HUD");
            // Main Menu UI Components
            UIMainMenuObject = GameObject.Find("MainMenu");
            UIHostButton = GameObject.Find("HostButton").GetComponent<Button>();
            UIJoinButton = GameObject.Find("JoinButton").GetComponent<Button>();
            UIQuitButton = GameObject.Find("QuitButton").GetComponent<Button>();

            // Host UI Components
            UIHostObject = GameObject.Find("HostUI");
            UITextField = UIHostObject.transform.Find("TextField").GetComponent<Image>();
            UIHostPrompt = UIHostObject.transform.Find("HostPrompt").GetComponent<TextMeshProUGUI>();
            UIHostID = UIHostObject.transform.Find("HostID").GetComponent<TextMeshProUGUI>();

            // Join UI Components
            UIJoinObject = GameObject.Find("JoinUI");
            UIJoinPrompt = UIJoinObject.transform.Find("JoinPrompt").GetComponent<TextMeshProUGUI>();
            UIInputField = UIJoinObject.transform.Find("InputField").GetComponent<TMP_InputField>();
            UIEnterButton = UIJoinObject.transform.Find("EnterButton").GetComponent<Button>();

            // 直接吧所有object之类放这里，像一个字典那样，因为如果直接public，然后drag and drop然后后边要搬或copypaste然后reference断了就超烦
            // 所以要找object时当Main做字典用吧 （Main.TargetPlayer.transform.position) 那样
            // movement和viewport之类也是不用monoheavbiour的superclass绑在object上，如果class搬了下或者改名可能不小心script not found了超烦 （直接代替一些singleton）
             
        }

        private void Start()
        {
            UI.Start();
            Audio.PlaySfx(AudioClipID.Noise, true);
            Audio.PlayBGM(AudioClipID.JestersPity);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F)) Environment.SetEnvironment(EnvPreset.Day);
            if (Input.GetKeyDown(KeyCode.G)) Environment.SetEnvironment(EnvPreset.Night);
            if (Input.GetKeyDown(KeyCode.H)) Environment.SetEnvironment(EnvPreset.BlackScreen);
                
            if (Input.GetKeyDown(KeyCode.Escape)) 
                Screen.fullScreen = !Screen.fullScreen;
            
            // 这里看到update是由这个update叫其他其他update那样cascade下去，那样如果一个update要在另外一个update后面跑的话就不用lateupdate()那样折磨了 （avoids race condition)
            Viewport.Update();
            switch (CurrentStatus)
            {
                case Status.MainMenu:  
                    break;
                case Status.HostUI:  
                    break;
                case Status.JoinUI:  
                    break;
                case Status.Pause:  
                    break;
                case Status.Game:  
                    Movement.Update();
                    // 这里可以一会而用来pause一些东西，好像在interact时候不要控制走来走去
                    break;
            } 
        }
    }
}
