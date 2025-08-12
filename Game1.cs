using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class Game1 : Game
{
    public GraphicsDeviceManager Graphics;
    public SpriteBatch SpriteBatch;

    public List<Entity> Enemies = new();
    public Terrain Terrain;

    private UIComponent ui;

    public Game1()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        AssetManager.Initialize(Content);
        // Load here to prevent components from trying to access assets before they're loaded.
        AssetManager.LoadAllAssets();
        Camera.Initialize(GraphicsDevice);
        BuildingSystem.Initialize(this);

        Terrain = new Terrain(this);
        Components.Add(Terrain);

        ui = new UIComponent(this);
        Components.Add(ui);

        var cameraManger = new CameraManager(this);
        Components.Add(cameraManger);

        var parallax = new Parallax(this);
        Components.Add(parallax);

        var mockEnemy = new Enemy(this, Vector2.One * 200);
        Components.Add(mockEnemy);
        Enemies.Add(mockEnemy);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        InputSystem.Update();
        BuildingSystem.Update();
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        Matrix translation = Camera.CalculateTranslation();
        SpriteBatch.Begin(transformMatrix: translation, sortMode: SpriteSortMode.BackToFront, depthStencilState: DepthStencilState.Default);

        base.Draw(gameTime);
        SpriteBatch.End();

        // Draw UI separately after everything else to avoid it from being moved by the camera.
        SpriteBatch.Begin();
        ui.Draw(gameTime);
        SpriteBatch.End();
    }
}
