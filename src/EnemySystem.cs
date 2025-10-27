using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class EnemySystem
{
    public static QuadTree<Enemy> EnemyTree { get; private set; }
    static Game1 Game;

    public static Dictionary<string, EnemySpawner> EnemyNameToSpawner = new()
    {
        { "walker", SpawnWalkerEnemy },
        { "fridge", SpawnFridgeEnemy }
    };

    public static void Initialize(Game1 game)
    {
        Game = game;
        var mainBounds = new Rectangle(-game.NativeScreenWidth, 0,
            game.NativeScreenWidth * 3, game.NativeScreenHeight * 2);
        EnemyTree = new QuadTree<Enemy>(mainBounds);
    }

    public static void Update(GameTime gameTime)
    {
    }

    public delegate Enemy EnemySpawner(Game game, Vector2 position);

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
        EnemyTree.Add(enemy);

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
        EnemyTree.Add(enemy);

        return enemy;
    }
}
