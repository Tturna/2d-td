using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class CameraManager : GameComponent
{
    private Game1 game;

    public CameraManager(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        Camera.Position = Vector2.One * 400;

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState currentState = Keyboard.GetState();

        Vector2 posChange = Vector2.Zero;

        if (currentState.IsKeyDown(Keys.A))
            posChange.X -= deltaTime * 100;
        if (currentState.IsKeyDown(Keys.D))
            posChange.X += deltaTime * 100;
        if (currentState.IsKeyDown(Keys.W))
            posChange.Y -= deltaTime * 100;
        if (currentState.IsKeyDown(Keys.S))
            posChange.Y += deltaTime * 100;

        var scroll = (float)Mouse.GetState().ScrollWheelValue / 1000f;
        if (scroll < 0)
        {
            scroll = Math.Abs(scroll);
            Camera.SetScale(1 / (scroll+1));
        }

        Camera.Position += posChange;
        base.Update(gameTime);
    }
}
