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
    public int Pierce = 0;
    public int ExplosionTileRadius = 0;
    public Color TrailColor = Color.White;
    public float RotationOffset;
    public float TrailParticleInterval = 0.01f;

    private HashSet<Enemy> hitEnemies = new();
    private HashSet<Enemy> damagedEnemies = new();
    private float trailParticleTimer;
    private Entity ownerEntity; // what spawned/owns this projectile

    // this constructor is simple so that the turrets can edit the property
    // of bullet themself
    public Projectile(Game game, Entity source, Vector2 startLocation) :
        base(game, startLocation, null, Vector2.One)
    {
        ownerEntity = source;
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var oldPosition = Position;
        UpdatePosition(Direction * (BulletPixelsPerSecond * deltaTime));

        RotationRadians = (float)Math.Atan2(Direction.Y, Direction.X);

        // checks 3 times, once on the center of the bullet
        // and on the sides of the bullet 
        var rad = (float)Math.PI / 2;
        var perpendicularDirection = Direction;
        perpendicularDirection.Rotate(rad);

        var sideOneOffset = perpendicularDirection * BulletWidth;
        var sideTwoOffset = -perpendicularDirection * BulletWidth;

        var bulletToDelete = false;
        var shouldExplode = false;

        var startPointCandidates = EnemySystem.EnemyBins.GetBinAndNeighborValues(oldPosition);
        var endPointCandidates = EnemySystem.EnemyBins.GetBinAndNeighborValues(Position);

        HashSet<Enemy> enemyCandidates = new (startPointCandidates);
        enemyCandidates.UnionWith(endPointCandidates);

        foreach (Enemy enemy in enemyCandidates)
        {
            if (ExplosionTileRadius == 0)
            {
                if (Collision.IsLineInEntity(oldPosition, Position, enemy,
                    out Vector2 entryPoint, out Vector2 exitPoint))
                {
                    if (hitEnemies.Add(enemy))
                    {
                        ParticleSystem.PlaySparkEffect(entryPoint, -Direction);
                    }
                }
                else if (Collision.IsLineInEntity(oldPosition + sideOneOffset,
                    Position + sideOneOffset, enemy, out entryPoint, out exitPoint))
                {
                    if (hitEnemies.Add(enemy))
                    {
                        ParticleSystem.PlaySparkEffect(entryPoint, -Direction);
                    }
                }
                else if (Collision.IsLineInEntity(oldPosition + sideTwoOffset,
                Position + sideTwoOffset, enemy, out entryPoint, out exitPoint))
                {
                    if (hitEnemies.Add(enemy))
                    {
                        ParticleSystem.PlaySparkEffect(entryPoint, -Direction);
                    }
                }
            }
            else
            {
                if (Collision.IsLineInEntity(oldPosition, Position, enemy,
                    out Vector2 entryPoint, out Vector2 exitPoint))
                {
                    shouldExplode = true;
                }
                else if (Collision.IsLineInEntity(oldPosition + sideOneOffset,
                    Position + sideOneOffset, enemy, out entryPoint, out exitPoint))
                {
                    shouldExplode = true;
                }
                else if (Collision.IsLineInEntity(oldPosition + sideTwoOffset,
                Position + sideTwoOffset, enemy, out entryPoint, out exitPoint))
                {
                    shouldExplode = true;
                }
            }
        }

        Lifetime -= deltaTime;

        if (ExplosionTileRadius == 0)
        {
            foreach (var enemy in hitEnemies)
            {
                if (damagedEnemies.Contains(enemy)) continue;

                var diff = enemy.Position - oldPosition;
                var knockbackDirection = diff;
                knockbackDirection.Normalize();
                var knockback = knockbackDirection * (Damage / 15);

                enemy.HealthSystem.TakeDamage(ownerEntity, Damage);
                enemy.ApplyKnockback(knockback);
                damagedEnemies.Add(enemy);

                if (Pierce > 0)
                {
                    Pierce -= 1;
                }
                else
                {
                    bulletToDelete = true;
                    break;
                }
            }
        }
        else if (shouldExplode == true)
        {
            EffectUtility.Explode(this, Position, ExplosionTileRadius * Grid.TileLength,
                magnitude: 5f, Damage);

            if (Pierce > 0)
            {
                Pierce -= 1;
            }
            else
            {
                bulletToDelete = true;
            }
        }

        if (bulletToDelete || Lifetime <= 0f)
        {
            Destroy();
        }

        if (trailParticleTimer < TrailParticleInterval)
        {
            trailParticleTimer += deltaTime;
        }

        while (trailParticleTimer >= TrailParticleInterval)
        {
            trailParticleTimer -= TrailParticleInterval;

            if (trailParticleTimer < 0) trailParticleTimer = 0;

            var pos = Position - Direction * trailParticleTimer * BulletPixelsPerSecond;
            ParticleSystem.PlayFloater(pos, TrailColor, Direction);
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        RotationRadians = (float)Math.Atan2(Direction.Y, Direction.X) + RotationOffset;
        DrawOrigin = new Vector2(BulletLength / 2, BulletWidth / 2);

        var bulletStart = Position - Direction * BulletLength / 2f;

        if (Sprite == null)
        {
            LineUtility.DrawLine(Game.SpriteBatch, bulletStart, Position, Color.Red, thickness: BulletWidth);
        }

        base.Draw(gameTime);
    }
}
