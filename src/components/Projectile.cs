using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class Projectile : Entity
{
    public Vector2 Direction;
    public float Lifetime = 1f;
    public float BulletPixelsPerSecond = 0f;
    public float BulletLength = 16f;
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

        var bulletToDelete = false;

        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            if (Collision.IsLineInEntity(oldPosition, Position, enemy,
                out Vector2 entryPoint, out Vector2 exitPoint))
            {
                hitEnemies.Add(enemy);
            }
        }

        Lifetime -= deltaTime;

        for (int i = 0; i < hitEnemies.Count; i++)
        {
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
        var bulletStart = Position - Direction * BulletLength / 2f;

        LineUtility.DrawLine(Game.SpriteBatch, bulletStart, Position, Color.Red, thickness: 2f);

        base.Draw(gameTime);
    }
}
