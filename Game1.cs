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
    public Vector2 RenderTargetSize;
    public Vector2 RenderedBlackBoxSize;
    public int NativeScreenWidth = 800;
    public int NativeScreenHeight = 480;

    private UIComponent ui;
    private MainMenuUIComponent mainMenu;
    private RenderTarget2D renderTarget;
    private Rectangle renderDestination;

    public Game1()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Graphics.PreferredBackBufferWidth = NativeScreenWidth;
        Graphics.PreferredBackBufferHeight = NativeScreenHeight;
        Graphics.ApplyChanges();
        // Graphics.IsFullScreen = true;

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
        CalculateRenderDestination();

        SceneManager.SceneLoaded += InitializeScene;
    }

    protected override void Initialize()
    {
        renderTarget = new(GraphicsDevice, NativeScreenWidth, NativeScreenHeight);

        AssetManager.Initialize(Content);
        InputSystem.Initialize(this);
        // Load here to prevent components from trying to access assets before they're loaded.
        AssetManager.LoadAllAssets();
        Camera.Initialize(this);
        ClickManager.Initialize(this);

        SceneManager.LoadMainMenu();

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
        WaveSystem.Update(gameTime);
        ClickManager.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            SceneManager.LoadMainMenu();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render to custom render target that can be scaled based on display size
        GraphicsDevice.SetRenderTarget(renderTarget);
        GraphicsDevice.Clear(Color.FromNonPremultiplied(new Vector4(115/255f, 55/255f, 57/255f, 1)));

        Matrix translation = Camera.CalculateTranslation();

        SpriteBatch.Begin(transformMatrix: translation, sortMode: SpriteSortMode.BackToFront,
            samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default);

        base.Draw(gameTime);
        SpriteBatch.End();

        // Draw UI separately after everything else to avoid it from being moved by the camera.
        if (ui is not null)
        {
            SpriteBatch.Begin();
            ui.Draw(gameTime);
            SpriteBatch.End();
        }

        if (mainMenu is not null)
        {
            SpriteBatch.Begin();
            mainMenu.Draw(gameTime);
            SpriteBatch.End();
        }

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
        var xScale = (float)screenSize.X / NativeScreenWidth;
        var yScale = (float)screenSize.Y / NativeScreenHeight;
        var scale = Math.Min(xScale, yScale);

        var xSize = NativeScreenWidth * scale;
        var ySize = NativeScreenHeight * scale;
        RenderTargetSize = new Vector2(xSize, ySize);
        RenderedBlackBoxSize = new Vector2(screenSize.X - xSize, screenSize.Y - ySize);
        var xPos = RenderedBlackBoxSize.X / 2;
        var yPos = RenderedBlackBoxSize.Y / 2;

        renderDestination = new Rectangle((int)xPos, (int)yPos, (int)xSize, (int)ySize);
    }

    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        CalculateRenderDestination();
    }

    private void InitializeScene(SceneManager.Scene loadedScene)
    {
        Components.Clear();
        ui = null;
        mainMenu = null;
        Terrain = null;

        switch (loadedScene)
        {
            case SceneManager.Scene.Game:
                BuildingSystem.Initialize(this);
                WaveSystem.Initialize(this);

                Terrain = new Terrain(this);
                Components.Add(Terrain);

                ui = new UIComponent(this);
                Components.Add(ui);

                var cameraManger = new CameraManager(this);
                Components.Add(cameraManger);

                var parallax = new Parallax(this);
                Components.Add(parallax);

                /*EnemySystem.SpawnFridgeEnemy(this, new Vector2(10, 400));
                  EnemySystem.SpawnWalkerEnemy(this, new Vector2(30, 400));
                  EnemySystem.SpawnWalkerEnemy(this, new Vector2(50, 400));
                  EnemySystem.SpawnWalkerEnemy(this, new Vector2(70, 400));
                  EnemySystem.SpawnWalkerEnemy(this, new Vector2(90, 400));
                  EnemySystem.SpawnWalkerEnemy(this, new Vector2(110, 400));
                  EnemySystem.SpawnWalkerEnemy(this, new Vector2(130, 400));*/
                break;
            case SceneManager.Scene.Menu:
                mainMenu = new MainMenuUIComponent(this);
                Components.Add(mainMenu);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Loaded scene '{loadedScene}' did not match any scene in SceneManager.Scene.");
        }
    }
}
