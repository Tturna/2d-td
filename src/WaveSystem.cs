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
        }

        private static List<Formation> wave;

        public struct Wave
        {
            public List<Formation> formations;
        }

        public struct Level
        {
            public List<Wave> waves;
            public int waveNumber;
        }

        private static Formation mockForm = new Formation { enemies = new List<Enemy>() };
        private static Game1 game;

        public static void Initialize(Game1 gameRef)
        {
            game = gameRef;

            for (int i = 0; i < 5; i++)
            {
                Enemy mockEnemy = new Enemy(game, Vector2.One);
                mockForm.enemies.Add(mockEnemy);
                Console.WriteLine("Added the " + i + "th enemy to the formation");
            }
        }

        static void SpawnFormation(List<Enemy> formation)
        {
            foreach (Enemy enemy in formation)
            {
                //spawn each enemy, enemy needs spawn function
            }
        }

        static void SpawnWave()
        {
            //will call SpawnFormation() per formation in 
        }

        static void NextWave()
        {
            //starts next wave
            Level.waveNumber++;
        }
    }
}
