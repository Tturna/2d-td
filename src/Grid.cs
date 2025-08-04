using System;
using Microsoft.Xna.Framework;

public static class Grid
{
    public static Vector2 TileToWorldPosition(int x, int y)
    {
        float xPos = x * 16f;
        float yPos = y * 16f;
        return new Vector2(xPos, yPos);
    }

    public static Vector2 SnapPositionToGrid(Vector2 position)
    {
        int tilePosX = (int)Math.Floor(position.X / 16);
        int tilePosY = (int)Math.Floor(position.Y / 16);
        return new Vector2(tilePosX * 16f, tilePosY * 16f);
    }
}
