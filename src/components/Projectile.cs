using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class Projectile : Entity
{
    public Vector2 Target;
    public float InitialLifetime = 1f;
    public float Lifetime;
    public float BulletPixelsPerSecond = 0f;
    public float bulletLength = 16f;
    private List<Enemy> hitEnemies = new();
    private int damage = 0;
    // public Direction
    public Projectile(Game game, Vector2 startLocation, Vector2 _target, int _damage, float speedInPixel, float _lifetime) : base(game, null, Vector2.One)
    {
        Position = startLocation;
        Target = _target;
        damage = _damage;
        Lifetime = _lifetime;
        BulletPixelsPerSecond = speedInPixel;
    }

    public override void Update(GameTime gameTime)
    {
        hitEnemies.Clear();

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var diff = Target - Position;
        var direction = diff;
        direction.Normalize();

        var oldPosition = Position;
        Position += direction * (BulletPixelsPerSecond * deltaTime);

        var bulletHit = false;

        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            if (Collision.IsLineInEntity(oldPosition, Position, enemy,
                out Vector2 entryPoint, out Vector2 exitPoint))
            {
                hitEnemies.Add(enemy);
                bulletHit = true;
            }
        }

        Lifetime -= deltaTime;

        for (int i = 0; i < hitEnemies.Count; i++)
        {
            var enemy = hitEnemies[i];
            enemy.HealthSystem.TakeDamage(damage);
        }

        if (bulletHit || Lifetime <= 0f)
        {
            Destroy();
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var diff = Target - Position;
        var direction = diff;
        direction.Normalize();

        // var position = Position - direction * (BulletPixelsPerSecond * deltaTime);
        var bulletStart = Position - direction * bulletLength / 2f;
        var bulletEnd = Position + direction * bulletLength / 2f;

        LineUtility.DrawLine(Game.SpriteBatch, bulletStart, bulletEnd, Color.Red, thickness: 2f);

        base.Draw(gameTime);
    }
}