using System.Collections.Generic;
using UnityEngine;

public class Cache
{
    private static readonly Dictionary<string, AudioClip> AudioClipCache = new Dictionary<string, AudioClip>();
    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
    
    public static AudioClip LoadAudioClip(string path)
    {
        string fullPath = "Audio/" + path;
        if (AudioClipCache.TryGetValue(fullPath, out AudioClip cachedClip))
        {
            return cachedClip;
        }

        AudioClip clip = UnityEngine.Resources.Load<AudioClip>(fullPath);
        if (clip)
        {
            AudioClipCache[fullPath] = clip;
        } 

        return clip;
    }

    public static Sprite LoadSprite(string path)
    {
        string fullPath = "Textures/Sprites/" + path;
        if (SpriteCache.TryGetValue(fullPath, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        Sprite sprite = UnityEngine.Resources.Load<Sprite>(fullPath);
        if (sprite)
        {
            SpriteCache[fullPath] = sprite;
        } 

        return sprite;
    }
}