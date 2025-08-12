using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class Collision
{
    private static bool AABB(float xPos1, float yPos1, float xSize1, float ySize1,
                             float xPos2, float yPos2, float xSize2, float ySize2)
    {
        if (xPos1 < xPos2 + xSize2 &&
            xPos1 + xSize1 > xPos2 &&
            yPos1 < yPos2 + ySize2 &&
            yPos1 + ySize1 > yPos2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool AreEntitiesColliding(Entity ent1, Entity ent2)
    {
        var x1 = ent1.Position.X;
        var y1 = ent1.Position.Y;
        var w1 = ent1.Size.X;
        var h1 = ent1.Size.Y;
        var x2 = ent2.Position.X;
        var y2 = ent2.Position.Y;
        var w2 = ent2.Size.X;
        var h2 = ent2.Size.Y;

        return AABB(x1, y1, w1, h1, x2, y2, w2, h2);
    }

    public static bool IsPointInEntity(Vector2 point, Entity ent)
    {
        var x1 = ent.Position.X;
        var y1 = ent.Position.Y;
        var w1 = ent.Size.X;
        var h1 = ent.Size.Y;
        var x2 = point.X;
        var y2 = point.Y;

        return AABB(x1, y1, w1, h1, x2, y2, 0f, 0f);
    }

    public static bool IsPointInTile(Vector2 point, Vector2 tileWorldPosition)
    {
        var x1 = tileWorldPosition.X;
        var y1 = tileWorldPosition.Y;
        var w1 = Grid.TileLength;
        var h1 = Grid.TileLength;
        var x2 = point.X;
        var y2 = point.Y;

        return AABB(x1, y1, w1, h1, x2, y2, 0f, 0f);
    }

    public static bool IsEntityInTerrain(Entity ent, Terrain terrain, out Vector2 collidedTilePosition)
    {
        var entityTilePosition = Grid.WorldToTilePosition(ent.Position);
        var entityTileSize = Vector2.Ceiling(ent.Size / Grid.TileLength) + Vector2.One;

        for (int y = 0; y < entityTileSize.Y; y++)
        {
            for (int x = 0; x < entityTileSize.Y; x++)
            {
                var comparedTilePosition = entityTilePosition + new Vector2(x, y);

                // Check collision by checking if a tile exists within the grid space
                // taken by the entity.
                if (terrain.TileExistsAtPosition(comparedTilePosition))
                {
                    collidedTilePosition = comparedTilePosition;
                    return true;
                }
            }
        }

        collidedTilePosition = Vector2.Zero;
        return false;
    }
}
