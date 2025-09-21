using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class EnemySystem
{
    public static List<Enemy> Enemies { get; private set; } = new();
    static Game1 Game;


    public static void Initialize(Game1 game)
    {
        Game = game;
    }

    public static void Update(GameTime gameTime)
    {
        CheckIfEnemyPastLevel();
    }

    public static void CheckIfEnemyPastLevel()
    {
        float levelEndX = Game.Terrain.GetLastTilePosition().X;
        
        foreach (Enemy enemy in Enemies)
        {
            float enemyX = enemy.Position.X;
            if (enemyX > levelEndX)
            {

                //Console.WriteLine("Enemy has passed level");
            }
        }
    }


    public static Enemy SpawnWalkerEnemy(Game game, Vector2 position)
    {
        var movementData = new MovementSystem.MovementData
        {
            Pattern = MovementSystem.MovementPattern.Charge,
            CanWalk = true,
            WalkSpeed = 22f,
            JumpForce = 7f
        };

        var texture = AssetManager.GetTexture("goon");
        var hurtTexture = AssetManager.GetTexture("goon_hit");
        var frameSize = new Vector2(texture.Width / 8, texture.Height);

        var animationData = new AnimationSystem.AnimationData
        (
            texture: texture,
            frameCount: 8,
            frameSize: frameSize,
            delaySeconds: 0.1f
        );

        var enemy = new Enemy(game, position, frameSize, movementData, animationData, hurtTexture,
            health: 100, scrapValue: 1);
        Enemies.Add(enemy);
        game.Components.Add(enemy);

        return enemy;
    }

    public static Enemy SpawnFridgeEnemy(Game game, Vector2 position)
    {
        var movementData = new MovementSystem.MovementData
        {
            Pattern = MovementSystem.MovementPattern.Charge,
            CanWalk = true,
            WalkSpeed = 16f,
            JumpForce = 6f
        };

        var texture = AssetManager.GetTexture("fridge");
        var hurtTexture = AssetManager.GetTexture("fridge_hit");
        var frameSize = new Vector2(texture.Width / 8, texture.Height);

        var animationData = new AnimationSystem.AnimationData
        (
            texture: texture,
            frameCount: 8,
            frameSize: frameSize,
            delaySeconds: 0.1f
        );

        var enemy = new Enemy(game, position, frameSize, movementData, animationData, hurtTexture,
            health: 300, scrapValue: 5);
        Enemies.Add(enemy);
        game.Components.Add(enemy);

        return enemy;

    }
}
