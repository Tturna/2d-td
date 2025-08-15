using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class PhysicsSystem
{
    private Game1 game;
    public float LocalGravity { get; private set; } = 30f;
    public Vector2 Velocity { get; private set; }
    public float DragFactor { get; set; } = 0.05f;

    public PhysicsSystem(Game1 game)
    {
        this.game = game;
    }

    public void UpdatePhysics(Entity entity, GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Velocity += Vector2.UnitY * LocalGravity * deltaTime;
        Velocity = Vector2.Lerp(Velocity, Vector2.Zero, DragFactor);

        entity.Position += Velocity;

        if (Collision.IsEntityInTerrain(entity, game.Terrain, out Vector2[] collidedTilePositions))
        {
            foreach (var collidedTilePosition in collidedTilePositions)
            {
                var tileWorldPosition = Grid.TileToWorldPosition(collidedTilePosition);

                var x1 = entity.Position.X;
                var x2 = tileWorldPosition.X;
                var y1 = entity.Position.Y;
                var y2 = tileWorldPosition.Y;
                var w1 = x1 + entity.Size.X;
                var w2 = x2 + Grid.TileLength;
                var h1 = y1 + entity.Size.Y;
                var h2 = y2 + Grid.TileLength;

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

                entity.Position += correction;
            }
        }
    }

    public void AddForce(Vector2 force)
    {
        Velocity += force;
    }
}
