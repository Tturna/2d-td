using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class Terrain : DrawableGameComponent
{
    private Game1 game;
    private Tileset tileset;

    private Dictionary<Vector2, int> level = new();
    private Vector2 levelOffset = new Vector2(0, 32 * Grid.TileLength);

    public Terrain(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        string levelPath = Path.Combine(AppContext.BaseDirectory, game.Content.RootDirectory,
                "data", "levels", "testlevel", "testlevel.csv");
        var reader = new StreamReader(levelPath);
        string line = reader.ReadLine();
        var row = 0;

        while (line is not null)
        {
            String[] ids = line.Split(",");

            for (int col = 0; col < ids.Length; col++)
            {
                if (!int.TryParse(ids[col], out int tileId))
                {
                    throw new InvalidDataException(
                        $"Invalid value '{ids[col]}' at row {row}, column {col + 1} in {levelPath}. Expected an integer."
                    );
                }

                if (tileId == -1) continue; // The Tiled editor sets air to -1

                level[new Vector2(col, row)] = tileId;
            }

            line = reader.ReadLine();
            row++;
        }

        tileset = new Tileset(AssetManager.GetTexture("tileset"),
                tilesetWidth: 2,
                tilesetHeight: 1);
    }

    public override void Draw(GameTime gameTime)
    {
        foreach ((Vector2 tilePosition, int tileId) in level)
        {
            var position = tilePosition * Grid.TileLength + levelOffset;
            tileset.DrawTile(game.SpriteBatch, tileId, position);
        }
    }
}
