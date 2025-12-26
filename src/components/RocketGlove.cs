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
    public int ExplosionTileRadius;
    public Vector2 Direction;
    public float Lifetime;
    
    public RocketGlove(Game game, Vector2 startLocation, float knockback) : base(game, startLocation,
        GetRocketGloveAnimation())
    {
        // Rocket specific defaults
        PixelsPerSecond = 100f;
        Damage = 5000;
        ExplosionTileRadius = 2;
        maxHitEnemies = 5;
        Lifetime = 1f;
        this.knockback = knockback;

        Size -= new Vector2(0, 4);
    }

    public override void Update(GameTime gameTime)
    {
        if (Lifetime <= 0f)
        {
            EffectUtility.Explode(this, Position + Direction, ExplosionTileRadius * Grid.TileLength,
                magnitude: knockback, Damage);
            Destroy();
            return;
        }

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var oldPosition = Position;
        UpdatePosition(Direction * (PixelsPerSecond * deltaTime));

        if (hitEnemies.Count >= maxHitEnemies)
        {
            EffectUtility.Explode(this, Position + Direction, ExplosionTileRadius * Grid.TileLength,
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
            }
        }

        Lifetime -= deltaTime;

        if (Collision.IsEntityInTerrain(this, Game.Terrain, out var _))
        {
            EffectUtility.Explode(this, Position + Direction, ExplosionTileRadius * Grid.TileLength,
                magnitude: knockback, Damage);
            Destroy();
        }
    }

    private static AnimationSystem.AnimationData GetRocketGloveAnimation()
    {
        var rocketGloveTexture = AssetManager.GetTexture("rocketglove");

        var animation = new AnimationSystem.AnimationData(
            texture: rocketGloveTexture,
            frameCount: 2,
            frameSize: new Vector2(rocketGloveTexture.Width / 2, rocketGloveTexture.Height),
            delaySeconds: 0.1f);

        return animation;
    }
}
