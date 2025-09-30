using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td
{
    public static class WaveSystem
    {
        public struct Formation
        {
            public List<EnemySystem.EnemySpawner> enemies; // or enemyWeights or whatever you wanna do
            public float cooldown;//time waited to spawn new formation
            public float weight; // if we want weighted randomization of formations. could just be fully random too
            public int spawnCooldown; // how many formations have to spawn before this one can spawn again (unless none left or smn)
            public int spawnCDRemaining;
        }

        // define formations somewhere. could just be a big function or some C# script

        public struct Wave
        {
            public List<Formation> formations;
            public int spawnedFormations;
            public int maxFormations;
            public float formCooldownRemaining;
        }

        public struct Zone
        {
            public List<Wave> waves;
            public int currentLvl;
        }

        //these are for testing
        private static Formation mockForm1;
        private static Formation mockForm2;
        private static Formation mockForm3;
        private static Formation mockForm4;
        private static Zone zone1;
        private static Wave wave1;
        private static Wave wave2;

        //private static float formCooldown;
        private static int currentWaveIndex;
        private static int maxWaveIndex;
        //private static int currentFormationIndex;
        private static bool waveStarted;
        private static float waveCd;
        private static float waveCdLeft;

        // these variables are for the long term malliable script
        private static Zone currentZone;
        private static Wave currentWave;

        private static Game1 game;
        

        // Removed enemySystem variable - use EnemySystem's static methods directly

        public static void Initialize(Game1 gameRef)
        {
            game = gameRef;
            waveCd = 10f;
            waveCdLeft = 0f;
            currentWaveIndex = 0;
            waveStarted = true;
            mockForm1 = new Formation { enemies = new List<EnemySystem.EnemySpawner> {EnemySystem.SpawnWalkerEnemy,EnemySystem.SpawnWalkerEnemy,EnemySystem.SpawnFridgeEnemy},cooldown =3,weight = 1,spawnCooldown=2};
            mockForm2 = new Formation { enemies = new List<EnemySystem.EnemySpawner> {EnemySystem.SpawnWalkerEnemy,EnemySystem.SpawnWalkerEnemy,EnemySystem.SpawnWalkerEnemy,EnemySystem.SpawnWalkerEnemy,EnemySystem.SpawnWalkerEnemy,EnemySystem.SpawnWalkerEnemy},cooldown =2,weight = 3,spawnCooldown=1};
            mockForm3 = new Formation { enemies =new List<EnemySystem.EnemySpawner> {EnemySystem.SpawnFridgeEnemy,EnemySystem.SpawnFridgeEnemy},cooldown =3,weight = 2,spawnCooldown=3 };
            mockForm4 = new Formation { enemies =new List<EnemySystem.EnemySpawner> {EnemySystem.SpawnWalkerEnemy},cooldown =.5f,weight = 2.7f,spawnCooldown=0 };
            wave1 = new Wave { formations = new List<Formation> { mockForm1,mockForm2,mockForm3,mockForm4},maxFormations=5};
            wave2 = new Wave { formations = new List<Formation> { mockForm1 },maxFormations=5};
            zone1 = new Zone { waves = new List<Wave> { wave1, wave2 }, currentLvl = 1 };
            currentZone = zone1;
            currentWave = currentZone.waves[currentWaveIndex];
            int startingMax = 5;
            int maxIncrease = 2;
            maxWaveIndex = startingMax + (currentZone.currentLvl - 1) * maxIncrease;
        }

        public static void Update(GameTime gameTime)
        {
            if (game is null) return;

            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (currentWave.formCooldownRemaining > 0f)
                //Console.WriteLine(currentWave.formCooldownRemaining);
                currentWave.formCooldownRemaining -= elapsedSeconds;

            if (waveCdLeft > 0f)
                waveCdLeft -= elapsedSeconds;

            if (waveCdLeft <= 0f && !waveStarted)
                NextWave();

            if (currentWave.formCooldownRemaining <= 0f && waveStarted)
                SpawnNextFormation();
        }

        private static void SpawnNextFormation()
        {
            if (currentZone.waves.Count <= currentWaveIndex) return;

            if (currentWave.spawnedFormations <= currentWave.maxFormations)
            {
                Random random = new Random();
                Formation formation = new Formation { spawnCooldown = -1 };
                //formation picking logic


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

                        Console.WriteLine("Cumulative: " + cumulative + " Random Val: " + randomVal + " Formation cooldown: " + formation.spawnCDRemaining);
                        if (randomVal < cumulative && currentWave.formations[i].spawnCDRemaining == 0)
                        {
                            Console.WriteLine("Formation" + (i+1)+ " Spawned!");
                            formation = currentWave.formations[i];
                            formation.spawnCDRemaining = formation.spawnCooldown;
                            currentWave.formations[i] = formation;
                            break;
                        }
                        currentWave.formations[i] = UpdateFormationCooldown(currentWave.formations[i]);
                    }
                } while (formation.spawnCooldown == -1);
                currentWave.spawnedFormations++;
                currentWave.formCooldownRemaining = formation.cooldown;
                SpawnFormation(formation);
                     
            } else if (EnemySystem.Enemies.Count == 0)
            {
                EndWave();
            }
            else
            {
                //Console.WriteLine("Cannot Spawn Formation");
            }
            // handle next wave if all formations are spawned
        }

        private static void SpawnFormation(Formation formation)
        {
            Console.WriteLine("-------enemy spawned-------");
            int positionIndex = 1;
            foreach (var spawner in formation.enemies)
            {
                spawner?.Invoke(game, new Vector2(positionIndex * 10, 400));
                positionIndex++;
            }
            
        }

        private static Formation UpdateFormationCooldown(Formation f)
        {
            if (f.spawnCDRemaining != 0)
                f.spawnCDRemaining -= 1;
            return f;
        }

        public static void EndWave()
        {
            Console.WriteLine("Wave " + currentWaveIndex + " Has Ended");
            waveStarted = false;
            waveCdLeft = waveCd;
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
