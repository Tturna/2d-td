using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private InputSystem _inputSystem;

    private Texture2D turretTexture;
    private List<Vector2> turretPositions = new();

    private Vector2 gridMousePosition;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _inputSystem = new InputSystem();
        Services.AddService(typeof(InputSystem), _inputSystem);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        turretTexture = Content.Load<Texture2D>("sprites/turret");
    }

    protected override void Update(GameTime gameTime)
    {
        _inputSystem.Update();
        gridMousePosition = Grid.SnapPositionToGrid(_inputSystem.GetMousePosition());
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (_inputSystem.IsLeftMouseButtonClicked())
        {
            turretPositions.Add(gridMousePosition);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _spriteBatch.Draw(turretTexture, gridMousePosition, Color.White);

        for (int i = 0; i < turretPositions.Count; i++)
        {
            _spriteBatch.Draw(turretTexture, turretPositions[i], Color.White);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
