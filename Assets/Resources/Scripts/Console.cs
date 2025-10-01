using System;
using Resources.Scripts;
using UnityEngine;

public class Console : MonoBehaviour
{
    // for debugging
    private static float _fps = 0.0f;
    private static int _frameCount = 0;
    private static float _elapsedTime = 0.0f;
    private static GUIStyle _guiStyle;  

    void Start()
    { 
        _guiStyle = new()
        {
            fontSize = 24,
            font = UnityEngine.Resources.Load<Font>("Textures/Others/OrangeKid/OrangeKid"),
            normal = { textColor = Color.white }
        };
    }

    void OnGUI()
    {
        HandleFPS();
        String output = "FPS: " + Mathf.Ceil(_fps) + "\n";
        if (Main.TargetPlayer) output += Main.TargetPlayer.transform.position;
        GUI.Label(new Rect(10, 10, 100, 20), output, _guiStyle);
        return;
        
        void HandleFPS()
        {
            _frameCount++;
            _elapsedTime += Time.unscaledDeltaTime;

            if (_elapsedTime >= 1.0f)
            {
                _fps = _frameCount / _elapsedTime;
                _frameCount = 0;
                _elapsedTime = 0.0f;
            }
        }
    }
    
}
