using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class ScrapSystem
{
    private static Dictionary<Vector2, ScrapTile> scrapTileMap = new();

    public static void AddScrap(Game1 game, Vector2 worldPosition)
    {
        var gridPosition = Grid.SnapPositionToGrid(worldPosition);
        ScrapTile tile = null;

        if (!scrapTileMap.TryGetValue(gridPosition, out tile))
        {
            tile = new ScrapTile(game, gridPosition);
            scrapTileMap.Add(gridPosition, tile);
            return;
        }

        var shouldOverflow = tile.AddToPile();
    }
}
