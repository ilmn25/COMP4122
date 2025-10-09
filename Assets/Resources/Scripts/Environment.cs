using System.Collections;
using System.Collections.Generic;
using Resources.Scripts;
using Resources.Scripts.Utility;
using UnityEngine;

namespace Resources.Scripts
{
    public enum EnvPreset
    {
        BlackScreen, Night, Day
    }
    
    public partial class Environment    
    {
        // 当这个是个presets的data structure
        private Color _ambientColor;
        private Color _spotLightColor;
        private float _spotLightIntensity;
        // public ParticleEffect ParticleEffect; 一会给树叶，下雪之类可以
    }

    public partial class Environment
    {
        private static Environment _current;
        private static Environment _previous;
        private const int TransTime = 100;
        private static CoroutineTask _task;

        public static void SetEnvironment(EnvPreset preset, bool instant = false)
        {
            _previous = new Environment
            {
                _ambientColor = Main.AmbientLight.color,
                _spotLightColor = Main.SpotLight.color,
                _spotLightIntensity = Main.SpotLight.intensity
            };
            
            _current = Dictionary[preset];
            
            _task?.Stop();
            _task = new CoroutineTask(Ienumerator()); 
            //这个跟unity 的 StartCoroutine 一摸一样，分别是这个可以在monobheaviour的class外面用
            // Coroutine 和 IEnumerator 基本上就书是让code在多过一个frame里面跑，好像下面就分开了每frame跑一次while里面，yield return null 是 wait for next frame, 也有 yield return waitforsecond(5) 之类
            return;
            IEnumerator Ienumerator()
            { 
                int currentTransTime = instant? TransTime - 1 : 0;
                while (currentTransTime < TransTime)
                {  
                    float t = Mathf.InverseLerp(0, TransTime, currentTransTime % TransTime);
                    SetValues(Color.Lerp(_previous._ambientColor, _current._ambientColor, t),
                        Color.Lerp(_previous._spotLightColor, _current._spotLightColor, t),
                        Mathf.Lerp(_previous._spotLightIntensity, _current._spotLightIntensity, t));
                    currentTransTime++;
                    yield return null;
                }   
            }
        }
    }

    public partial class Environment
    {
        // 弄个class拿来控制互环境的lighting，particle effects 之类
        private static readonly Dictionary<EnvPreset, Environment> Dictionary = new();
        
        static Environment()
        {
            Dictionary.Add(EnvPreset.BlackScreen, new Environment
            {
                _ambientColor = Color.black,
                _spotLightColor = Color.black,
                _spotLightIntensity = 1
            });
            Dictionary.Add(EnvPreset.Night, new Environment
            {
                _ambientColor = GetColor(14, 11, 79),
                _spotLightColor = GetColor(14, 7, 61),
                _spotLightIntensity = 5 
            });
            Dictionary.Add(EnvPreset.Day, new Environment
            {
                _ambientColor = GetColor(122, 134, 140),
                _spotLightColor = GetColor(0, 16, 79),
                _spotLightIntensity = 3
            });
        }

        private static void SetValues(Color ambientColor, Color spotLightColor, float spotLightIntensity)
        {
            Main.AmbientLight.color = ambientColor;
            Main.SpotLight.color = spotLightColor;
            Main.SpotLight.intensity = spotLightIntensity;
        }

        private static Color GetColor(float r, float g, float b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }
    } 
}