using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class Grid
{
    public const float TileLength = 16f;

    public static Vector2 TileToWorldPosition(int x, int y)
    {
        float xPos = x * TileLength;
        float yPos = y * TileLength;
        return new Vector2(xPos, yPos);
    }

    public static Vector2 SnapPositionToGrid(Vector2 position)
    {
        int tilePosX = (int)Math.Floor(position.X / TileLength);
        int tilePosY = (int)Math.Floor(position.Y / TileLength);
        return new Vector2(tilePosX * TileLength, tilePosY * TileLength);
    }
}
