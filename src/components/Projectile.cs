using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class Projectile : Entity
{
    public Vector2 Direction = Vector2.Zero;
    public float Lifetime = 1f;
    public float BulletPixelsPerSecond = 0f;
    public float BulletLength = 16f;
    public float BulletWidth = 2f;
    public int Damage = 0;
    public int Pierce = 1;
    private List<Enemy> hitEnemies = new();

    // this constructor is simple so that the turrets can edit the property
    // of bullet themself
    public Projectile(Game game, Vector2 startLocation) : base(game, null, Vector2.One)
    {
        Position = startLocation;
    }

    public override void Update(GameTime gameTime)
    {
        hitEnemies.Clear();

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

        var bulletToDelete = false;

        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            if (Collision.IsLineInEntity(oldPosition, Position, enemy,
                out Vector2 entryPoint, out Vector2 exitPoint))
            {
                hitEnemies.Add(enemy);
            } else if (Collision.IsLineInEntity(oldPosition+sideOneOffset,
                Position + sideOneOffset, enemy, out entryPoint, out exitPoint))
            {
                hitEnemies.Add(enemy);
            } else if (Collision.IsLineInEntity(oldPosition+sideTwoOffset,
            Position + sideTwoOffset, enemy, out entryPoint, out exitPoint))
            {
                hitEnemies.Add(enemy);
            }
        }

        Lifetime -= deltaTime;

        for (int i = 0; i < hitEnemies.Count; i++)
        {
            // todo: make it so a projectile with pierce doesn't hit the same enemy multiple time
            if (Pierce > 0)
            {
                var enemy = hitEnemies[i];
                enemy.HealthSystem.TakeDamage(Damage);
                Pierce -= 1;
            }
            else
            {
                bulletToDelete = true;
                break;
            }
        }

        if (bulletToDelete || Lifetime <= 0f)
        {
            Destroy();
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        RotationRadians = (float)Math.Atan2(Direction.Y, Direction.X);
        DrawOrigin = new Vector2(BulletLength/2, BulletWidth/2);

        var bulletStart = Position - Direction * BulletLength / 2f;

        if (Sprite == null)
        {
            LineUtility.DrawLine(Game.SpriteBatch, bulletStart, Position, Color.Red, thickness: BulletWidth);
        }

        base.Draw(gameTime);
    }
}
