using System;
using _2d_td.interfaces;
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
    private int extraClimbCheckDistance = 6;

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

    private bool ShouldClimb(Entity entity)
    {
        var entityTopRight = entity.Position + new Vector2(entity.Size.X, 0);
        var entityBotRight = entity.Position + entity.Size;
        var entityBotLeft = entity.Position + new Vector2(0, entity.Size.Y);

        var topRightCheckPoint = entityTopRight + new Vector2(climbCheckDistanceFactor, 0);
        var botRightCheckPoint = entityBotRight + new Vector2(climbCheckDistanceFactor, -1);
        // For checking whether entity is in air (like when climbing) to lengthen climb check line.
        // This way entity climbs over corners better.
        var botLeftCheckPoint = entityBotLeft + new Vector2(0, climbCheckDistanceFactor);
        var botRightLowerPoint = entityBotRight + new Vector2(0, climbCheckDistanceFactor);
        var entityCenter = entity.Position + entity.Size / 2;
        var elongated = false;

        if (Collision.IsLineInTerrain(botLeftCheckPoint, botRightLowerPoint, out var _, out var _))
        {
            elongated = true;
            botRightCheckPoint += Vector2.UnitY * extraClimbCheckDistance;
        }

        if (!elongated)
        {
            var elonCorpseCandidates = ScrapSystem.Corpses.GetBinAndNeighborValues(entityCenter);

            foreach (var corpse in elonCorpseCandidates)
            {
                if (Collision.IsLineInEntity(botLeftCheckPoint, botRightLowerPoint, corpse, out var _, out var _))
                {
                    elongated = true;
                    botRightCheckPoint += Vector2.UnitY * extraClimbCheckDistance;
                    break;
                }
            }
        }

        if (!elongated)
        {
            foreach (var tower in BuildingSystem.Towers)
            {
                var core = ((ITower)tower).GetTowerCore();

                if (core.Health.CurrentHealth <= 0) continue;

                if (Collision.IsLineInEntity(botLeftCheckPoint, botRightLowerPoint, tower, out var _, out var _))
                {
                    elongated = true;
                    botRightCheckPoint += Vector2.UnitY * extraClimbCheckDistance;
                    break;
                }
            }
        }

        if (Collision.IsLineInTerrain(topRightCheckPoint, botRightCheckPoint, out var _, out var _))
        {
            return true;
        }

        var corpseCandidates = ScrapSystem.Corpses.GetBinAndNeighborValues(entityCenter);

        foreach (var corpse in corpseCandidates)
        {
            if (Collision.IsLineInEntity(topRightCheckPoint, botRightCheckPoint, corpse, out var _, out var _))
            {
                return true;
            }
        }

        foreach (var tower in BuildingSystem.Towers)
        {
            var core = ((ITower)tower).GetTowerCore();

            if (core.Health.CurrentHealth <= 0) continue;

            if (Collision.IsLineInEntity(topRightCheckPoint, botRightCheckPoint, tower, out var _, out var _))
            {
                return true;
            }
        }

        return false;
    }

    private void HandleCharge(Entity entity, float deltaTime)
    {
        if (CurrentData.CanWalk)
        {
            if (ShouldClimb(entity))
            {
                if (entity is Enemy)
                {
                    ((Enemy)entity).PhysicsSystem.StopMovement();
                }

                var climbVelocity = -Vector2.UnitY;
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

            entity.UpdatePosition(defaultChargeDirection * CurrentData.WalkSpeed);
            entity.Rotate(deltaTime * CurrentData.WalkSpeed * 10f);
        }
    }

    private void HandleBounceForward(Entity entity, float deltaTime)
    {
        var collided = false;
        var roughCollisionPoint = Vector2.Zero;

        // hacky way to make entity collide with its surroundings. It doesn't normally because
        // physics is resolved instantly.
        entity.UpdatePosition(Vector2.UnitY);

        if (Collision.IsEntityInTerrain(entity, game.Terrain, out var collidedTilePositions))
        {
            collided = true;
            roughCollisionPoint = Grid.TileToWorldPosition(collidedTilePositions[0]);
        }

        if (!collided)
        {
            foreach (var tower in BuildingSystem.Towers)
            {
                if (Collision.AreEntitiesColliding(entity, tower))
                {
                    collided = true;
                    roughCollisionPoint = (entity.Position + entity.Size / 2 + tower.Position + tower.Size / 2) / 2;
                    break;
                }
            }
        }

        if (!collided)
        {
            var corpseCandidates = ScrapSystem.Corpses.GetBinAndNeighborValues(entity.Position + entity.Size / 2);

            foreach (var corpse in corpseCandidates)
            {
                if (Collision.AreEntitiesColliding(entity, corpse))
                {
                    collided = true;
                    roughCollisionPoint = (entity.Position + entity.Size / 2 + corpse.Position + corpse.Size / 2) / 2;
                    break;
                }
            }
        }

        if (!collided)
        {
            var enemyCandidates = EnemySystem.EnemyBins.GetBinAndNeighborValues(entity.Position + entity.Size / 2);

            foreach (var enemy in enemyCandidates)
            {
                if (enemy == entity) continue;

                if (Collision.AreEntitiesColliding(entity, enemy))
                {
                    collided = true;
                    roughCollisionPoint = (entity.Position + entity.Size / 2 + enemy.Position + enemy.Size / 2) / 2;
                    break;
                }
            }
        }

        if (collided)
        {
            // This whole dot product checking prevents the bouncer from getting stuck on ceiling corners.
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
