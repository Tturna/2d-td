namespace _2d_td;

public static class WaveSystem
{
    public struct Formation
    {
        public List<Enemy> enemies;
    }

    private List<Formation> wave;

    public struct Wave
    {
        public List<Formation> formations;
    }

    public struct Level
    {
        public List<Wave> waves;
        private int waveNumber = 1;
    }

    private Formation mockForm = new Formation();
    new protected Game1 game;

    

    public static void Initialize(Game1 gameRef)
    {
        game = gameRef;

        for (int i = 0; i < 5; i++) 
        {
            var Enemy mockEnemy = new Enemy(game,Vector2.One);
            mockForm.enemies.add(mockEnemy)
            Console.WriteLine("Added the " + i + "th enemy to the formation");
        }
    }

    void SpawnFormation(List<Enemy> formation)
    {
        foreach(Enemy in formation)
        {
            //spawn each enemy, enemy needs spawn function
        }
    }

    void SpawnWave()
    {
        //will call SpawnFormation() per formation in 
    }

    void NextWave()
    {
        //starts next wave
        waveNumber++;

    }


}