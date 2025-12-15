using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class MovementSystem
{
    public enum MovementPattern
    {
        Charge,
        BounceForward
    }

    public struct MovementData
    {
        public MovementPattern Pattern;
        public bool CanWalk;
        public bool CanFly;
        public float WalkSpeed;
        public float FlySpeed;
        public float JumpForce;
    }

    private Game1 game;
    private Vector2 defaultChargeDirection = Vector2.UnitX;
    private float climbCheckDistanceFactor = 0.15f;

    public MovementData CurrentData { get; private set; }

    public MovementSystem(Game1 game, MovementData data)
    {
        this.game = game;
        CurrentData = data;
    }

    public void UpdateMovement(Entity entity, float deltaTime)
    {
        switch (CurrentData.Pattern)
        {
            case MovementPattern.Charge:
                HandleCharge(entity, deltaTime);
                break;
            case MovementPattern.BounceForward:
                HandleBounceForward(entity, deltaTime);
                break;
        }
    }

    private (bool, bool) ShouldClimb(Entity entity)
    {
        // TODO: Consider using one vertical line check in front of the enemy instead of
        // multiple points.

        (float tileWidth, float remainderWidth) = int.DivRem((int)entity.Size.X, Grid.TileLength);
        var entityTileWidth = Math.Floor(tileWidth);

        var entityTileHeight = (int)Math.Floor(entity.Size.Y / Grid.TileLength);
        var remainderHeight = entity.Size.Y % Grid.TileLength;
        var halfEntityWidth = entity.Size.X / 2;
        var climbCheckDistance = halfEntityWidth + climbCheckDistanceFactor * Grid.TileLength;
        var shouldClimb = false;

        for (int i = 0; i <= entityTileHeight; i++)
        {
            var yOffset = Vector2.UnitY * (Grid.TileLength * i);

            if (i == entityTileHeight)
            {
                yOffset -= Vector2.UnitY * (Grid.TileLength / 2);
                yOffset += Vector2.UnitY * (remainderHeight / 2);
            }

            var startPos = entity.Position + yOffset;
            var horizontalEntityCenter = startPos + Vector2.UnitX * halfEntityWidth;
            var climbCheckPoint = horizontalEntityCenter + defaultChargeDirection * climbCheckDistance;
            
            if (Collision.IsPointInTerrain(climbCheckPoint, game.Terrain) ||
                ScrapSystem.IsPointInCorpse(climbCheckPoint))
            {
                shouldClimb = true;
                break;
            }

            // climb over towers
            foreach (var tower in BuildingSystem.Towers)
            {
                if (Collision.IsPointInEntity(climbCheckPoint, tower))
                {
                    shouldClimb = true;
                    break;
                }
            }

            if (shouldClimb) break;
        }

        // true if the entity has its side next to a wall. will be false if the entity
        // only has its bottom right most corner next to a wall (e.g. if they already climbed
        // most of the wall).
        var shouldClimbWall = shouldClimb;

        var bottomStartPos = entity.Position + Vector2.UnitY * (entity.Size.Y - 1);
        var centerStartPos = bottomStartPos + Vector2.UnitX * halfEntityWidth;
        var finalCheckPoint = centerStartPos + defaultChargeDirection * climbCheckDistance;
        var shouldClimbCorner = false;

        if (Collision.IsPointInTerrain(finalCheckPoint, game.Terrain) ||
            ScrapSystem.IsPointInCorpse(finalCheckPoint))
        {
            shouldClimbCorner = true;
        }

        foreach (var tower in BuildingSystem.Towers)
        {
            if (Collision.IsPointInEntity(finalCheckPoint, tower))
            {
                shouldClimbCorner = true;
                break;
            }
        }

        return (shouldClimbWall, shouldClimbCorner);
    }

    private void HandleCharge(Entity entity, float deltaTime)
    {
        if (CurrentData.CanWalk)
        {
            // if (CanAndShouldJump(entity))
            // {
            //     var enemy = (Enemy)entity;
            //     enemy.PhysicsSystem.AddForce(-Vector2.UnitY * CurrentData.JumpForce);
            //     enemy.PhysicsSystem.AddForce(defaultChargeDirection * CurrentData.WalkSpeed * deltaTime);
            //     jumpTimer = jumpInterval;
            // }

            var (shouldClimbWall, shouldClimbCorner) = ShouldClimb(entity);

            if (shouldClimbWall || shouldClimbCorner)
            {
                if (entity is Enemy)
                {
                    ((Enemy)entity).PhysicsSystem.StopMovement();
                }

                var power = 0.7f;
                if (shouldClimbCorner) power += 1f;

                var climbVelocity = -Vector2.UnitY * power;
                entity.UpdatePosition(climbVelocity);

                // if climbing into enemies or corpses, make them move
                var enemyCandidates = EnemySystem.EnemyBins.GetBinAndNeighborValues(entity.Position + entity.Size / 2);

                foreach (var enemy in enemyCandidates)
                {
                    if (entity == enemy) continue;
                    if (!Collision.AreEntitiesColliding(entity, enemy)) continue;

                    enemy.UpdatePosition(climbVelocity);
                }

                var corpseCandidates = ScrapSystem.Corpses.GetBinAndNeighborValues(entity.Position + entity.Size / 2);

                foreach (var corpse in corpseCandidates)
                {
                    if (entity == corpse) continue;
                    if (!Collision.AreEntitiesColliding(entity, corpse)) continue;

                    corpse.ClimbUp(climbVelocity);
                }
            }

            var leapMagnitude = 1f;

            if (shouldClimbCorner) leapMagnitude += 1f;

            entity.UpdatePosition(defaultChargeDirection * CurrentData.WalkSpeed * leapMagnitude);
            entity.Rotate(deltaTime * CurrentData.WalkSpeed * 10f);
        }
        // TODO: Implement flying enemy logic and shi
    }

    private void HandleBounceForward(Entity entity, float deltaTime)
    {
        if (Collision.IsEntityInTerrain(entity, game.Terrain, out var collidedTilePositions))
        {
            // This whole dot product checking prevents the bouncer from getting stuck on ceiling corners.
            var roughCollisionPoint = Grid.TileToWorldPosition(collidedTilePositions[0]);
            var diff = roughCollisionPoint - entity.Position + entity.Size / 2;
            var dir = diff;
            dir.Normalize();
            var dot = Vector2.Dot(dir, -Vector2.UnitY);

            if (MathF.Abs(dot) > 0.5f)
            {
                var bounceDir = diff.Y <= 0 ? 1 : -1;
                var enemy = (Enemy)entity;
                enemy.PhysicsSystem.AddForce(Vector2.UnitY * bounceDir * CurrentData.JumpForce);
                enemy.PhysicsSystem.AddForce(defaultChargeDirection * CurrentData.WalkSpeed);
                entity.StretchImpact(new Vector2(1.5f, 0.5f), 0.2f);
            }
        }

        entity.UpdatePosition(defaultChargeDirection * CurrentData.WalkSpeed);
        entity.Rotate(deltaTime * CurrentData.WalkSpeed * 10f);
    }
}
