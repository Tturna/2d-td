using System;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace TDgame.Components;

// this is a class that contains the components for tiles
public class Tile : DrawableGameComponent
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public string texture { get; set; }

    public Tile(Game game, int x, int y, string texture) : base(game)
    {
        X = x;
        Y = y;
        this.texture = texture;
    }

    // monogame methods
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {

        base.Draw(gameTime);
    }
}

