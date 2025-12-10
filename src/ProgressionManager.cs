namespace _2d_td;

public static class ProgressionManager
{
    private static int lastUnlockedZone = 1;
    private static int lastUnlockedLevel = 1;
    private static bool initialized;

    public static void Initialize()
    {
        if (initialized) return;

        initialized = true;
        WaveSystem.LevelWin += OnLevelWin;
    }

    public static bool IsZoneUnlocked(int zone)
    {
        return zone <= lastUnlockedZone;
    }

    public static bool IsLevelUnlocked(int zone, int level)
    {
        return IsZoneUnlocked(zone) && level <= lastUnlockedLevel;
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

    public static void UnlockLevel(int zone, int level)
    {
        if (zone > lastUnlockedZone)
        {
            lastUnlockedZone = zone;
            lastUnlockedLevel = level;
        }
        else if (zone == lastUnlockedZone && level > lastUnlockedLevel)
        {
            lastUnlockedLevel = level;
        }
    }

    public static void UnlockNextLevel()
    {
        if (lastUnlockedZone == 5 && lastUnlockedLevel == 5) return;

        lastUnlockedLevel++;

        if (lastUnlockedLevel > 5)
        {
            lastUnlockedLevel = 1;
            lastUnlockedZone++;
        }
    }
}
