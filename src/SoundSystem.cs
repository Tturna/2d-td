using Microsoft.Xna.Framework.Audio;

namespace _2d_td;

public static class SoundSystem
{
    static public SoundEffectInstance PlaySound(string name)
    {
        var sound = AssetManager.GetSound(name);
        var instance = sound.CreateInstance();
        instance.Volume = SettingsSystem.GetTotalSFXVolume();
        instance.Play();
        return instance;
    }
}
