using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public class RocketGlove : Entity
{
    private HashSet<Enemy> hitEnemies = new();
    private int maxHitEnemies;
    private float knockback;

    public float PixelsPerSecond;
    public int Damage;
    public float ExplosionTileRadius;
    public Vector2 Direction;
    public float Lifetime;
    
    public RocketGlove(Game game, Vector2 startLocation, float knockback) : base(game, startLocation,
        TextureUtility.GetBlankTexture(((Game1)game).SpriteBatch, 7, 7, Color.White))
    {
        // Rocket specific defaults
        PixelsPerSecond = 100f;
        Damage = 5000;
        ExplosionTileRadius = 2;
        maxHitEnemies = 5;
        Lifetime = 1f;
        this.knockback = knockback;
        Console.WriteLine("Spawned rocket");
    }

    public override void Update(GameTime gameTime)
    {
        if (Lifetime <= 0f)
        {
            EffectUtility.Explode(Position + Direction, ExplosionTileRadius * Grid.TileLength,
                magnitude: knockback, Damage);
            Destroy();
            return;
        }

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var oldPosition = Position;
        UpdatePosition(Direction * (PixelsPerSecond * deltaTime));

        if (hitEnemies.Count >= maxHitEnemies)
        {
            EffectUtility.Explode(Position + Direction, ExplosionTileRadius * Grid.TileLength,
                magnitude: knockback, Damage);
            Destroy();
        }

        var enemyCandidates = EnemySystem.EnemyBins.GetBinAndNeighborValues(Position);

        foreach (Enemy enemy in enemyCandidates)
        {
            if (Collision.AreEntitiesColliding(this, enemy))
            {
                hitEnemies.Add(enemy);
                enemy.ApplyKnockback(Direction * 4f);
                Console.WriteLine("Rocket hit enemy at position " + enemy.Position);
            }
        }

        Lifetime -= deltaTime;

        if (Collision.IsEntityInTerrain(this, Game.Terrain, out var _))
        {
            EffectUtility.Explode(Position + Direction, ExplosionTileRadius * Grid.TileLength,
                magnitude: knockback, Damage);
            Destroy();
        }
    }
}
