using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


namespace Resources.Scripts
{
    public enum Status
    {
        MainMenu, Game
    }
    
    public class Main : MonoBehaviour
    {
        public static Status CurrentStatus = Status.MainMenu;
        public static Character TargetPlayer;
        public static GameObject ViewportObject;
        public static GameObject MainCameraObject; 
        public static GameObject UIMainMenuObject;
        public static Button UIHostButton;
        public static Button UIQuitButton;
        public static Light2D AmbientLight;
        public static Light2D SpotLight;
        
        public static LayerMask MaskStatic;
        private void Awake()
        {
            // Time.fixedDeltaTime = 0.30f;
            Application.targetFrameRate = 100; // set max fps 
            QualitySettings.vSyncCount = 0;
        
            MaskStatic  = LayerMask.GetMask("Collide", "Map"); 
            ViewportObject = GameObject.Find("Viewport");
            MainCameraObject = GameObject.Find("MainCamera"); 
            UIMainMenuObject = GameObject.Find("MainMenu");
            UIHostButton = GameObject.Find("HostButton").GetComponent<Button>();
            UIQuitButton = GameObject.Find("QuitButton").GetComponent<Button>();
            AmbientLight = GameObject.Find("AmbientLight").GetComponent<Light2D>();
            SpotLight = GameObject.Find("SpotLight").GetComponent<Light2D>(); 
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
                case Status.Game:  
                    Movement.Update();
                    // 这里可以一会而用来pause一些东西，好像在interact时候不要控制走来走去
                    break;
            } 
        }
    }
}
