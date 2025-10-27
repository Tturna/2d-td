using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class WaveSystem
{
    public struct EnemyGroup
    {
        public string enemyName { get; set; }
        public int spawnCount { get; set; }
    }

    public struct Formation
    {
        public List<EnemySystem.EnemySpawner> enemies;
        public float cooldown; // time waited to spawn new formation
        public float weight;
        public int spawnCooldown; // how many formations have to spawn before this one can spawn again (unless none left or smn)
        public int spawnCooldownRemaining;
    }

    public struct FormationJsonData
    {
        public string formationName { get; set; }
        public EnemyGroup[] enemyGroups { get; set; }
        public float cooldown { get; set; }
        public float weight { get; set; }
        public int spawnCooldown { get; set; }
    }

    public struct Wave
    {
        public List<Formation> formations;
        public int spawnedFormations;
        public int maxFormations;
        public float formCooldownRemaining;
    }

    public struct WaveJsonData
    {
        public List<string> formations { get; set; }
        public int maxFormations { get; set; }
    }

    public struct Zone
    {
        public List<Wave> waves;
    }

    public delegate void WaveEndedHandler();
    public delegate void LevelWinHandler();
    public static event LevelWinHandler LevelWin;
    public static event WaveEndedHandler WaveEnded;
    public static int MaxWaveIndex;
    public static int CurrentWaveIndex;

    private static bool waveStarted;
    private static float waveCooldown;
    private static float waveCooldownLeft;
    private const int StartingMaxWaves = 5;
    private const int MaxWaveIncreasePerLevel = 5;

    // these variables are for the long term malliable script
    private static Zone currentZone;
    private static Wave currentWave;

    private static Game1 game;

    public static void Initialize(Game1 gameRef, int currentZoneNumber, int currentLevelNumber)
    {
        game = gameRef;

        Console.WriteLine($"Loading zone {currentZoneNumber} enemy data...");
        string formationsPath = Path.Combine(AppContext.BaseDirectory, game.Content.RootDirectory,
                "data", "enemy-data", "formations.json");

        string formationsDataString = File.ReadAllText(formationsPath);
        var formationsJson = JsonSerializer.Deserialize<FormationJsonData[]>(formationsDataString);
        Dictionary<string, Formation> formations = new();

        foreach (var formationData in formationsJson)
        {
            var newFormation = new Formation();
            newFormation.enemies = new List<EnemySystem.EnemySpawner>();
            newFormation.cooldown = formationData.cooldown;
            newFormation.weight = formationData.weight;
            newFormation.spawnCooldown = formationData.spawnCooldown;

            foreach (var enemyCountPair in formationData.enemyGroups)
            {
                for (var i = 0; i < enemyCountPair.spawnCount; i++)
                {
                    newFormation.enemies.Add(EnemySystem.EnemyNameToSpawner[enemyCountPair.enemyName]);
                }
            }

            formations[formationData.formationName] = newFormation;
        }

        string wavesPath = Path.Combine(AppContext.BaseDirectory, game.Content.RootDirectory,
                "data", "enemy-data", $"zone{currentZoneNumber}_waves.json");

        string wavesDataString = File.ReadAllText(wavesPath);
        var wavesJson = JsonSerializer.Deserialize<WaveJsonData[]>(wavesDataString);
        List<Wave> waves = new();

        foreach (var waveData in wavesJson)
        {
            var newWave = new Wave();
            newWave.maxFormations = waveData.maxFormations;
            newWave.formations = new List<Formation>();

            foreach (var formationName in waveData.formations)
            {
                newWave.formations.Add(formations[formationName]);
            }

            waves.Add(newWave);
        }

        Console.WriteLine($"Loaded {waves.Count} waves with {formations.Count} formations");

        waveCooldown = 10f;
        waveCooldownLeft = 0f;
        CurrentWaveIndex = 0;
        waveStarted = true;
        var zone1 = new Zone { waves = waves };
        currentZone = zone1;
        currentWave = currentZone.waves[CurrentWaveIndex];

        MaxWaveIndex = StartingMaxWaves + (currentLevelNumber - 1) * MaxWaveIncreasePerLevel;
    }

    public static void Update(GameTime gameTime)
    {
        if (game is null) return;
        if (!waveStarted && CurrentWaveIndex >= MaxWaveIndex - 1) return;

        float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (currentWave.formCooldownRemaining > 0f)
            currentWave.formCooldownRemaining -= elapsedSeconds;

        if (waveCooldownLeft > 0f)
            waveCooldownLeft -= elapsedSeconds;

        if (waveCooldownLeft <= 0f && !waveStarted)
            NextWave();

        if (currentWave.formCooldownRemaining <= 0f && waveStarted)
            SpawnNextFormation();
    }

    private static void SpawnNextFormation()
    {
        if (currentWave.spawnedFormations < currentWave.maxFormations)
        {
            Formation formation = new Formation { spawnCooldown = -1 };

            // formation picking logic

            float totalWeight = 0;
            for (int i = 0; i < currentWave.formations.Count; i++)
            {
                totalWeight += currentWave.formations[i].weight;
            }

            do
            {
                double randomVal = Random.Shared.NextDouble() * totalWeight;
                double cumulative = 0;

                for (int i = 0; i < currentWave.formations.Count; i++)
                {
                    cumulative += currentWave.formations[i].weight;

                    // Console.WriteLine("Cumulative: " + cumulative + " Random Val: " + randomVal + " Formation cooldown: " + formation.spawnCooldownRemaining);

                    if (randomVal < cumulative && currentWave.formations[i].spawnCooldownRemaining == 0)
                    {
                        Console.WriteLine("Formation" + (i + 1)+ " Spawned!");
                        formation = currentWave.formations[i];
                        formation.spawnCooldownRemaining = formation.spawnCooldown;
                        currentWave.formations[i] = formation;
                        break;
                    }

                    currentWave.formations[i] = UpdateFormationCooldown(currentWave.formations[i]);
                }
            } while (formation.spawnCooldown == -1);

            currentWave.spawnedFormations++;
            currentWave.formCooldownRemaining = formation.cooldown;
            SpawnFormation(formation);
            return;
        }

        if (EnemySystem.EnemyTree.IsEmpty())
        {
            EndWave();
            return;
        }

        // Do nothing while wave is active
    }

    private static void SpawnFormation(Formation formation)
    {
        int positionIndex = 1;

        foreach (var spawner in formation.enemies)
        {
            spawner?.Invoke(game, new Vector2(positionIndex * 10, 400));
            positionIndex++;
        }
    }

    private static Formation UpdateFormationCooldown(Formation f)
    {
        if (f.spawnCooldownRemaining != 0)
            f.spawnCooldownRemaining -= 1;

        return f;
    }

    public static void EndWave()
    {
        Console.WriteLine("Wave " + CurrentWaveIndex + " Has Ended");
        waveStarted = false;
        waveCooldownLeft = waveCooldown;
        // called when the wave ends and will give the player time to build or wtv

        WaveEnded?.Invoke();

        if (CurrentWaveIndex == MaxWaveIndex - 1)
        {
            LevelWin?.Invoke();
        }
    }

    private static void NextWave()
    { 
        CurrentWaveIndex++;
        currentWave = currentZone.waves[CurrentWaveIndex];
        Console.WriteLine("Wave " + CurrentWaveIndex + " Has Started");
        waveStarted = true;

        // starts next wave
    }
}
