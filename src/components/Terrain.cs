using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class Terrain : DrawableGameComponent
{
    private Game1 game;
    private readonly Tileset Tileset;
    private readonly Tileset PlayerLightTileset;
    private readonly Tileset PlayerHeavyTileset;
    private readonly string LevelPath;

    private Dictionary<Vector2, int> tiles = new();
    private Dictionary<Vector2, int> lightTiles = new();
    private Dictionary<Vector2, int> heavyTiles = new();
    private Vector2 levelOffset = new Vector2(0, 64 * Grid.TileLength);

    private Vector2 topRightMostTile;

    public Terrain(Game game, int zone, int level, string tilesetName = "groundtiles_zone1",
        int tilesetWidth = 12, int tilesetHeight = 4) : base(game)
    {
        this.game = (Game1)game;

        if (zone == 0)
        {
            throw new ArgumentException("Zone can't be less than 1", nameof(zone));
        }
        else if (level == 0)
        {
            throw new ArgumentException("Level can't be less than 1", nameof(level));
        }

        var levelName = $"zone{zone}level{level}";
        LevelPath = Path.Combine(AppContext.BaseDirectory, game.Content.RootDirectory,
            "data", "levels", levelName, $"{levelName}.csv");

        Tileset = new Tileset(AssetManager.GetTexture(tilesetName), tilesetWidth, tilesetHeight);
        PlayerHeavyTileset = new Tileset(AssetManager.GetTexture("heavytiles"), 4, 4);
        PlayerLightTileset = new Tileset(AssetManager.GetTexture("lighttiles"), 4, 4);
    }

    public override void Initialize()
    {
        // "using" makes it so Dispose() is automatically called on StreamReader when
        // it's not needed anymore.
        using (var levelReader = new StreamReader(LevelPath))
        {
            string line = levelReader.ReadLine();
            var row = 0;

            while (line is not null)
            {
                String[] ids = line.Split(",");

                for (int col = 0; col < ids.Length; col++)
                {
                    if (!int.TryParse(ids[col], out int tileId))
                    {
                        throw new InvalidDataException(
                            $"Invalid value '{ids[col]}' at row {row}, column {col + 1} in {LevelPath}. Expected an integer."
                        );
                    }

                    if (tileId == -1) continue; // The Tiled editor sets air to -1

                    var tilePosition = new Vector2(col, row);
                    tiles[tilePosition] = tileId;

                    if (col > topRightMostTile.X)
                    {
                        topRightMostTile = tilePosition;
                    }
                }

                line = levelReader.ReadLine();
                row++;
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        foreach ((Vector2 tilePosition, int tileId) in tiles)
        {
            var worldPosition = tilePosition * Grid.TileLength + levelOffset;
            Tileset.DrawTile(game.SpriteBatch, tileId, worldPosition);
        }

        foreach ((Vector2 tilePosition, int tileId) in lightTiles)
        {
            var worldPosition = tilePosition * Grid.TileLength + levelOffset;
            PlayerLightTileset.DrawTile(game.SpriteBatch, tileId, worldPosition);
        }

        foreach ((Vector2 tilePosition, int tileId) in heavyTiles)
        {
            var worldPosition = tilePosition * Grid.TileLength + levelOffset;
            PlayerHeavyTileset.DrawTile(game.SpriteBatch, tileId, worldPosition);
        }
    }

    public void PlaceLightTileAt(Vector2 worldPosition, int tileId)
    {
        if(CanPlaceLightTile(worldPosition) == false)
        {
            return;
        }
        var tilePosition = Grid.WorldToTilePosition(worldPosition-levelOffset);
        lightTiles[tilePosition] = tileId;
    }

    public void PlaceHeavyTileAt(Vector2 worldPosition, int tileId)
    {
        if(CanPlaceHeavyTile(worldPosition) == false)
        {
            return;
        }
        var tilePosition = Grid.WorldToTilePosition(worldPosition-levelOffset);
        heavyTiles[tilePosition] = tileId;
    }

    public bool CanPlaceLightTile(Vector2 worldPosition)
    {
        var tilePosition = Grid.WorldToTilePosition(worldPosition - levelOffset);
        return tiles.ContainsKey(tilePosition + new Vector2(0,1)) || lightTiles.ContainsKey(tilePosition + new Vector2(0,1)) 
        || heavyTiles.ContainsKey(tilePosition + new Vector2(0,1)) && !AnyTileExistsAtTilePosition(tilePosition);
    }

    public bool CanPlaceHeavyTile(Vector2 worldPosition)
    {
        var tilePosition = Grid.WorldToTilePosition(worldPosition - levelOffset);
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                if(i == 0 && j==i)
                {
                    if(AnyTileExistsAtTilePosition(tilePosition))
                    {
                        return false;
                    }
                    continue;
                }

                if(tiles.ContainsKey(tilePosition + new Vector2(i,j)) || lightTiles.ContainsKey(tilePosition + new Vector2(0,1)) 
                || heavyTiles.ContainsKey(tilePosition + new Vector2(i,j)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Vector2 GetRightMostTopTileWorldPosition()
    {
        return topRightMostTile * Grid.TileLength + levelOffset;
    }

    public Vector2 GetFirstTilePosition()
    {
        return tiles.Keys.First() * Grid.TileLength + levelOffset;
    }

    public bool SolidTileExistsAtPosition(Vector2 worldPosition)
    {
        var tilePosition = worldPosition - levelOffset / Grid.TileLength;
        return tiles.ContainsKey(tilePosition) || heavyTiles.ContainsKey(tilePosition);
    }
    public bool AnyTileExistsAtTilePosition(Vector2 tilePosition)
    {
        return tiles.ContainsKey(tilePosition) || heavyTiles.ContainsKey(tilePosition) || lightTiles.ContainsKey(tilePosition);
    }
}
