namespace _2d_td;

public static class SettingsSystem
{
    public static float MasterVolume = 0.2f;
    public static float RawSoundEffectVolume = 1f;

    public static float GetTotalSFXVolume()
    {
        return RawSoundEffectVolume * MasterVolume;
    }
}
