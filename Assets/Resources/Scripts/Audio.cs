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
        private static List<AudioSource> _audioSources;
        private static AudioSource _bgmSource;

        private static readonly float BgmVolume = 1f;
        private static readonly float SfxVolume = 1f;
        private static readonly int PoolSize = 12;

        private static readonly Dictionary<AudioClipID, float> Volume = new ();
        
        static Audio()
        {
            GameObject audioManager = new GameObject("Audio");
            _bgmSource = audioManager.AddComponent<AudioSource>();

            _audioSources = new List<AudioSource>();
            for (int i = 0; i < PoolSize; i++)
            {
                AudioSource newSource = audioManager.AddComponent<AudioSource>();
                _audioSources.Add(newSource);
            } 
            
            
            Volume[AudioClipID.Noise] = 10;
        }

        public static void PlayBGM(AudioClipID id, float volume = 1f, bool loop = true)
        {
            AudioClip clip = UnityEngine.Resources.Load<AudioClip>("Audios/" + id);
            _bgmSource.clip = clip;
            _bgmSource.volume = volume * BgmVolume;
            _bgmSource.loop = loop;
            _bgmSource.Play();
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
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
        }

        private static AudioSource GetAvailableAudioSource()
        {
            foreach (AudioSource source in _audioSources)
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