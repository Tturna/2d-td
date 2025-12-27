using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class Collision
{
    public static bool AABB(float xPos1, float yPos1, float xSize1, float ySize1,
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

    public static bool AreRectAndCircleColliding(Vector2 rectPos, Vector2 rectSize,
        Vector2 circlePos, float circleRadius)
    {
        var closestX = MathHelper.Clamp(circlePos.X, rectPos.X, rectPos.X + rectSize.X);
        var closestY = MathHelper.Clamp(circlePos.Y, rectPos.Y, rectPos.Y + rectSize.Y);

        var dx = circlePos.X - closestX;
        var dy = circlePos.Y - closestY;

        return dx * dx + dy * dy <= circleRadius * circleRadius;
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

    public static bool IsEntityInTerrain(Entity ent, Terrain terrain, out Vector2[] collidedTilePositions)
    {
        var entityTilePosition = Grid.WorldToTilePosition(ent.Position);
        var entityTileSize = Vector2.Floor(ent.Size / Grid.TileLength) + Vector2.One;
        HashSet<Vector2> collided = new();

        for (int y = 0; y < entityTileSize.Y; y++)
        {
            for (int x = 0; x < entityTileSize.X; x++)
            {
                var comparedTilePosition = entityTilePosition + new Vector2(x, y);

                // Check collision by checking if a tile exists within the grid space
                // taken by the entity.
                if (terrain.CollisionTileIsActiveAtPosition(comparedTilePosition))
                {
                    collided.Add(comparedTilePosition);
                }
            }

            var farEnd = ent.Position + Vector2.UnitX * ent.Size.X;
            var feGridPos = Grid.SnapPositionToGrid(farEnd);
            var feTilePos = Grid.WorldToTilePosition(feGridPos);

            if (terrain.CollisionTileIsActiveAtPosition(feTilePos))
            {
                collided.Add(feTilePos);
            }
        }

        var bottomEnd = ent.Position + Vector2.UnitY * ent.Size.Y;
        var beGridPos = Grid.SnapPositionToGrid(bottomEnd);
        var beTilePos = Grid.WorldToTilePosition(beGridPos);

        for (int x = 0; x < entityTileSize.X; x++)
        {
            var comparedTilePosition = beTilePos + new Vector2(x, 0f);

            // Check collision by checking if a tile exists within the grid space
            // taken by the entity.
            if (terrain.CollisionTileIsActiveAtPosition(comparedTilePosition))
            {
                collided.Add(comparedTilePosition);
            }
        }

        var bottomRightCorner = ent.Position + ent.Size;
        var brcGridPos = Grid.SnapPositionToGrid(bottomRightCorner);
        var brcTilePos = Grid.WorldToTilePosition(brcGridPos);

        if (terrain.CollisionTileIsActiveAtPosition(brcTilePos))
        {
            collided.Add(brcTilePos);
        }

        collidedTilePositions = new Vector2[collided.Count];
        collided.CopyTo(collidedTilePositions);

        return collidedTilePositions.Length > 0;
    }

    public static bool IsPointInTerrain(Vector2 point, Terrain terrain)
    {
        var pointGridPosition = Grid.SnapPositionToGrid(point);
        var pointTilePosition = Grid.WorldToTilePosition(pointGridPosition);

        return terrain.CollisionTileIsActiveAtPosition(pointTilePosition);
    }

    public static bool IsLineInRectangle(Vector2 linePointA, Vector2 linePointB,
        float x, float y, float w, float h, out Vector2 entryPoint, out Vector2 exitPoint)
    {
        entryPoint = Vector2.Zero;
        exitPoint = Vector2.Zero;

        // Find line intersection in an AABB using the slab method.

        // Use a parametric line, meaning a representation of the line that can be used
        // to get any point along the line.
        // Line: P1 + t(P2 - P1), where t is within [0 - 1]. Effectively it's lerp.
        // t is the factor that can be used to get any point along the line.

        // Split line into X and Y and check against AABB.
        // When box.minX = P1.X + t(P2.X - P1.X), the line intersects the left vertical
        // edge of the AABB. Solving t gets the intersection point.
        // t = (box.minX - P1.X) / (P2.X - P1.X)

        var lineDirection = linePointB - linePointA;
        float horizontalEnterFactor, horizontalExitFactor, verticalEnterFactor, verticalExitFactor;

        if (Math.Abs(lineDirection.X) < float.Epsilon)
        {
            // Line is vertical
            if (linePointA.X < x || linePointA.X > x + w)
            {
                return false;
            }

            horizontalEnterFactor = float.NegativeInfinity;
            horizontalExitFactor = float.PositiveInfinity;
        }
        else
        {
            // Get enter and exit factors regardless of line direction
            var leftIntersectFactor = (x - linePointA.X) / lineDirection.X;
            var rightIntersectFactor = (x + w - linePointA.X) / lineDirection.X;
            horizontalEnterFactor = MathHelper.Min(leftIntersectFactor, rightIntersectFactor);
            horizontalExitFactor = MathHelper.Max(leftIntersectFactor, rightIntersectFactor);
        }

        if (Math.Abs(lineDirection.Y) < float.Epsilon)
        {
            // Line is horizontal
            if (linePointA.Y < y || linePointA.Y > y + h)
            {
                return false;
            }

            verticalEnterFactor = float.NegativeInfinity;
            verticalExitFactor = float.PositiveInfinity;
        }
        else
        {
            var topIntersectFactor = (y - linePointA.Y) / lineDirection.Y;
            var bottomIntersectFactor = (y + h - linePointA.Y) / lineDirection.Y;
            verticalEnterFactor = MathHelper.Min(topIntersectFactor, bottomIntersectFactor);
            verticalExitFactor = MathHelper.Max(topIntersectFactor, bottomIntersectFactor);
        }

        var furthestEnterFactor = MathHelper.Max(horizontalEnterFactor, verticalEnterFactor);
        var nearestExitFactor = MathHelper.Min(horizontalExitFactor, verticalExitFactor);

        // If the line exits an axis before entering the other, it missed the AABB.
        // In other words, if it intersected both lines in an axis before intersecting one
        // on the other axis, it missed the AABB.
        // Or, the line exits one slab before entering the other.
        if (furthestEnterFactor > nearestExitFactor)
        {
            return false;
        }

        // Line intersects at some point

        var clampedEnterFactor = MathHelper.Max(0, furthestEnterFactor);
        var clampedExitFactor = MathHelper.Min(1, nearestExitFactor);

        if (clampedEnterFactor > clampedExitFactor)
        {
            // Intersection happens outside of given line segment
            return false;
        }

        entryPoint = linePointA + lineDirection * clampedEnterFactor;
        exitPoint = linePointA + lineDirection * clampedExitFactor;

        return true;

        /* Example illustration of a line that misses an AABB
         *
         *       |     |
         *       |     |         s
         *       |     |        /
         * --------------------t---
         *       |xxxxx|      /
         *       |xxxxx|     /
         *       |xxxxx|    /
         * ----------------b---
         *       |     |  /
         *       |     | /
         *       |     |/
         *       |     r
         *       |    /|
         *       |   / |
         *       |  /  |
         *       | /   |
         *       |/    |
         *       l     |
         *      /
         * x = AABB
         * s = line start point
         * t = top intersect factor
         * b = bottom intersect factor
         * r = right intersect factor
         * l = left intersect factor
         *
         * To get any of these as a point on the line, you do:
         * A + T(B - A), where A is the line start point, B is the line end point, and T
         * is any of these factors.
         *
         * There are two parallel lines on each axis that the line ccan intersect. Each axis
         * has an enter and exit line. If the line intersects both enter and exit lines of an axis,
         * it has to miss the AABB. If the line enters both enter lines before exiting one,
         * it has to hit the AABB.
         *
         * This can be checked by checking if the furtherst enter factor is greater than the
         * nearest exit factor. Effectively, if the second exit point is closer than the first
         * enter point, both lines of an axis have been crossed and the AABB is has been missed.
        */
    }

    public static bool IsLineInEntity(Vector2 linePointA, Vector2 linePointB, Entity entity, out Vector2 entryPoint, out Vector2 exitPoint)
    {
        return IsLineInRectangle(linePointA, linePointB, entity.Position.X, entity.Position.Y,
            entity.Size.X, entity.Size.Y, out entryPoint, out exitPoint);
    }

    public static bool IsLineInTerrain(Vector2 linePointA, Vector2 linePointB, out Vector2 entryPoint, out Vector2 exitPoint)
    {
        entryPoint = Vector2.Zero;
        exitPoint = Vector2.Zero;

        // Create an axis-aligned rectangle where the line points are opposite corners.
        // Check all tiles in the rectangle for whether they're in terrain.
        // Return true if the given line passes through any terrain tile.
        var xGap = Math.Abs(linePointA.X - linePointB.X);
        var yGap = Math.Abs(linePointA.Y - linePointB.Y);
        var xTiles = (int)Math.Floor(xGap / Grid.TileLength) + 2;
        var yTiles = (int)Math.Floor(yGap / Grid.TileLength) + 2;
        var minX = Math.Min(linePointA.X, linePointB.X);
        var minY = Math.Min(linePointA.Y, linePointB.Y);

        for (int y = 0; y < yTiles; y++)
        {
            for (int x = 0; x < xTiles; x++)
            {
                var testPointX = minX + x * Grid.TileLength;
                var testPointY = minY + y * Grid.TileLength;
                var testTilePosition = Grid.SnapPositionToGrid(new Vector2(testPointX, testPointY));

                if (IsPointInTerrain(testTilePosition, Game1.Instance.Terrain))
                {
                    if (IsLineInRectangle(linePointA, linePointB, testTilePosition.X,
                        testTilePosition.Y, Grid.TileLength, Grid.TileLength, out entryPoint, out exitPoint))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
