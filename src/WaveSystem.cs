public static class WaveSystem
{
    public struct Formation
    {
        public List<Enemy> Enemies;
    }

    private List<Formation> wave;

    private int waveNumber = 1;



    void SpawnFormation(List<Enemy> formation)
    {
        
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