using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace _2d_td;

public static class SoundSystem
{
    private record struct ToggledSound
    {
        public SoundEffectInstance Instance;
        public int CurrentUserCount;
    }

    private static Dictionary<SoundEffect, ToggledSound> toggledSounds = new();
    private static bool isSubscribedToSettings;

    private static void UpdateToggledSoundVolume()
    {
        foreach (var (sound, toggledSound) in toggledSounds)
        {
            if (toggledSound.Instance is null) continue;

            toggledSound.Instance.Volume = SettingsSystem.GetTotalSFXVolume();
        }
    }

    static public SoundEffectInstance PlaySound(string name)
    {
        if (!isSubscribedToSettings)
        {
            SettingsScreen.OnDestroyed += () => UpdateToggledSoundVolume();
        }

        var sound = AssetManager.GetSound(name);
        var instance = sound.CreateInstance();
        instance.Volume = SettingsSystem.GetTotalSFXVolume();
        instance.Play();

        return instance;
    }

    static public SoundEffectInstance ToggleSound(string name, bool state)
    {
        if (!isSubscribedToSettings)
        {
            SettingsScreen.OnDestroyed += () => UpdateToggledSoundVolume();
        }

        var sound = AssetManager.GetSound(name);
        ToggledSound toggledSound;

        if (toggledSounds.TryGetValue(sound, out toggledSound))
        {
            if (state)
            {
                toggledSound.CurrentUserCount++;
                toggledSounds[sound] = toggledSound;

                return toggledSound.Instance;
            }

            if (toggledSound.CurrentUserCount > 1)
            {
                toggledSound.CurrentUserCount--;
                toggledSounds[sound] = toggledSound;

                return toggledSound.Instance;
            }

            toggledSound.Instance.Stop();
            toggledSound.Instance.Dispose();
            toggledSounds.Remove(sound);
        }
        else if (state)
        {
            var instance = sound.CreateInstance();
            instance.Volume = SettingsSystem.GetTotalSFXVolume();
            instance.IsLooped = true;
            instance.Play();

            toggledSound = new ToggledSound();
            toggledSound.Instance = instance;
            toggledSound.CurrentUserCount = 1;
            toggledSounds.Add(sound, toggledSound);

            return instance;
        }

        return null;
    }

    public static void PauseAllToggledAudio()
    {
        foreach (var (sound, toggledSound) in toggledSounds)
        {
            if (toggledSound.Instance is null) continue;

            toggledSound.Instance.Pause();
        }
    }

    public static void ResumeAllToggledAudio()
    {
        foreach (var (sound, toggledSound) in toggledSounds)
        {
            if (toggledSound.Instance is null) continue;

            toggledSound.Instance.Resume();
        }
    }

    public static void StopAllToggledAudio()
    {
        foreach (var (sound, toggledSound) in toggledSounds)
        {
            if (toggledSound.Instance is null) continue;

            toggledSound.Instance.Stop();
            toggledSound.Instance.Dispose();
            toggledSounds.Remove(sound);
        }
    }
}
