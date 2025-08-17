using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class Game1 : Game
{
    public GraphicsDeviceManager Graphics;
    public SpriteBatch SpriteBatch;
    public Terrain Terrain;

    private UIComponent ui;
    private RenderTarget2D renderTarget;
    private Rectangle renderDestination;
    private int nativeScreenWidth = 800;
    private int nativeScreenHeight = 480;

    public Game1()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Graphics.PreferredBackBufferWidth = nativeScreenWidth;
        Graphics.PreferredBackBufferHeight = nativeScreenHeight;
        Graphics.ApplyChanges();
        // Graphics.IsFullScreen = true;

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
        CalculateRenderDestination();
    }

    protected override void Initialize()
    {
        renderTarget = new(GraphicsDevice, nativeScreenWidth, nativeScreenHeight);

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

        EnemySystem.SpawnFridgeEnemy(this, new Vector2(10, 400));
        EnemySystem.SpawnWalkerEnemy(this, new Vector2(30, 400));
        EnemySystem.SpawnWalkerEnemy(this, new Vector2(50, 400));
        EnemySystem.SpawnWalkerEnemy(this, new Vector2(70, 400));
        EnemySystem.SpawnWalkerEnemy(this, new Vector2(90, 400));
        EnemySystem.SpawnWalkerEnemy(this, new Vector2(110, 400));
        EnemySystem.SpawnWalkerEnemy(this, new Vector2(130, 400));

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
        // GraphicsDevice.Clear(Color.CornflowerBlue);

        // Render to custom render target that can be scaled based on display size
        GraphicsDevice.SetRenderTarget(renderTarget);

        Matrix translation = Camera.CalculateTranslation();

        SpriteBatch.Begin(transformMatrix: translation, sortMode: SpriteSortMode.BackToFront,
            samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default);

        base.Draw(gameTime);
        SpriteBatch.End();

        // Draw UI separately after everything else to avoid it from being moved by the camera.
        SpriteBatch.Begin();
        ui.Draw(gameTime);
        SpriteBatch.End();

        // Render to the back buffer so the custom render target is visible
        GraphicsDevice.SetRenderTarget(null);
        // GraphicsDevice.Clear(Color.DarkGray);

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        SpriteBatch.Draw(renderTarget, renderDestination, Color.White);
        SpriteBatch.End();
    }

    private void CalculateRenderDestination()
    {
        Point screenSize = GraphicsDevice.Viewport.Bounds.Size;
        var xScale = (float)screenSize.X / nativeScreenWidth;
        var yScale = (float)screenSize.Y / nativeScreenHeight;
        var scale = Math.Min(xScale, yScale);

        var xSize = nativeScreenWidth * scale;
        var ySize = nativeScreenHeight * scale;
        var xPos = (screenSize.X - xSize) / 2;
        var yPos = (screenSize.Y - ySize) / 2;

        renderDestination = new Rectangle((int)xPos, (int)yPos, (int)xSize, (int)ySize);
    }

    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        CalculateRenderDestination();
    }
}
