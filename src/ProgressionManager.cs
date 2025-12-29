namespace _2d_td;

public static class ProgressionManager
{
    public static int LastUnlockedZone { get; private set; } = 1;
    public static int LastUnlockedLevel { get; private set; } = 1;
    private static bool initialized;

    public static void Initialize()
    {
        if (initialized) return;

        initialized = true;
        WaveSystem.LevelWin += OnLevelWin;
    }

    public static bool IsZoneUnlocked(int zone)
    {
        return zone <= LastUnlockedZone;
    }

    public static bool IsLevelUnlocked(int zone, int level)
    {
        if (zone < LastUnlockedZone) return true;
        return IsZoneUnlocked(zone) && level <= LastUnlockedLevel;
    }

    private static void OnLevelWin(int zone, int wonLevel)
    {
        int maxZones = 5;
        int maxLevels = 5;

        if (wonLevel + 1 > maxLevels)
        {
            if (zone + 1 > maxZones)
            {
                // no more levels to unlock
                return;
            }

            UnlockLevel(zone + 1, 1);
            return;
        }

        UnlockLevel(zone, wonLevel + 1);
    }

    public static void UnlockLevel(int zone, int level, bool autoSave = true)
    {
        if (zone > LastUnlockedZone)
        {
            LastUnlockedZone = zone;
            LastUnlockedLevel = level;

            if (autoSave)
            {
                SavingSystem.SaveGame();
            }
        }
        else if (zone == LastUnlockedZone && level > LastUnlockedLevel)
        {
            LastUnlockedLevel = level;

            if (autoSave)
            {
                SavingSystem.SaveGame();
            }
        }
    }

    public static void UnlockNextLevel()
    {
        if (LastUnlockedZone == 5 && LastUnlockedLevel == 5) return;

        LastUnlockedLevel++;

        if (LastUnlockedLevel > 5)
        {
            LastUnlockedLevel = 1;
            LastUnlockedZone++;
        }

        SavingSystem.SaveGame();
    }
}
