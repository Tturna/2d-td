using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TDgame;


namespace TDgame.Components;

// this contains everything in the grid
public class Grid : DrawableGameComponent
{
    public int Width { get; set; }
    public int Height { get; set; }
    private Tile[] tiles;
    private int gridSize = 32;
    // public Dictionary<string, Texture2D> textures;
    
    private SpriteBatch _spriteBatch;

    public Grid(Game game, int width, int height) : base(game)
    {
        Width = width;
        Height = height;

        tiles = new Tile[Width * Height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                tiles[index] = new Tile(game, x, y, "empty_box");
                game.Components.Add(tiles[index]);
            }
        }
    }

    public Tile GetTile(int X, int Y)
    {
        int index = X + Y * Width;
        return tiles[index];
    }

    public void ForEachTile(Action<Tile> func)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = x + y * Width;
                func(tiles[index]);
            }
        }

    }

    // monogame methods
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

        // textures = new Dictionary<string, Texture2D>();
        // textures["empty_box"] = AssetManager.GetTexture("empty_box");

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        ForEachTile(tile =>
        {

        });
        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();
        ForEachTile(tile =>
        {
            Texture2D tileTexture = AssetManager.GetTexture(tile.texture);
            _spriteBatch.Draw(tileTexture, new Vector2(tile.X * gridSize, tile.Y * gridSize), Color.White);
        });
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}