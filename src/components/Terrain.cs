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
    private readonly string LevelPath;

    private Dictionary<Vector2, int> tiles = new();
    private Vector2 levelOffset = new Vector2(0, 64 * Grid.TileLength);

    public Terrain(Game game, int zone, int level, string tilesetName = "purptiles",
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
    }

    public Vector2 GetFirstTilePosition()
    {
        return tiles.Keys.First() * Grid.TileLength + levelOffset;
    }

    public Vector2 GetLastTilePosition()
    {
        return tiles.Keys.Last() * Grid.TileLength + levelOffset;
    }

    public bool TileExistsAtPosition(Vector2 worldPosition)
    {
        var tilePosition = worldPosition - levelOffset / Grid.TileLength;
        return tiles.ContainsKey(tilePosition);
    }
}
