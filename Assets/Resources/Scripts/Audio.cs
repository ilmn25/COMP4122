using System.Collections.Generic;
using UnityEngine;

namespace Resources.Scripts
{
    public enum AudioClipID
    {
        Noise, JestersPity
    }
    public class Audio
    {
        private static readonly List<AudioSource> AudioSources;
        private static readonly AudioSource BGMSource;

        private const float BgmVolume = 1f;
        private const float SfxVolume = 1f;
        private const int PoolSize = 12;

        private static readonly Dictionary<AudioClipID, float> Volume = new ();
        
        static Audio()
        {
            GameObject audioManager = new GameObject("Audio");
            BGMSource = audioManager.AddComponent<AudioSource>();

            AudioSources = new List<AudioSource>();
            for (int i = 0; i < PoolSize; i++)
            {
                AudioSource newSource = audioManager.AddComponent<AudioSource>();
                AudioSources.Add(newSource);
            } 
            
            Volume[AudioClipID.Noise] = 10;
        }

        public static void PlayBGM(AudioClipID id, float volume = 1f, bool loop = true)
        {
            AudioClip clip = UnityEngine.Resources.Load<AudioClip>("Audios/" + id);
            BGMSource.clip = clip;
            BGMSource.volume = volume * BgmVolume;
            BGMSource.loop = loop;
            BGMSource.Play();
        }

        public static AudioSource PlaySfx(AudioClipID id, bool loop = false)
        {
            AudioClip clip = UnityEngine.Resources.Load<AudioClip>("Audios/" + id);
        
            float volume = Volume.ContainsKey(id) ? Volume[id] : 1;
            AudioSource availableSource = GetAvailableAudioSource();
            if (availableSource)
            {
                availableSource.clip = clip;
                availableSource.volume = volume * SfxVolume;
                availableSource.loop = loop;
                availableSource.Play();
            }
            return availableSource;
        }

        public static void StopSfx(AudioSource audioSource)
        {
            if (audioSource)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
        }

        private static AudioSource GetAvailableAudioSource()
        {
            foreach (AudioSource source in AudioSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return null;
        }
    }
}