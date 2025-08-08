using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class CameraManager : GameComponent
{
    private Game1 game;
    private float currentScale = 1;

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

        var Speed = 500;
        if (currentState.IsKeyDown(Keys.A))
            posChange.X -= deltaTime * Speed;
        if (currentState.IsKeyDown(Keys.D))
            posChange.X += deltaTime * Speed;
        if (currentState.IsKeyDown(Keys.W))
            posChange.Y -= deltaTime * Speed;
        if (currentState.IsKeyDown(Keys.S))
            posChange.Y += deltaTime * Speed;

        // scroll down is negative
        var scroll = -(float)InputSystem.mouseJustScrolledAmount() / 1000f;
        // can't go above 1, so limit it
        currentScale = Math.Max(1, currentScale + scroll);
        // eg. 1/8 will make it zoom out 8x
        var totalCameraScale = 1 / currentScale;
        Camera.Scale = totalCameraScale;

        Camera.Position += posChange;
        base.Update(gameTime);
    }
}
