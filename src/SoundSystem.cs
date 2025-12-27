using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace _2d_td;

public static class SoundSystem
{
    private static Dictionary<SoundEffect, SoundEffectInstance> toggledSounds = new();

    static public SoundEffectInstance PlaySound(string name)
    {
        var sound = AssetManager.GetSound(name);
        var instance = sound.CreateInstance();
        instance.Volume = SettingsSystem.GetTotalSFXVolume();
        instance.Play();
        return instance;
    }

    static public SoundEffectInstance ToggleSound(string name, bool state)
    {
        var sound = AssetManager.GetSound(name);

        if (toggledSounds.TryGetValue(sound, out var soundInstance))
        {
            if (state)
            {
                return soundInstance;
            }

            soundInstance.Stop();
            soundInstance.Dispose();
            toggledSounds.Remove(sound);
        }
        else if (state)
        {
            var instance = sound.CreateInstance();
            instance.Volume = SettingsSystem.GetTotalSFXVolume();
            instance.IsLooped = true;
            instance.Play();
            toggledSounds.Add(sound, instance);

            return instance;
        }

        return null;
    }
}
