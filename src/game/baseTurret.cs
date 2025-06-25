using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TDgame.Components;

public class Turret : Entity
{

    public Turret(Game game, Vector2 position) : base(game, position, 32, 32, new BasicHealthManager())
    {

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