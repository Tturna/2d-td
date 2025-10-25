using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
class RocketGlove : Entity
{
    private HashSet<Enemy> hitEnemies = new();
    private int maxHitEnemies;
    public float PixelsPerSecond;
    public int Damage;
    public float ExplosionTileRadius;
    public Vector2 Direction;
    public float Lifetime;
    
    private static Texture2D GetShellTexture(SpriteBatch spriteBatch)
    {
        var tex = new Texture2D(spriteBatch.GraphicsDevice, width: 8, height: 8,
                mipmap: false, SurfaceFormat.Color);

        var colorData = new Color[64];

        for (int i = 0; i < 64; i++)
        {
            colorData[i] = Color.White;
        }

        tex.SetData(colorData);

        return tex;
    }

    public RocketGlove(Game game, Vector2 startLocation) : base(game, startLocation,GetShellTexture(((Game1)game).SpriteBatch))
    {
        // Rocket specific defaults
        PixelsPerSecond = 100f;
        Damage = 5000;
        ExplosionTileRadius = 2;
        maxHitEnemies = 5;
        Lifetime = 1f;
        Console.WriteLine("Spawned rocket");
    }

    public override void Update(GameTime gameTime)
    {
        if(Lifetime <= 0f)
        {
            HandleExplosion();
            Destroy();
            return;
        }
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var oldPosition = Position;
        Position += Direction * (PixelsPerSecond * deltaTime);

        if (hitEnemies.Count >= maxHitEnemies)
        {
            HandleExplosion();
            Destroy();
        }

        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            if(Collision.AreEntitiesColliding(this,enemy))
            {
                hitEnemies.Add(enemy);
                enemy.Knockback(Direction,4f);
                Console.WriteLine("Rocket hit enemy at position " + enemy.Position);
            }
        }

        Lifetime -= deltaTime;
    }
    private void HandleExplosion()
    {
        var explosionCenter = Position + Direction;

        var Enemies = EnemySystem.Enemies.ToArray();

        foreach (Enemy enemy in Enemies)
        {
            var distance = Vector2.Distance(explosionCenter, enemy.Position + enemy.Size / 2);
            var tileDistance = distance / Grid.TileLength;

            if (tileDistance <= ExplosionTileRadius)
            {
                enemy.HealthSystem.TakeDamage(Damage);
            }
        }

        Console.WriteLine("Rocket exploded at " + explosionCenter);
    }
}