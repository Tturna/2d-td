using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td
{
    public static class WaveSystem
    {
        public struct Formation
        {
            public List<Enemy> enemies;
            //public int frequency
        }

        public struct Wave
        {
            public List<Formation> formations;
            //public int formationCooldown;
        }

        public struct Zone
        {
            public List<Wave> waves;
            public int currentLvl;
        }

        private static Formation mockForm;

        private static Zone zone1;

        private static Wave wave1;

        private float formCooldown;

        private float formCooldownRemaining;

        private int currentWave;

        private static Game1 game;

        public static void Initialize(Game1 gameRef)
        {
            game = gameRef

            formCooldownTime = 5f;

            mockForm = new Formation { enemies = new List<Enemy>() };

            wave1 = new Wave { formations = new List<Formation>(mockForm) };

            Zone1 = new Zone { waves = new List<Wave>(wave1), currentLvl = 0 }

            for (int i = 0; i < 5; i++)
            {
                Enemy mockEnemy = new Enemy(game, Vector2.One);
                mockForm.enemies.Add(mockEnemy);
                Console.WriteLine("Added the " + i + "th enemy to the formation");
            }


        }

        public override void Update(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (formCooldownRemaining > 0f) 
            {
                formCooldownRemaining -= elapsedSeconds;
            }
        }

        static void SpawnFormation(Formation formation)
        {
            formCooldownRemaining = formCooldown;

            foreach (Enemy enemy in formation)
            {
                //spawn each enemy, enemy needs spawn function
            }
        }

        static void StartWave(Zone zone)
        {
            foreach (Formation formation in zone.waves)
            {
                if (formCooldownRemaining <= 0f)
                {
                    SpawnFormation(formation);
                }
            }
        }

        static void NextWave()
        {
            //starts next wave
            Zone1.waveNumber++;
        }
    }
}
