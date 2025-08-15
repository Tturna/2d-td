using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class EnemySystem
{
    public static List<Enemy> Enemies { get; private set; } = new();

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
        var frameSize = new Vector2(texture.Width / 8, texture.Height);

        var animationData = new AnimationSystem.AnimationData
        {
            Texture = texture,
            FrameCount = 8,
            FrameSize = frameSize,
            DelaySeconds = 0.1f
        };

        var enemy = new Enemy(game, position, frameSize, movementData, animationData);
        Enemies.Add(enemy);
        game.Components.Add(enemy);

        return enemy;
    }
}
