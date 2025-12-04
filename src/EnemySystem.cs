using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class EnemySystem
{
    public static BinGrid<Enemy> EnemyBins;
    static Game1 Game;

    public static Dictionary<string, EnemySpawner> EnemyNameToSpawner = new()
    {
        { "node", SpawnNodeEnemy },
        { "bouncer", SpawnBouncerEnemy },
        { "meganode", SpawnMeganodeEnemy }
    };

    public static void Initialize(Game1 game)
    {
        Game = game;
        var mainBounds = new Rectangle(-game.NativeScreenWidth, 0,
            game.NativeScreenWidth * 3, game.NativeScreenHeight * 2);
        EnemyBins = new BinGrid<Enemy>(Grid.TileLength * 4);
    }

    public static void Update(GameTime gameTime)
    {
    }

    public delegate Enemy EnemySpawner(Game game, Vector2 position);

    public static Enemy SpawnNodeEnemy(Game game, Vector2 position)
    {
        var movementData = new MovementSystem.MovementData
        {
            Pattern = MovementSystem.MovementPattern.Charge,
            CanWalk = true,
            WalkSpeed = 0.35f,
            JumpForce = 7f
        };

        var texture = AssetManager.GetTexture("node");
        var frameSize = new Vector2(texture.Width, texture.Height);

        var animationData = new AnimationSystem.AnimationData
        (
            texture: texture,
            frameCount: 1,
            frameSize: frameSize,
            delaySeconds: float.PositiveInfinity
        );

        var enemy = new Enemy(game, position, frameSize, movementData, animationData,
            health: 100, scrapValue: 1);
        enemy.DrawOffset = enemy.Size / 2;
        enemy.DrawOrigin = enemy.Size / 2;
        EnemyBins.Add(enemy);

        return enemy;
    }

    public static Enemy SpawnBouncerEnemy(Game game, Vector2 position)
    {
        var movementData = new MovementSystem.MovementData
        {
            Pattern = MovementSystem.MovementPattern.JumpForward,
            CanWalk = true,
            WalkSpeed = 0.4f,
            JumpForce = 8f
        };

        var texture = AssetManager.GetTexture("bouncer");
        var frameSize = new Vector2(texture.Width, texture.Height);

        var animationData = new AnimationSystem.AnimationData
        (
            texture: texture,
            frameCount: 1,
            frameSize: frameSize,
            delaySeconds: float.PositiveInfinity
        );

        var enemy = new Enemy(game, position, frameSize, movementData, animationData,
            health: 100, scrapValue: 1);
        enemy.DrawOffset = enemy.Size / 2;
        enemy.DrawOrigin = enemy.Size / 2;
        enemy.PhysicsSystem.LocalGravity = 0.3f;
        enemy.PhysicsSystem.DragFactor = 0.02f;
        EnemyBins.Add(enemy);

        return enemy;
    }

    public static Enemy SpawnMeganodeEnemy(Game game, Vector2 position)
    {
        var movementData = new MovementSystem.MovementData
        {
            Pattern = MovementSystem.MovementPattern.Charge,
            CanWalk = true,
            WalkSpeed = 0.2f,
            JumpForce = 6f
        };

        var texture = AssetManager.GetTexture("meganode");
        var frameSize = new Vector2(texture.Width, texture.Height);

        var animationData = new AnimationSystem.AnimationData
        (
            texture: texture,
            frameCount: 1,
            frameSize: frameSize,
            delaySeconds: float.PositiveInfinity
        );

        var enemy = new Enemy(game, position, frameSize, movementData, animationData,
            health: 300, scrapValue: 5);
        EnemyBins.Add(enemy);

        return enemy;
    }
}
