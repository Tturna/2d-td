using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public static class ScrapSystem
{
    private static Dictionary<Vector2, ScrapTile>? scrapTileMap;

    public static void Initialize()
    {
        if (scrapTileMap is not null)
        {
            foreach (var item in scrapTileMap)
            {
                item.Value.Destroy();
            }
        }

        scrapTileMap = new();
    }

    public static void AddScrap(Game1 game, Vector2 worldPosition)
    {
        var gridPosition = Grid.SnapPositionToGrid(worldPosition);
        ScrapTile? tile = null;

        var targetPosition = gridPosition;

        while (true)
        {
            var belowTile = ScrapSystem.GetScrapFromPosition(targetPosition);

            if (belowTile is not null && belowTile.ScrapLevel > 0)
            {
                if (belowTile.ScrapLevel < ScrapTile.MaxScrapLevel)
                {
                    belowTile.AddToPile();
                    return;
                }

                break;
            }

            if (Collision.IsPointInTerrain(targetPosition, game.Terrain))
            {
                break;
            }

            targetPosition += Vector2.UnitY * Grid.TileLength;
        }

        targetPosition -= Vector2.UnitY * Grid.TileLength;

        // Assume Initialize has been called. Throw if not.
        if (!scrapTileMap!.TryGetValue(targetPosition, out tile))
        {
            tile = new ScrapTile(game, targetPosition);
            scrapTileMap.Add(targetPosition, tile);
            return;
        }

        tile.AddToPile();
    }

    public static ScrapTile? GetScrapFromPosition(Vector2 worldPosition)
    {
        var gridPosition = Grid.SnapPositionToGrid(worldPosition);

        if (scrapTileMap!.TryGetValue(gridPosition, out var tile))
        {
            return tile;
        }

        return null;
    }
}
