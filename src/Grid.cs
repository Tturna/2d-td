using Microsoft.Xna.Framework;

namespace _2d_td;

public static class Grid
{
    public const int TileLength = 8;

    public static Vector2 TileToWorldPosition(Vector2 tilePosition)
    {
        float xPos = tilePosition.X * TileLength;
        float yPos = tilePosition.Y * TileLength;
        return new Vector2(xPos, yPos);
    }

    public static Vector2 WorldToTilePosition(Vector2 position)
    {
        return Vector2.Floor(position / TileLength);
    }

    public static Vector2 SnapPositionToGrid(Vector2 worldPosition)
    {
        return WorldToTilePosition(worldPosition) * TileLength;
    }
}
