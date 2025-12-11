using System;
using System.IO;
using System.Text.Json;

namespace _2d_td;

public static class SavingSystem
{
    private record struct SaveData
    {
        public int UnlockedZone { get; set; }
        public int UnlockedLevel { get; set; }
    }

    public static void SaveGame()
    {
        string saveDir = Path.Combine(AppContext.BaseDirectory, "saves");
        string savePath = Path.Combine(saveDir, "save.json");

        if (File.Exists(savePath))
        {
            // TODO: consider handling exceptions like maybe missing perms
            File.Delete(savePath);
        }

        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }

        var fileStream = File.Create(savePath);
        fileStream.Close();

        var saveData = new SaveData();
        saveData.UnlockedZone = ProgressionManager.LastUnlockedZone;
        saveData.UnlockedLevel = ProgressionManager.LastUnlockedLevel;
        var saveJson = JsonSerializer.Serialize(saveData);
        File.WriteAllText(savePath, saveJson);

        Console.WriteLine("Game saved");
    }

    public static void LoadGame()
    {
        string savePath = Path.Combine(AppContext.BaseDirectory, "saves", "save.json");

        if (!File.Exists(savePath))
        {
            Console.WriteLine("Couldn't find save file! Loading failed!");
            return;
        }

        var saveJson = File.ReadAllText(savePath);
        var saveData = JsonSerializer.Deserialize<SaveData>(saveJson);

        ProgressionManager.UnlockLevel(saveData.UnlockedZone, saveData.UnlockedLevel);

        Console.WriteLine("Save loaded");
    }
}
