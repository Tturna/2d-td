using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td
{
    public static class WaveSystem
    {
        public enum enemy
        {
            Walker,
            Fridge
        }

        public struct Formation
        {
            public List<enemy> enemies; // or enemyWeights or whatever you wanna do
            public float cooldown;//time waited to spawn new formation
            public float weight; // if we want weighted randomization of formations. could just be fully random too
            public int spawnCooldown; // how many formations have to spawn before this one can spawn again (unless none left or smn)
            public int spawnCDRemaining;
        }

        // define formations somewhere. could just be a big function or some C# script

        public struct Wave
        {
            public List<Formation> formations;
            public int spawnedFormation;
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
            /*mockForm1 = new Formation { enemies = 4 };
            mockForm2 = new Formation { enemies = 2 };
            mockForm3 = new Formation { enemies = 6 };
            wave1 = new Wave { formations = new List<Formation> { mockForm1, mockForm2, mockForm3 } };
            wave2 = new Wave { formations = new List<Formation> { mockForm3, mockForm1, mockForm2, mockForm1, mockForm1 } };
            zone1 = new Zone { waves = new List<Wave> { wave1, wave2 }, currentLvl = 1 };*/
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
            {
                currentWave.formCooldownRemaining -= elapsedSeconds;
            }

            if (waveCdLeft > 0f)
            {
                waveCdLeft -= elapsedSeconds;
            }

            if (waveCdLeft <= 0f && !waveStarted)
            {
                NextWave();
            }

            if (currentWave.formCooldownRemaining <= 0f && waveStarted)
            {
                SpawnNextFormation();
            }
        }

        private static void SpawnNextFormation()
        {
            if (currentZone.waves.Count <= currentWaveIndex) return;

            Wave wave = currentZone.waves[currentWaveIndex];

            if (wave.spawnedFormation < wave.maxFormations)
            {
                Random random = new Random();
                Formation formation = wave.formations[0];
                //formation picking logic


                float totalWeight = 0;
                for (int i = 0; i < wave.formations.Count; i++)
                {
                    totalWeight += wave.formations[i].weight;
                }
                double randomVal = random.NextDouble() * totalWeight;

                double cumulative = 0;
                for (int i = 0; i < wave.formations.Count; i++)
                {
                    cumulative += wave.formations[i].weight;
                    if (randomVal < cumulative&&wave.formations[i].spawnCDRemaining==0)
                        formation = wave.formations[i];
                }
                //Formation formation = wave.formations[currentFormationIndex];
                SpawnFormation(formation);

                //currentFormationIndex++; 
                wave.formCooldownRemaining = formation.cooldown; 
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
            UpdateFormationCooldowns();
            for (int i = 0; i < formation.enemies.Count; i++)
            {
                EnemySystem.SpawnEnemy(game, new Vector2(i * 10, 400), (int)formation.enemies[i]);
            }
            formation.spawnCDRemaining = formation.spawnCooldown;
        }

        private static void UpdateFormationCooldowns()
        {
            for(int i = 0; i < currentWave.formations.Count;i++)
            {
                Formation f = currentWave.formations[i];
                if (f.spawnCDRemaining != 0)
                    f.spawnCDRemaining -= 1;
            }
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
