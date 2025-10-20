using System.Collections.Generic;

namespace _2d_td
{
    public static class ZoneData
    {
        static WaveSystem.Formation easyForm1 = new WaveSystem.Formation
        {
            enemies = new List<EnemySystem.EnemySpawner> { EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnWalkerEnemy },
            cooldown = 2f,
            weight = 2f,
            spawnCooldown = 2
        };

        static WaveSystem.Formation easyForm2 = new WaveSystem.Formation
        {
            enemies = new List<EnemySystem.EnemySpawner> { EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnFridgeEnemy },
            cooldown = 3f,
            weight = 1.7f,
            spawnCooldown = 3
        };
        static WaveSystem.Formation easyForm3 = new WaveSystem.Formation
        {
            enemies = new List<EnemySystem.EnemySpawner> { EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnWalkerEnemy,EnemySystem.SpawnWalkerEnemy },
            cooldown = 2.5f,
            weight = 1.8f,
            spawnCooldown = 2
        };
        static WaveSystem.Formation medForm1 = new WaveSystem.Formation
        {
            enemies = new List<EnemySystem.EnemySpawner> { EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnFridgeEnemy,EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnWalkerEnemy },
            cooldown = 3.5f,
            weight = 1.3f,
            spawnCooldown = 3
        };
        static WaveSystem.Formation medForm2 = new WaveSystem.Formation
        {
            enemies = new List<EnemySystem.EnemySpawner> { EnemySystem.SpawnFridgeEnemy, EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnWalkerEnemy,EnemySystem.SpawnWalkerEnemy },
            cooldown = 2f,
            weight = 1.5f,
            spawnCooldown = 3
        };
        static WaveSystem.Formation medForm3 = new WaveSystem.Formation
        {
            enemies = new List<EnemySystem.EnemySpawner> { EnemySystem.SpawnFridgeEnemy, EnemySystem.SpawnFridgeEnemy},
            cooldown = 1f,
            weight = 1.2f,
            spawnCooldown = 4
        };
        static WaveSystem.Formation hardForm1 = new WaveSystem.Formation
        {
            enemies = new List<EnemySystem.EnemySpawner> { EnemySystem.SpawnFridgeEnemy, EnemySystem.SpawnFridgeEnemy, EnemySystem.SpawnWalkerEnemy,EnemySystem.SpawnFridgeEnemy },
            cooldown = 3f,
            weight = 1f,
            spawnCooldown = 5
        };
        static WaveSystem.Formation hardForm2 = new WaveSystem.Formation
        {
            enemies = new List<EnemySystem.EnemySpawner> { EnemySystem.SpawnFridgeEnemy, EnemySystem.SpawnFridgeEnemy, EnemySystem.SpawnFridgeEnemy },
            cooldown = 3f,
            weight = 0.8f,
            spawnCooldown = 6
        };
        static WaveSystem.Formation hardForm3 = new WaveSystem.Formation
        {
            enemies = new List<EnemySystem.EnemySpawner> { EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnFridgeEnemy, EnemySystem.SpawnFridgeEnemy,EnemySystem.SpawnWalkerEnemy, EnemySystem.SpawnFridgeEnemy },
            cooldown = 1f,
            weight = 0.9f,
            spawnCooldown = 5
        };
        public static WaveSystem.Zone[] Zones = {
            new WaveSystem.Zone {
                waves = new List<WaveSystem.Wave> {
                    new WaveSystem.Wave {
                        formations = new List<WaveSystem.Formation> { easyForm1, easyForm2, easyForm3},
                        maxFormations = 5
                    },
                    new WaveSystem.Wave {
                        formations = new List<WaveSystem.Formation> {easyForm1, easyForm2, medForm2, medForm3},
                        maxFormations = 7
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { easyForm1, easyForm3, medForm2 },
                        maxFormations = 5
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { easyForm2, medForm1, easyForm3, medForm3 },
                        maxFormations = 6
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { medForm1, easyForm2, medForm2, easyForm3, medForm3 },
                        maxFormations = 7
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { easyForm1, medForm2, easyForm3, medForm3, easyForm2 },
                        maxFormations = 6
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { medForm1, medForm3, easyForm1, easyForm2, medForm2 },
                        maxFormations = 7
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { easyForm3, medForm2, medForm1, easyForm2 },
                        maxFormations = 6
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { medForm3, easyForm1, medForm2, easyForm3, medForm1 },
                        maxFormations = 8
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { medForm2, hardForm1, easyForm3, medForm3 },
                        maxFormations = 7
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm2, medForm1, easyForm2, medForm3, hardForm3 },
                        maxFormations = 8
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { medForm2, hardForm1, easyForm3, medForm3, easyForm1 },
                        maxFormations = 9
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm3, medForm1, medForm2, easyForm2, hardForm1 },
                        maxFormations = 8
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { medForm3, hardForm2, easyForm1, medForm2, hardForm1 },
                        maxFormations = 9
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm1, medForm3, easyForm2, hardForm3 },
                        maxFormations = 8
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm2, medForm2, easyForm3, medForm1, hardForm3 },
                        maxFormations = 9
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm1, medForm3, hardForm2, easyForm1, medForm2 },
                        maxFormations = 10
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm3, medForm1, hardForm2, easyForm2 },
                        maxFormations = 9
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm1, hardForm3, medForm2, easyForm3, medForm1 },
                        maxFormations = 10
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm2, medForm3, hardForm1, easyForm1 },
                        maxFormations = 9
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm3, hardForm1, medForm2, hardForm2 },
                        maxFormations = 10
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm2, hardForm3, medForm1, easyForm2, hardForm1 },
                        maxFormations = 10
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm1, hardForm3, medForm3, easyForm1 },
                        maxFormations = 10
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm2, hardForm1, medForm2, hardForm3 },
                        maxFormations = 10
                    },
                    new WaveSystem.Wave
                    {
                        formations = new List<WaveSystem.Formation> { hardForm3, hardForm2, hardForm1, medForm2 },
                        maxFormations = 10
                    },
                },
                currentLvl = 1
            }
        };
    }

}
