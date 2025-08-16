using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td
{
    public static class WaveSystem
    {
        public struct Formation
        {
            public int enemies;
        }

        public struct Wave
        {
            public List<Formation> formations;
        }

        public struct Zone
        {
            public List<Wave> waves;
            public int currentLvl;
        }

        private static Formation mockForm1;
        private static Formation mockForm2;
        private static Formation mockForm3;
        private static Zone zone1;
        private static Wave wave1;

        private static float formCooldown;
        private static float formCooldownRemaining;
        private static int currentWaveIndex;
        private static int currentFormationIndex;
        private static bool waveStarted;

        private static Game1 game;

        // Removed enemySystem variable - use EnemySystem's static methods directly

        public static void Initialize(Game1 gameRef)
        {
            game = gameRef;

            formCooldown = 5f;
            formCooldownRemaining = 0f;

            currentFormationIndex = 0;
            currentWaveIndex = 0;

            waveStarted = true;

            mockForm1 = new Formation { enemies = 4 };

            mockForm2 = new Formation { enemies = 2 };

            mockForm3 = new Formation { enemies = 6 };

            wave1 = new Wave { formations = new List<Formation> { mockForm1, mockForm2, mockForm3 } };

            zone1 = new Zone { waves = new List<Wave> { wave1 }, currentLvl = 1 };

            //StartWave(zone1);
        }

        public static void Update(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (formCooldownRemaining > 0f)
            {
                formCooldownRemaining -= elapsedSeconds;
            }

            if (formCooldownRemaining <= 0f && waveStarted)
            {
                SpawnNextFormation();
            }
        }

        private static void SpawnNextFormation()
        {
            //if (zone1 == null || zone1.waves.Count <= currentWaveIndex) return;

            Wave wave = zone1.waves[currentWaveIndex];

            if (currentFormationIndex < wave.formations.Count)
            {

                Formation formation = wave.formations[currentFormationIndex];

                

                SpawnFormation(formation);

                currentFormationIndex++; 
                formCooldownRemaining = formCooldown; 
            }
            // handle next wave if all formations are spawned
        }

        private static void SpawnFormation(Formation formation)
        {
            Console.WriteLine(formation.enemies);
            for (int i = 0; i < formation.enemies; i++)
            {
                EnemySystem.SpawnWalkerEnemy(game, new Vector2(i*10, 400));
            }
        }

        /*private static void StartWave(Zone zone)
        {
            foreach (Wave wave in zone.waves)
            {
                foreach (Formation formation in wave.formations)
                {
                    if (formCooldownRemaining <= 0f)
                    {
                        SpawnFormation(formation);
                    }
                }
            }
        }*/

        static void NextWave()
        {
            //starts next wave
            // zone1.waveNumber++; // waveNumber does not exist in your Zone struct
        }
    }
}