using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class CameraManager : GameComponent
{
    private Game1 game;
    private Vector2 _position = new(400, 400);
    public CameraManager(Game game) : base(game)
    {
        this.game = (Game1)game;

    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState currentState = Keyboard.GetState();
        var speed = 500;
        if (currentState.IsKeyDown(Keys.A))
            _position.X -= deltaTime * speed;
        if (currentState.IsKeyDown(Keys.D))
            _position.X += deltaTime * speed;
        if (currentState.IsKeyDown(Keys.W))
            _position.Y -= deltaTime * speed;
        if (currentState.IsKeyDown(Keys.S))
            _position.Y += deltaTime * speed;

        var scroll = (float)Mouse.GetState().ScrollWheelValue / 1000f;
        if (scroll < 0)
        {
            scroll = Math.Abs(scroll);
            Camera.SetScale(1 / (scroll+1));
        }

        Camera.SetPosition(_position);
        base.Update(gameTime);
    }
}