using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Game1 : Game
{
    public GraphicsDeviceManager Graphics;
    public SpriteBatch SpriteBatch;
    public Terrain Terrain;
    public Vector2 RenderTargetSize;
    public Vector2 RenderedBlackBoxSize;
    public int NativeScreenWidth = 640;
    public int NativeScreenHeight = 360;
    public int CurrentZone { get; private set; }
    public int CurrentLevel { get; private set; }

    private UIComponent ui;
    private MainMenuUIComponent mainMenu;
    private RenderTarget2D renderTarget;
    private Rectangle renderDestination;
    private bool isPaused;

    public static Game1 Instance { get; private set; }

    public Game1()
    {
        Instance = this;
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Graphics.PreferredBackBufferWidth = NativeScreenWidth;
        Graphics.PreferredBackBufferHeight = NativeScreenHeight;
        // Graphics.IsFullScreen = true;

        // default vsync setting
        Graphics.SynchronizeWithVerticalRetrace = true;
        Graphics.ApplyChanges();

        // FPS limit
        IsFixedTimeStep = false;

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

        if (isPaused)
        {
            if (ui is not null) ui.Update(gameTime);
            if (mainMenu is not null) mainMenu.Update(gameTime);
            if (SceneManager.CurrentScene == SceneManager.Scene.Game) DebugUtility.Update(this, gameTime);
            return;
        }

        // Console.WriteLine("Components ===============================");
        // foreach (var component in Components)
        // {
        //     Console.WriteLine(component.ToString());
        // }

        switch (SceneManager.CurrentScene)
        {
            case SceneManager.Scene.Game:
                BuildingSystem.Update(gameTime);
                WaveSystem.Update(gameTime);
                EnemySystem.Update(gameTime);
                ScrapSystem.Update(gameTime);
                DebugUtility.Update(this, gameTime);
                break;
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

        foreach (var lineTuple in DebugUtility.LineSet)
        {
            Vector2 startPoint = lineTuple.Item1;
            Vector2 endPoint = lineTuple.Item2;
            Color color = lineTuple.Item3;
            LineUtility.DrawLine(SpriteBatch, startPoint, endPoint, color, thickness: 1f);
        }

        DebugUtility.ResetLines();

        SpriteBatch.End();

        // Draw UI separately after everything else to avoid it from being moved by the camera.
        if (ui is not null)
        {
            SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront,
                samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default);
            ui.Draw(gameTime);
            DebugUtility.DrawDebugScreen(SpriteBatch);
            SpriteBatch.End();
        }

        if (mainMenu is not null)
        {
            SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront,
                samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default);
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
        SetPauseState(false);
        ui = null;
        mainMenu = null;
        Terrain = null;

        switch (loadedScene)
        {
            case SceneManager.Scene.Game:
                BuildingSystem.Initialize(this);
                WaveSystem.Initialize(this, CurrentZone, CurrentLevel);
                CurrencyManager.Initialize();
                ScrapSystem.Initialize();

                Terrain = new Terrain(this, CurrentZone, CurrentLevel);

                Components.Add(Terrain);
                //hqPosition will need to be flexible for each level
                var hqPosition = Terrain.GetLastTilePosition() - new Vector2(0,23*Grid.TileLength);
                var hq = new HQ(this,hqPosition);
                EnemySystem.Initialize(this);

                ui = new UIComponent(this);
                Components.Add(ui);

                var cameraManger = new CameraManager(this);
                Components.Add(cameraManger);

                var parallax = new Parallax(this);
                Components.Add(parallax);

                break;
            case SceneManager.Scene.Menu:
                mainMenu = new MainMenuUIComponent(this);
                Components.Add(mainMenu);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Loaded scene '{loadedScene}' did not match any scene in SceneManager.Scene.");
        }
    }

    public void SetCurrentZoneAndLevel(int zone, int level)
    {
        CurrentZone = zone;
        CurrentLevel = level;
    }

    public void SetPauseState(bool isPaused)
    {
        this.isPaused = isPaused;
    }
}
