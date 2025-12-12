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
    public const float FixedDeltaTime = 1f / 60f;
    public SpriteFont DefaultFont;
    public bool IsPaused;

    private UIComponent ui;
    private MainMenuUIComponent mainMenu;
    private RenderTarget2D renderTarget;
    private Rectangle renderDestination;
    private float physicsTimer;

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
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        renderTarget = new(GraphicsDevice, NativeScreenWidth, NativeScreenHeight);

        // Load here to prevent components from trying to access assets before they're loaded.
        AssetManager.Initialize(Content);
        AssetManager.LoadAllAssets();
        DefaultFont = AssetManager.GetFont("pixelsix");

        InputSystem.Initialize(this);
        Camera.Initialize(this);
        ParticleSystem.Initialize(this);

        SceneManager.LoadMainMenu();

        SavingSystem.LoadGame();

        base.Initialize();
    }

    protected override void LoadContent() { }

    protected override void Update(GameTime gameTime)
    {
        InputSystem.Update();

        if (IsPaused)
        {
            if (ui is not null) ui.Update(gameTime);
            if (mainMenu is not null) mainMenu.Update(gameTime);
            if (SceneManager.CurrentScene == SceneManager.Scene.Game)
            {
                DebugUtility.Update(this, gameTime);
            }

            return;
        }

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        physicsTimer += deltaTime;
        var componentsToUpdate = new GameComponent[Components.Count];
        Components.CopyTo(componentsToUpdate, 0);

        while (physicsTimer >= FixedDeltaTime) {
            foreach (var component in componentsToUpdate)
            {
                if (component is not Entity) continue;
                var ent = component as Entity;
                ent.FixedUpdate(FixedDeltaTime);
            }

            if (SceneManager.CurrentScene == SceneManager.Scene.Game)
            {
                DebugUtility.FixedUpdate();
            }

            ParticleSystem.FixedUpdate();

            physicsTimer -= FixedDeltaTime;
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

        if (!IsPaused)
        {
            DebugUtility.ResetLines();
        }

        SpriteBatch.End();

        ParticleSystem.DrawParticles(SpriteBatch, translation);

        // Draw UI separately after everything else to avoid it from being moved by the camera.
        if (ui is not null)
        {
            SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront,
                samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default);
            ui.Draw(gameTime);
            DebugUtility.DrawDebugScreen(SpriteBatch);

            // var mousePoint = InputSystem.GetMouseScreenPosition();
            // LineUtility.DrawCircle(SpriteBatch, mousePoint, radius: 60f, Color.Red, thickness: 1f,
            //     resolution: 24);

            SpriteBatch.End();
        }

        if (mainMenu is not null)
        {
            SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront,
                samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default);
            mainMenu.Draw(gameTime);

            var infoPos = new Vector2(10, NativeScreenHeight - 40);
            SpriteBatch.DrawString(DefaultFont, AppContext.BaseDirectory, infoPos, Color.White);

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

        ProgressionManager.Initialize();

        switch (loadedScene)
        {
            case SceneManager.Scene.Game:
                BuildingSystem.Initialize(this);
                WaveSystem.Initialize(this, CurrentZone, CurrentLevel);
                CurrencyManager.Initialize();
                ScrapSystem.Initialize();

                Terrain = new Terrain(this, CurrentZone, CurrentLevel);

                Components.Add(Terrain);

                var hq = new HQ(this, Vector2.Zero);
                var lastTilePosition = Terrain.GetRightMostTopTileWorldPosition();
                var hqPosition = lastTilePosition - hq.Size;
                hq.SetPosition(hqPosition);

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
        this.IsPaused = isPaused;
    }
}
