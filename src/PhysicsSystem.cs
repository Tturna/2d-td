using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class PhysicsSystem
{
    private Game1 game;
    public float LocalGravity { get; set; } = 30f;
    public Vector2 Velocity { get; private set; }
    public float DragFactor { get; set; } = 0.05f;

    public PhysicsSystem(Game1 game)
    {
        this.game = game;
    }

    /// <summary>
    /// Moves the given entity based on its velocity. Returns a boolean indicating whether
    /// the entity collided with terrain or scrap.
    /// </summary>
    public bool UpdatePhysics(Entity entity, float deltaTime)
    {
        Velocity += Vector2.UnitY * LocalGravity * deltaTime;
        Velocity = Vector2.Lerp(Velocity, Vector2.Zero, DragFactor);

        var oldPosition = entity.Position + entity.Size / 2;
        entity.UpdatePosition(Velocity);
        var newCenter = entity.Position + entity.Size / 2;

        if (Collision.IsLineInTerrain(oldPosition, newCenter, out var entryPoint, out var _))
        {
            // Set entity position to first collision point and then resolve.
            // This helps prevent fast moving objects moving far into terrain or scrap and
            // resolving incorrectly.
            entity.SetPosition(entryPoint - entity.Size / 2);
        }

        if (Collision.IsEntityInScrap(entity, out var collidedScraps))
        {
            ResolveEntityScrapCollision(entity, collidedScraps);
        }

        if (Collision.IsEntityInTerrain(entity, game.Terrain, out var collidedTilePositions))
        {
            ResolveEntityTerrainCollision(entity, collidedTilePositions);
        }

        // Collide with other enemies
        var maxSide = MathHelper.Max(entity.Size.X, entity.Size.Y);
        var enemyCandidates = EnemySystem.EnemyTree.GetValuesInOverlappingQuads(newCenter, (int)(maxSide));

        foreach (var enemy in enemyCandidates)
        {
            if (enemy == entity) continue;

            if (!Collision.AreEntitiesColliding(enemy, entity)) continue;

            ResolveEntitiesCollision(entity, enemy);
        }

        // Collide with corpses
        foreach (var corpse in ScrapSystem.Corpses)
        {
            if (!Collision.AreEntitiesColliding(entity, corpse)) continue;

            ResolveEntitiesCollision(entity, corpse);
        }

        return collidedScraps.Length + collidedTilePositions.Length > 0;
    }

    private void ResolveEntityCollision(Entity entity, float x1, float x2, float y1, float y2,
        float w1, float w2, float h1, float h2)
    {
            var rightOverlap = w1 - x2;
            var leftOverlap = w2 - x1;
            var bottomOverlap = h1 - y2;
            var topOverlap = h2 - y1;

            var correctionX = (rightOverlap < leftOverlap) ? -rightOverlap : leftOverlap;
            var correctionY = (bottomOverlap < topOverlap) ? -bottomOverlap : topOverlap;
            Vector2 correction = Vector2.Zero;

            if (Math.Abs(correctionX) < Math.Abs(correctionY))
            {
                correction = Vector2.UnitX * correctionX;
                Velocity = Vector2.UnitY * Velocity.Y;
            }
            else
            {
                correction = Vector2.UnitY * correctionY;
                Velocity = Vector2.UnitX * Velocity.X;
            }

            entity.UpdatePosition(correction);
    }

    private void ResolveEntitiesCollision(Entity resolvingEntity, Entity collidedEntity)
    {
        var x1 = resolvingEntity.Position.X;
        var x2 = collidedEntity.Position.X;
        var y1 = resolvingEntity.Position.Y;
        var y2 = collidedEntity.Position.Y;
        var w1 = x1 + resolvingEntity.Size.X;
        var w2 = x2 + collidedEntity.Size.X;
        var h1 = y1 + resolvingEntity.Size.Y;
        var h2 = y2 + collidedEntity.Size.Y;

        ResolveEntityCollision(resolvingEntity, x1, x2, y1, y2, w1, w2, h1, h2);
    }

    private void ResolveEntityTerrainCollision(Entity entity, Vector2[] collidedTilePositions)
    {
        foreach (var tilePosition in collidedTilePositions)
        {
            var tileWorldPosition = Grid.TileToWorldPosition(tilePosition);

            var x1 = entity.Position.X;
            var x2 = tileWorldPosition.X;
            var y1 = entity.Position.Y;
            var y2 = tileWorldPosition.Y;
            var w1 = x1 + entity.Size.X;
            var w2 = x2 + Grid.TileLength;
            var h1 = y1 + entity.Size.Y;
            var h2 = y2 + Grid.TileLength;

            ResolveEntityCollision(entity, x1, x2, y1, y2, w1, w2, h1, h2);
        }
    }

    private void ResolveEntityScrapCollision(Entity entity, ScrapTile[] collidedScraps)
    {
        foreach (var scrapTile in collidedScraps)
        {
            var x1 = entity.Position.X;
            var x2 = scrapTile.Position.X;
            var y1 = entity.Position.Y;
            var y2 = scrapTile.Position.Y;
            var w1 = x1 + entity.Size.X;
            var w2 = x2 + scrapTile.Size.X * scrapTile.Scale.X;
            var h1 = y1 + entity.Size.Y;
            var h2 = y2 + scrapTile.Size.Y * scrapTile.Scale.Y;

            ResolveEntityCollision(entity, x1, x2, y1, y2, w1, w2, h1, h2);
        }
    }

    public void AddForce(Vector2 force)
    {
        Velocity += force;
    }

    public void StopMovement()
    {
        Velocity = Vector2.Zero;
    }
}
