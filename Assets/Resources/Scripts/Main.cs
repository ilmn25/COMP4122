using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;


namespace Resources.Scripts
{ 
    
    public class Main : MonoBehaviour
    {
        public static Main Instance; 
        public static Character TargetPlayer;
        public static GameObject ViewportObject;
        public static GameObject MainCameraObject;

        public static bool CanMove = true;
        public static Light2D AmbientLight;
        public static Light2D SpotLight;
        
        public static LayerMask MaskStatic;
        public static LayerMask MaskSemi;
        public static LayerMask MaskCollide;
        
        public static LayerMask MaskInteractable;
 
        private void Awake()
        {
            Instance = this;
            
            // Audio.PlaySfx(AudioClipID.Noise, true);
            // Audio.PlayBGM(AudioClipID.JestersPity);
            // Time.fixedDeltaTime = 0.30f;
            Application.targetFrameRate = 100; // set max fps 
            QualitySettings.vSyncCount = 0;
            Screen.SetResolution(640, 360, false);
        
            MaskStatic  = LayerMask.GetMask( "Map", "Semi"); 
            MaskCollide  = LayerMask.GetMask( "Map"); 
            MaskSemi  = LayerMask.GetMask("Semi"); 
            MaskInteractable = LayerMask.GetMask("Interactable");
            ViewportObject = GameObject.Find("Viewport");
            MainCameraObject = GameObject.Find("MainCamera"); 
            
            AmbientLight = GameObject.Find("AmbientLight").GetComponent<Light2D>();
            SpotLight = GameObject.Find("SpotLight").GetComponent<Light2D>(); 

            // 直接吧所有object之类放这里，像一个字典那样，因为如果直接public，然后drag and drop然后后边要搬或copypaste然后reference断了就超烦
            // 所以要找object时当Main做字典用吧 （Main.TargetPlayer.transform.position) 那样
            // movement和viewport之类也是不用monoheavbiour的superclass绑在object上，如果class搬了下或者改名可能不小心script not found了超烦 （直接代替一些singleton）
              
        } 
        private void Update()
        {  
              
            if (Input.GetKeyDown(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;
            Viewport.Update(); 
            
            if (CanMove) Move();
            else 
                TargetPlayer.Direction = Vector2.zero;
            return;
            
            void Move()
            {
                if (TargetPlayer)
                {
                    Vector2 direction = Vector2.zero;
                    if (Input.GetKey(KeyCode.W))
                        direction += Vector2.up;
                    if (Input.GetKey(KeyCode.S)) 
                        direction += Vector2.down;
                    if (Input.GetKey(KeyCode.A))
                        direction += Vector2.left;
                    if (Input.GetKey(KeyCode.D))
                        direction += Vector2.right;
            
                    TargetPlayer.Direction = direction;
                } 
            }
        } 
    }
}
