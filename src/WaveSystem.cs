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

        //these are for testing
        private static Formation mockForm1;
        private static Formation mockForm2;
        private static Formation mockForm3;
        private static Zone zone1;
        private static Wave wave1;
        private static Wave wave2;

        private static float formCooldown;
        private static float formCooldownRemaining;
        private static int currentWaveIndex;
        private static int maxWaveIndex;
        private static int currentFormationIndex;
        private static bool waveStarted;
        private static float waveCd;
        private static float waveCdLeft;

        //these variables are for the long term malliable script
        private static Zone currentZone;



        private static Game1 game;

        // Removed enemySystem variable - use EnemySystem's static methods directly

        public static void Initialize(Game1 gameRef)
        {
            game = gameRef;

            formCooldown = 5f;
            formCooldownRemaining = 0f;

            waveCd = 10f;
            waveCdLeft = 0f;

            currentFormationIndex = 0;
            currentWaveIndex = 0;
            

            waveStarted = true;

            mockForm1 = new Formation { enemies = 4 };

            mockForm2 = new Formation { enemies = 2 };

            mockForm3 = new Formation { enemies = 6 };

            wave1 = new Wave { formations = new List<Formation> { mockForm1, mockForm2, mockForm3 } };

            wave2 = new Wave { formations = new List<Formation> {mockForm3, mockForm1,mockForm2,mockForm1,mockForm1} };

            zone1 = new Zone { waves = new List<Wave> { wave1,wave2 }, currentLvl = 1 };

            currentZone = zone1;

            maxWaveIndex = 5 + (currentZone.currentLvl-1) * 2;

            //StartWave(zone1);
        }

        public static void Update(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (formCooldownRemaining > 0f)
            {
                formCooldownRemaining -= elapsedSeconds;
            }

            if (waveCdLeft > 0f)
            {
                waveCdLeft -= elapsedSeconds;
            }


            if (waveCdLeft <= 0f && !waveStarted)
            {
                NextWave();
            }

            if (formCooldownRemaining <= 0f && waveStarted)
            {
                SpawnNextFormation();
            }
        }

        private static void SpawnNextFormation()
        {
            if (currentZone.waves.Count <= currentWaveIndex) return;

            Wave wave = currentZone.waves[currentWaveIndex];

            if (currentFormationIndex < wave.formations.Count)
            {

                Formation formation = wave.formations[currentFormationIndex];

                

                SpawnFormation(formation);

                currentFormationIndex++; 
                formCooldownRemaining = formCooldown; 
            }else if(EnemySystem.Enemies.Count == 0)
            {
                EndWave();
            }
            else
            {
                Console.WriteLine("Cannot Spawn Formation");
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

        public static void EndWave()
        {
            Console.WriteLine("Wave "+currentWaveIndex+" Has Ended");
            waveStarted = false;
            waveCdLeft = waveCd;
            currentFormationIndex = 0;
            //called when the wave ends and will give the player time to build or wtv
        }

        private static void NextWave()
        { 
            currentWaveIndex++;
            Console.WriteLine("Wave " + currentWaveIndex+" Has Started");
            waveStarted = true;

            //starts next wave
        }
    }
}