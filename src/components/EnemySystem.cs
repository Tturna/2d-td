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
            WalkSpeed = 40f,
            JumpForce = 12f
        };

        var enemy = new Enemy(game, position, movementData);
        Enemies.Add(enemy);
        game.Components.Add(enemy);

        return enemy;
    }
}
