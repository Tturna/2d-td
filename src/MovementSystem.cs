using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class MovementSystem
{
    public enum MovementPattern
    {
        Charge
        // Add more if an enemy should do something other than charge to the right side of
        // the screen
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
    private float jumpTimer;
    private float jumpInterval = 0.5f;
    private float jumpCheckDistanceFactor = 0.5f;
    private float climbCheckDistanceFactor = 0.15f;

    public MovementData CurrentData { get; private set; }

    public MovementSystem(Game1 game, MovementData data)
    {
        this.game = game;
        CurrentData = data;
    }

    public void UpdateMovement(Entity entity, GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (jumpTimer > 0f)
        {
            jumpTimer -= deltaTime;

            if (jumpTimer <= 0f)
            {
                jumpTimer = 0f;
            }
        }

        switch (CurrentData.Pattern)
        {
            case MovementPattern.Charge:
                HandleCharge(entity, deltaTime);
                break;
        }
    }

    private bool CanAndShouldJump(Entity entity)
    {
        if (jumpTimer > 0 || entity is not Enemy) return false;

        var entityBottom = entity.Position + Vector2.UnitY * entity.Size.Y;
        var groundCheckStartPoint = entityBottom + Vector2.UnitY * (Grid.TileLength / 3);

        (float tileWidth, float remainderWidth) = int.DivRem((int)entity.Size.X, Grid.TileLength);
        var entityTileWidth = Math.Floor(tileWidth);

        var entityTileHeight = (int)Math.Floor(entity.Size.Y / Grid.TileLength);
        var remainderHeight = entity.Size.Y % Grid.TileLength;
        var halfEntityWidth = entity.Size.X / 2;
        var jumpCheckDistance = halfEntityWidth + jumpCheckDistanceFactor * Grid.TileLength;
        var shouldJump = false;

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
            var jumpCheckPoint = horizontalEntityCenter + defaultChargeDirection * jumpCheckDistance;
            
            if (Collision.IsPointInTerrain(jumpCheckPoint, game.Terrain) ||
                ScrapSystem.GetScrapFromPosition(jumpCheckPoint) is not null)
            {
                shouldJump = true;
                break;
            }
        }

        return shouldJump;
    }

    private bool ShouldClimb(Entity entity)
    {
        var entityBottom = entity.Position + Vector2.UnitY * entity.Size.Y;

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
                ScrapSystem.GetScrapFromPosition(climbCheckPoint) is not null)
            {
                shouldClimb = true;
                break;
            }
        }

        var bottomStartPos = entity.Position + Vector2.UnitY * (entity.Size.Y - 1);
        var centerStartPos = bottomStartPos + Vector2.UnitX * halfEntityWidth;
        var finalCheckPoint = centerStartPos + defaultChargeDirection * climbCheckDistance;

        if (Collision.IsPointInTerrain(finalCheckPoint, game.Terrain) ||
            ScrapSystem.GetScrapFromPosition(finalCheckPoint) is not null)
        {
            shouldClimb = true;
        }

        return shouldClimb;
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

            if (ShouldClimb(entity))
            {
                if (entity is Enemy)
                {
                    ((Enemy)entity).PhysicsSystem.StopMovement();
                }

                entity.UpdatePosition(-Vector2.UnitY * deltaTime * 100);
            }

            entity.UpdatePosition(defaultChargeDirection * CurrentData.WalkSpeed * deltaTime);
        }
        // TODO: Implement flying enemy logic and shi
    }
}
