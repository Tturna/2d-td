using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace _2d_td
{
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
            public int currentLvl;
        }

        private static int currentWaveIndex;
        private static int maxWaveIndex;
        private static bool waveStarted;
        private static float waveCooldown;
        private static float waveCooldownLeft;

        // these variables are for the long term malliable script
        private static Zone currentZone;
        private static Wave currentWave;

        private static Game1 game;

        public static void Initialize(Game1 gameRef)
        {
            game = gameRef;

            Console.WriteLine("Loading zone 1 enemy data...");
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
                "data", "enemy-data", "zone1_waves.json");

            string wavesDataString = File.ReadAllText(wavesPath);
            var zone1_wavesJson = JsonSerializer.Deserialize<WaveJsonData[]>(wavesDataString);
            List<Wave> zone1_waves = new();

            foreach (var waveData in zone1_wavesJson)
            {
                var newWave = new Wave();
                newWave.maxFormations = waveData.maxFormations;
                newWave.formations = new List<Formation>();

                foreach (var formationName in waveData.formations)
                {
                    newWave.formations.Add(formations[formationName]);
                }

                zone1_waves.Add(newWave);
            }

            Console.WriteLine($"Loaded {zone1_waves.Count} waves with {formations.Count} formations");

            waveCooldown = 10f;
            waveCooldownLeft = 0f;
            currentWaveIndex = 0;
            waveStarted = true;
            var zone1 = new Zone { waves = zone1_waves, currentLvl = 1 };
            currentZone = zone1;
            currentWave = currentZone.waves[currentWaveIndex];

            int startingMax = 5;
            int maxIncrease = 5;
            maxWaveIndex = startingMax + (currentZone.currentLvl - 1) * maxIncrease;
        }

        public static void Update(GameTime gameTime)
        {
            if (game is null) return;

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
            if (currentZone.waves.Count <= currentWaveIndex) return;

            if (currentWave.spawnedFormations < currentWave.maxFormations)
            {
                Random random = new Random();
                Formation formation = new Formation { spawnCooldown = -1 };

                // formation picking logic

                float totalWeight = 0;
                for (int i = 0; i < currentWave.formations.Count; i++)
                {
                    totalWeight += currentWave.formations[i].weight;
                }

                do
                {
                    double randomVal = random.NextDouble() * totalWeight;
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

            if (EnemySystem.Enemies.Count == 0)
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
            Console.WriteLine("Wave " + currentWaveIndex + " Has Ended");
            waveStarted = false;
            waveCooldownLeft = waveCooldown;
            // called when the wave ends and will give the player time to build or wtv
        }

        private static void NextWave()
        { 
            currentWaveIndex++;
            currentWave = currentZone.waves[currentWaveIndex];
            Console.WriteLine("Wave " + currentWaveIndex + " Has Started");
            waveStarted = true;

            // starts next wave
        }
    }
}
