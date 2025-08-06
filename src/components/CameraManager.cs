using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class CameraManager : GameComponent
{
    private Game1 game;
    private Vector2 _position;
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
        if (currentState.IsKeyDown(Keys.A))
            _position.X -= deltaTime * 100;
        if (currentState.IsKeyDown(Keys.D))
            _position.X += deltaTime * 100;
        if (currentState.IsKeyDown(Keys.W))
            _position.Y -= deltaTime * 100;
        if (currentState.IsKeyDown(Keys.S))
            _position.Y += deltaTime * 100;


        Camera.SetPosition(_position);
        base.Update(gameTime);
    }
}