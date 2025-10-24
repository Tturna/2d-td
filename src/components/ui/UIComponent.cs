using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class UIComponent : DrawableGameComponent
{
    Game1 game;

    private List<UIEntity> uiElements = new();
    private List<UIEntity> pauseMenuElements = new();
    private List<UIEntity> winScreenElements = new();
    private List<UIEntity> loseScreenElements = new();
    private UIEntity turretHologram;
    private UIEntity currencyText;
    private UIEntity waveIndicator;
    private bool isPauseMenuVisible;
    private bool escHeld;
    private bool isWon;
    private bool isLost;
    private int buyButtonCount = 0;

    private static SpriteFont defaultFont = AssetManager.GetFont("default");
    private static Texture2D buttonSprite = AssetManager.GetTexture("btn_square_empty");
    private float halfScreenWidth = Game1.Instance.NativeScreenWidth / 2;
    private float halfScreenHeight = Game1.Instance.NativeScreenHeight / 2;
    private static Vector2 buttonFrameSize = new Vector2(buttonSprite.Bounds.Width, buttonSprite.Bounds.Height);
    private readonly Vector2 scrapTextOffset = new Vector2(3, 6);

    private AnimationSystem.AnimationData buttonAnimationData = new
    (
        texture: buttonSprite,
        frameCount: 1,
        frameSize: buttonFrameSize,
        delaySeconds: 0
    );

    public static UIComponent Instance;

    public UIComponent(Game game) : base(game)
    {
        this.game = (Game1)game;
        Instance = this;
    }

    private void CreateTowerBuyButton<T>(Texture2D towerIcon, BuildingSystem.TowerType towerType) where T : ITower
    {
        var priceIcon = AssetManager.GetTexture("icon_scrap_small");
        var turretIcon = new UIEntity(game, uiElements, towerIcon);
        var turretButton = new UIEntity(game, uiElements, Vector2.Zero, buttonAnimationData);
        var turretPriceIcon = new UIEntity(game, uiElements, priceIcon);
        var turretPriceText = new UIEntity(game, uiElements, defaultFont, CurrencyManager.GetTowerPrice(towerType).ToString());
        turretButton.ButtonPressed += () => SelectTurret<T>();

        const float Margin = 20;
        const float Gap = 32;
        const int towers = 6;
        Vector2 priceIconOffset = new Vector2(3, 3);
        Vector2 priceTextOffset = new Vector2(2, -4);

        var xPos = game.NativeScreenWidth / 2 - buttonFrameSize.X / 2
            + buttonFrameSize.X * buyButtonCount + Gap * buyButtonCount
            - (buttonFrameSize.X / 2) * (towers - 1) - (Gap / 2 * (towers - 1));
        var yPos = game.NativeScreenHeight - buttonFrameSize.Y - Margin;
        var pos = new Vector2(xPos, yPos);
        var buttonCenter = pos + new Vector2(buttonFrameSize.X / 2, buttonFrameSize.Y / 2);
        var iconPosition = buttonCenter - new Vector2(turretIcon.Size.X / 2, turretIcon.Size.Y / 2);

        turretButton.Position = pos;
        turretIcon.Position = iconPosition;
        turretIcon.DrawLayerDepth = 0.7f;
        turretPriceIcon.Position = turretButton.Position + new Vector2(priceIconOffset.X,
            turretButton.Size.Y + priceIconOffset.Y);

        turretPriceText.Position = turretPriceIcon.Position
            + new Vector2(priceIcon.Width + priceTextOffset.X, priceTextOffset.Y);

        turretPriceText.Scale = Vector2.One * 0.8f; // temp until better font
        buyButtonCount++;
    }

    public override void Initialize()
    {
        HQ.Instance.HealthSystem.Died += ShowGameOverScreen;
        WaveSystem.LevelWin += ShowLevelWinScreen;

        var scrapIconTexture = AssetManager.GetTexture("icon_scrap");
        var scrapIcon = new UIEntity(game, uiElements, scrapIconTexture);
        currencyText = new UIEntity(game, uiElements, defaultFont, $"{CurrencyManager.Balance}");

        var balanceTextWidth = defaultFont.MeasureString("999").X;

        scrapIcon.Position = new Vector2(game.NativeScreenWidth / 2 - scrapIconTexture.Width / 2
            - balanceTextWidth / 2 - scrapTextOffset.X / 2,
            game.NativeScreenHeight - 64);
        currencyText.Position = scrapIcon.Position;
        currencyText.Position += Vector2.UnitX * (scrapIconTexture.Width + scrapTextOffset.X);
        currencyText.Position -= Vector2.UnitY * scrapTextOffset.Y;

        waveIndicator = new UIEntity(game, uiElements, defaultFont, "Wave 0 of 0");
        var waveTextWidth = defaultFont.MeasureString("Wave 9 of 9").X;
        waveIndicator.Position = new Vector2(game.NativeScreenWidth - waveTextWidth, 0);

        var gunTurretSprite = AssetManager.GetTexture("gunTurretBase");
        var turretTwoSprite = AssetManager.GetTexture("turretTwo");

        CreateTowerBuyButton<GunTurret>(gunTurretSprite, BuildingSystem.TowerType.GunTurret);
        CreateTowerBuyButton<Railgun>(turretTwoSprite, BuildingSystem.TowerType.Railgun);
        CreateTowerBuyButton<Drone>(turretTwoSprite, BuildingSystem.TowerType.Drone);
        CreateTowerBuyButton<Crane>(turretTwoSprite, BuildingSystem.TowerType.Crane);
        CreateTowerBuyButton<Mortar>(gunTurretSprite, BuildingSystem.TowerType.Mortar);
        CreateTowerBuyButton<Hovership>(turretTwoSprite, BuildingSystem.TowerType.Hovership);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        if (turretHologram is not null)
        {
            var mouseWorldPos = InputSystem.GetMouseWorldPosition();
            var mouseWorldGridPos = Grid.SnapPositionToGrid(mouseWorldPos);
            var mouseSnappedScreenPos = Camera.WorldToScreenPosition(mouseWorldGridPos);

            turretHologram.Position = mouseSnappedScreenPos;
            var size = Camera.Scale;
            turretHologram.Scale = new Vector2(size, size);
        }

        if (InputSystem.IsRightMouseButtonClicked())
        {
            RemoveTurretHologram();
            BuildingSystem.DeselectTower();
        }

        currencyText.Text = $"{CurrencyManager.Balance}";
        waveIndicator.Text = $"Wave {WaveSystem.CurrentWaveIndex + 1} of {WaveSystem.MaxWaveIndex}";

        var kbdState = Keyboard.GetState();
        if (!escHeld && kbdState.IsKeyDown(Keys.Escape))
        {
            escHeld = true;
            TogglePauseMenu(!isPauseMenuVisible);
        }

        if (kbdState.IsKeyUp(Keys.Escape)) escHeld = false;

        base.Update(gameTime);
    }

    // Don't override the component's Draw function to prevent it from being called automatically.
    // This is so that it can be called manually after everything else in Game1.
    // This avoids the main sprite batch from translating the UI when the camera moves.
    new public void Draw(GameTime gameTime)
    {
        // Call Draw() manually on each UI element so they don't move with the camera.
        // Game1 already creates a new sprite batch for this function call.
        foreach (UIEntity uiElement in uiElements)
        {
            uiElement.DrawCustom(gameTime);
        }

        base.Draw(gameTime);
    }

    private void RemoveTurretHologram()
    {
        if (turretHologram is not null)
        {
            turretHologram.Destroy();
            turretHologram = null;
        }
    }

    private void CreateTurretHologram(AnimationSystem.AnimationData animationData)
    {
        RemoveTurretHologram();

        turretHologram = new UIEntity(game, uiElements, Vector2.Zero, animationData);
    }

    private void SelectTurret<T>() where T : ITower
    {
        BuildingSystem.SelectTurret<T>();
        var turretAnimationData = T.GetTowerBaseAnimationData();
        CreateTurretHologram(turretAnimationData);
    }

    private void TogglePauseMenu(bool isPauseMenuVisible)
    {
        game.SetPauseState(isPauseMenuVisible);
        this.isPauseMenuVisible = isPauseMenuVisible;

        if (!isPauseMenuVisible)
        {
            foreach (var element in pauseMenuElements)
            {
                element.Destroy();
            }

            pauseMenuElements.Clear();

            if (isWon)
            {
                ShowLevelWinScreen();
            }
            else if (isLost)
            {
                ShowGameOverScreen();
            }

            return;
        }

        // clear win and loss screens
        foreach (var element in winScreenElements)
        {
            element.Destroy();
        }

        winScreenElements.Clear();

        foreach (var element in loseScreenElements)
        {
            element.Destroy();
        }

        loseScreenElements.Clear();

        var playButtonPos = new Vector2(halfScreenWidth - buttonFrameSize.X / 2, halfScreenHeight - buttonFrameSize.Y / 2);
        var resumeButton = new UIEntity(game, uiElements, playButtonPos, buttonAnimationData);

        var exitButtonPos = new Vector2(halfScreenWidth - buttonFrameSize.X / 2, halfScreenHeight + buttonFrameSize.Y / 2 + 10);
        var exitButton = new UIEntity(game, uiElements, exitButtonPos, buttonAnimationData);

        resumeButton.ButtonPressed += () => TogglePauseMenu(!isPauseMenuVisible);
        exitButton.ButtonPressed += () => SceneManager.LoadMainMenu();

        var resumeButtonText = new UIEntity(game, uiElements, defaultFont, "Resume");
        var exitButtonText = new UIEntity(game, uiElements, defaultFont, "Exit");
        resumeButtonText.Position = playButtonPos + resumeButton.Size / 2 - resumeButtonText.Size / 2;
        exitButtonText.Position = exitButtonPos + exitButton.Size / 2 - exitButtonText.Size / 2;

        pauseMenuElements.Add(resumeButton);
        pauseMenuElements.Add(exitButton);
        pauseMenuElements.Add(resumeButtonText);
        pauseMenuElements.Add(exitButtonText);
    }

    private void ShowGameOverScreen(Entity _ = null)
    {
        isLost = true;

        var retryButtonPos = new Vector2(halfScreenWidth - buttonFrameSize.X / 2, halfScreenHeight - buttonFrameSize.Y / 2);
        var retryButton = new UIEntity(game, uiElements, retryButtonPos, buttonAnimationData);

        var quitButtonPos = new Vector2(halfScreenWidth - buttonFrameSize.X / 2, halfScreenHeight + buttonFrameSize.Y / 2 + 10);
        var quitButton = new UIEntity(game, uiElements, quitButtonPos, buttonAnimationData);

        retryButton.ButtonPressed += () => SceneManager.LoadGame();
        quitButton.ButtonPressed += () => SceneManager.LoadMainMenu();

        var retryButtonText = new UIEntity(game, uiElements, defaultFont, "Retry");
        var exitButtonText = new UIEntity(game, uiElements, defaultFont, "Exit");
        retryButtonText.Position = retryButtonPos + retryButton.Size / 2 - retryButtonText.Size / 2;
        exitButtonText.Position = quitButtonPos + quitButton.Size / 2 - exitButtonText.Size / 2;

        loseScreenElements.Add(retryButton);
        loseScreenElements.Add(quitButton);
        loseScreenElements.Add(retryButtonText);
        loseScreenElements.Add(exitButtonText);
    }

    private void ShowLevelWinScreen()
    {
        // prevent showing the win screen if you've already lost
        if (isLost) return;

        isWon = true;
        var beatTheGame = game.CurrentZone == 3 && game.CurrentLevel == 5;

        var quitButtonPos = new Vector2(halfScreenWidth - buttonFrameSize.X / 2, halfScreenHeight + buttonFrameSize.Y / 2 + 10);
        var quitButton = new UIEntity(game, uiElements, quitButtonPos, buttonAnimationData);

        if (!beatTheGame)
        {
            var nextLevelButtonPos = new Vector2(halfScreenWidth - buttonFrameSize.X / 2, halfScreenHeight - buttonFrameSize.Y / 2);
            var nextLevelButton = new UIEntity(game, uiElements, nextLevelButtonPos, buttonAnimationData);
            var nextLevelButtonText = new UIEntity(game, uiElements, defaultFont, "Next Level");
            nextLevelButtonText.Position = nextLevelButtonPos + nextLevelButton.Size / 2 - nextLevelButtonText.Size / 2;

            nextLevelButton.ButtonPressed += () =>
            {
                var nextLevel = game.CurrentLevel + 1;
                var nextZone = game.CurrentZone;

                if (game.CurrentLevel >= 5)
                {
                    nextLevel = 1;
                    nextZone++;
                }

                game.SetCurrentZoneAndLevel(nextZone, nextLevel);
                SceneManager.LoadGame();
            };

            winScreenElements.Add(nextLevelButton);
            winScreenElements.Add(nextLevelButtonText);
        }

        quitButton.ButtonPressed += () => SceneManager.LoadMainMenu();

        var exitButtonText = new UIEntity(game, uiElements, defaultFont, "Exit");
        exitButtonText.Position = quitButtonPos + quitButton.Size / 2 - exitButtonText.Size / 2;

        winScreenElements.Add(quitButton);
        winScreenElements.Add(exitButtonText);
    }

    public void AddUIEntity(UIEntity entity)
    {
        uiElements.Add(entity);
    }

    public bool RemoveUIEntity(UIEntity entity)
    {
        return uiElements.Remove(entity);
    }
}
