using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class Projectile : Entity
{
    public Vector2 Direction;
    public float InitialLifetime = 1f;
    public float Lifetime;
    public float BulletPixelsPerSecond = 0f;
    public float bulletLength = 16f;
    private List<Enemy> hitEnemies = new();
    private int damage = 0;

    public Projectile(Game game, Vector2 startLocation, Vector2 _direction, int _damage, float speedInPixel, float _lifetime) : base(game, null, Vector2.One)
    {
        Position = startLocation;
        Direction = _direction;
        damage = _damage;
        Lifetime = _lifetime;
        BulletPixelsPerSecond = speedInPixel;
    }

    public override void Update(GameTime gameTime)
    {
        hitEnemies.Clear();

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var oldPosition = Position;
        Position += Direction * (BulletPixelsPerSecond * deltaTime);

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
        var bulletStart = Position - Direction * bulletLength / 2f;

        LineUtility.DrawLine(Game.SpriteBatch, bulletStart, Position, Color.Red, thickness: 2f);

        base.Draw(gameTime);
    }
}
