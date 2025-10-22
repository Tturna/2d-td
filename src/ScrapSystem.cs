using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public static class ScrapSystem
{
    private static Dictionary<Vector2, ScrapTile>? scrapTileMap;
    private static Stack<Vector2> scrapInsertionOrder = new();
    private static bool clearingScrap;
    private static readonly float clearStepInterval = 0.1f;
    private static float clearStepTimer;

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

        WaveSystem.WaveEnded += ClearScrap;
    }

    public static void Update(GameTime gameTime)
    {
        if (!clearingScrap) return;

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        clearStepTimer -= deltaTime;

        if (clearStepTimer <= 0)
        {
            clearStepTimer = clearStepInterval;

            if (scrapTileMap is null)
            {
                clearingScrap = false;
                return;
            }

            if (scrapTileMap.Count > 0)
            {
                var key = scrapInsertionOrder.Pop();
                scrapTileMap[key].Destroy();
                scrapTileMap.Remove(key);

                if (scrapTileMap.Count == 0)
                {
                    clearingScrap = false;
                }
            }
            else
            {
                clearingScrap = false;
            }
        }
    }

    public static void AddScrap(Game1 game, Vector2 worldPosition)
    {
        var gridPosition = Grid.SnapPositionToGrid(worldPosition);
        ScrapTile? tile = null;

        var targetPosition = gridPosition;
        var failSafeCounter = 100;

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

            failSafeCounter--;

            // No available space found from under the given position
            if (failSafeCounter <= 0) return;
        }

        targetPosition -= Vector2.UnitY * Grid.TileLength;

        // Assume Initialize has been called. Throw if not.
        if (!scrapTileMap!.TryGetValue(targetPosition, out tile))
        {
            tile = new ScrapTile(game, targetPosition);
            scrapTileMap.Add(targetPosition, tile);
            scrapInsertionOrder.Push(targetPosition);
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

    private static void ClearScrap()
    {
        if (clearingScrap) return;

        clearingScrap = true;
        clearStepTimer = clearStepInterval;
    }
}
