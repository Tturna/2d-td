using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class Terrain : DrawableGameComponent
{
    private Game1 game;
    private Tileset tileset;

    private Dictionary<Entity, int> tiles = new();
    private Vector2 levelOffset = new Vector2(0, 32 * Grid.TileLength);

    public Terrain(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        string levelPath = Path.Combine(AppContext.BaseDirectory, game.Content.RootDirectory,
                "data", "levels", "testlevel", "testlevel.csv");
        var levelReader = new StreamReader(levelPath);
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
                        $"Invalid value '{ids[col]}' at row {row}, column {col + 1} in {levelPath}. Expected an integer."
                    );
                }

                if (tileId == -1) continue; // The Tiled editor sets air to -1

                var worldPosition = new Vector2(col, row) * Grid.TileLength + levelOffset;
                var tile = new Entity(game, worldPosition, sprite: null, size: Vector2.One * Grid.TileLength);
                tiles[tile] = tileId;
                game.Components.Add(tile);
            }

            line = levelReader.ReadLine();
            row++;
        }

        tileset = new Tileset(AssetManager.GetTexture("tileset"),
                tilesetWidth: 2,
                tilesetHeight: 1);
    }

    // This was used to test tile collisions.
    // public override void Update(GameTime gameTime)
    // {
    //     var mouseWorldPos = InputSystem.GetMouseWorldPosition();
    //
    //     foreach ((Entity tile, var _) in tiles)
    //     {
    //         if (Collision.IsPointInEntity(mouseWorldPos, tile))
    //         {
    //             Console.WriteLine($"Mouse ({mouseWorldPos.ToString()}) colliding with tile ({tile.Position.ToString()})");
    //             break;
    //         }
    //     }
    // }

    public override void Draw(GameTime gameTime)
    {
        foreach ((Entity tile, int tileId) in tiles)
        {
            tileset.DrawTile(game.SpriteBatch, tileId, tile.Position);
        }
    }
}
