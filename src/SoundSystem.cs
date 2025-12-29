using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace _2d_td;

public static class SoundSystem
{
    private record struct ToggledSound
    {
        public string SoundEffectName;
        public SoundEffectInstance Instance;
        public int CurrentUserCount;
    }

    private static Dictionary<SoundEffect, ToggledSound> toggledSounds = new();
    private static bool isSubscribedToSettings;

    // Important: Multipliers can't be greater than 1! Valid volume range is 0 to 1.
    private static Dictionary<string, float> sfxVolumeMultipliers = new()
    {
        { "coin", 0.3f },
        { "alarm", 0.5f },
        { "enemyHit1", 0.75f },
        { "enemyHit2", 0.75f },
        { "enemyHit3", 0.75f }
    };
    
    private static float GetFinalSoundVolume(string soundEffect)
    {
        var finalVolume = SettingsSystem.GetTotalSFXVolume();

        if (sfxVolumeMultipliers.TryGetValue(soundEffect, out var volumeMultiplier))
        {
            finalVolume *= volumeMultiplier;
        }

        return finalVolume;
    }

    private static void UpdatePlayingAudioVolume()
    {
        MediaPlayer.Volume = SettingsSystem.GetTotalMusicVolume();

        foreach (var (sound, toggledSound) in toggledSounds)
        {
            if (toggledSound.Instance is null) continue;

            toggledSound.Instance.Volume = GetFinalSoundVolume(toggledSound.SoundEffectName);
        }
    }

    static public SoundEffectInstance PlaySound(string name)
    {
        if (!isSubscribedToSettings)
        {
            SettingsScreen.OnSettingsSaved += () => UpdatePlayingAudioVolume();
            isSubscribedToSettings = true;
        }

        var sound = AssetManager.GetSound(name);
        var instance = sound.CreateInstance();
        instance.Volume = GetFinalSoundVolume(name);
        instance.Play();

        return instance;
    }

    static public SoundEffectInstance ToggleSound(string name, bool state)
    {
        if (!isSubscribedToSettings)
        {
            SettingsScreen.OnSettingsSaved += () => UpdatePlayingAudioVolume();
            isSubscribedToSettings = true;
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
            instance.Volume = GetFinalSoundVolume(name);
            instance.IsLooped = true;
            instance.Play();

            toggledSound = new ToggledSound();
            toggledSound.SoundEffectName = name;
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
        var keys = new SoundEffect[toggledSounds.Keys.Count];
        toggledSounds.Keys.CopyTo(keys, 0);

        foreach (var key in keys)
        {
            var toggledSound = toggledSounds[key];

            if (toggledSound.Instance is null) continue;

            toggledSound.Instance.Stop();
            toggledSound.Instance.Dispose();
            toggledSounds.Remove(key);
        }
    }

    public static void PlaySong(string name)
    {
        if (!isSubscribedToSettings)
        {
            SettingsScreen.OnSettingsSaved += () => UpdatePlayingAudioVolume();
            isSubscribedToSettings = true;
        }

        MediaPlayer.Volume = SettingsSystem.GetTotalMusicVolume();
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(AssetManager.GetSong(name));
    }

    public static void StopSong()
    {
        MediaPlayer.Stop();
    }
}