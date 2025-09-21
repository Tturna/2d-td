using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class Terrain : DrawableGameComponent
{
    private Game1 game;
    private Tileset tileset;

    private Dictionary<Vector2, int> tiles = new();
    private Vector2 levelOffset = new Vector2(0, 64 * Grid.TileLength);

    public Terrain(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        string levelPath = Path.Combine(AppContext.BaseDirectory, game.Content.RootDirectory,
                "data", "levels", "purptest", "purptest1.csv");

        // "using" makes it so Dispose() is automatically called on StreamReader when
        // it's not needed anymore.
        using (var levelReader = new StreamReader(levelPath))
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
                            $"Invalid value '{ids[col]}' at row {row}, column {col + 1} in {levelPath}. Expected an integer."
                        );
                    }

                    if (tileId == -1) continue; // The Tiled editor sets air to -1

                    var tilePosition = new Vector2(col, row);
                    // var worldPosition = tilePosition * Grid.TileLength + levelOffset;
                    // var tile = new Entity(game, worldPosition, sprite: null, size: Vector2.One * Grid.TileLength);
                    tiles[tilePosition] = tileId;
                    // game.Components.Add(tile);
                }

                line = levelReader.ReadLine();
                row++;
            }
        }

        tileset = new Tileset(AssetManager.GetTexture("purptiles"),
                tilesetWidth: 12,
                tilesetHeight: 4);
    }

    // This was used to test tile collisions.
    // public override void Update(GameTime gameTime)
    // {
    //     var mouseWorldPos = InputSystem.GetMouseWorldPosition();
    //
    //     foreach ((Vector2 tilePosition, var _) in tiles)
    //     {
    //         var tileWorldPosition = tilePosition * Grid.TileLength + levelOffset;
    //
    //         if (Collision.IsPointInTile(mouseWorldPos, tileWorldPosition))
    //         {
    //             Console.WriteLine($"Mouse ({mouseWorldPos.ToString()}) colliding with tile ({tilePosition.ToString()})");
    //             break;
    //         }
    //     }
    // }

    public override void Draw(GameTime gameTime)
    {
        foreach ((Vector2 tilePosition, int tileId) in tiles)
        {
            var worldPosition = tilePosition * Grid.TileLength + levelOffset;
            tileset.DrawTile(game.SpriteBatch, tileId, worldPosition);
        }
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
