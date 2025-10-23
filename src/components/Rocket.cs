using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class Rocket : Projectile
{
    private List<Enemy> hitEnemies = new();
    private int maxHitEnemies;

    public Rocket(Game game, Vector2 startLocation) : base(game, startLocation)
    {
        // Rocket specific defaults
        BulletPixelsPerSecond = 200f;
        BulletLength = 24f;
        BulletWidth = 4f;
        Damage = 50;
        ExplosionTileRadius = 2;
        maxHitEnemies = 5;
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var oldPosition = Position;
        Position += Direction * (BulletPixelsPerSecond * deltaTime);

        RotationRadians = (float)Math.Atan2(Direction.Y, Direction.X);

        // checks 3 times, once on the center of the bullet
        // and on the sides of the bullet 
        var rad = (float)Math.PI / 2;
        var perpendicularDirection = Direction;
        perpendicularDirection.Rotate(rad);

        var sideOneOffset = perpendicularDirection * BulletWidth;
        var sideTwoOffset = -perpendicularDirection * BulletWidth;

        if (hitEnemies.Count >= maxHitEnemies)
        {
            HandleExplosion();
            Destroy();
        }

        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            if (Collision.IsLineInEntity(oldPosition, Position, enemy,
                out Vector2 entryPoint, out Vector2 exitPoint))
            {
                hitEnemies.Add(enemy);
            }
            else if (Collision.IsLineInEntity(oldPosition + sideOneOffset,
                Position + sideOneOffset, enemy, out entryPoint, out exitPoint))
            {
                hitEnemies.Add(enemy);
            }
            else if (Collision.IsLineInEntity(oldPosition + sideTwoOffset,
            Position + sideTwoOffset, enemy, out entryPoint, out exitPoint))
            {
                hitEnemies.Add(enemy);
            }

        }
        Lifetime -= deltaTime;
    }
    private void HandleExplosion()
    {
        var explosionCenter = Position + Direction * BulletLength;

        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            var distance = Vector2.Distance(explosionCenter, enemy.Position + enemy.Size / 2);
            var tileDistance = distance / Grid.TileLength;

            if (tileDistance <= ExplosionTileRadius)
            {
                enemy.HealthSystem.TakeDamage(Damage);
            }
        }
    }
}