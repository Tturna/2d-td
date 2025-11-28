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
    private UIEntity waveCooldownTimer;
    private UIEntity waveCooldownSkipButton;
    private UIEntity waveCooldownSkipText;
    private bool isPauseMenuVisible;
    private bool escHeld;
    private bool isWon;
    private bool isLost;
    private int buyButtonCount = 0;
    private float selectedTurretRange;

    private static SpriteFont pixelsixFont = AssetManager.GetFont("pixelsix");
    private static Texture2D buttonSprite = AssetManager.GetTexture("btn_square_empty");
    private float halfScreenWidth = Game1.Instance.NativeScreenWidth / 2;
    private float halfScreenHeight = Game1.Instance.NativeScreenHeight / 2;
    private static Vector2 buttonFrameSize = new Vector2(buttonSprite.Bounds.Width, buttonSprite.Bounds.Height);
    private readonly Vector2 scrapTextOffset = new Vector2(3, -1);

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
        var turretPriceText = new UIEntity(game, uiElements, pixelsixFont, CurrencyManager.GetTowerPrice(towerType).ToString());
        turretButton.ButtonPressed += () => SelectTurret<T>();

        const float Margin = 20;
        const float Gap = 32;
        const int towers = 6;
        Vector2 priceIconOffset = new Vector2(3, 3);
        Vector2 priceTextOffset = new Vector2(2, -2);

        var xPos = game.NativeScreenWidth / 2 - buttonFrameSize.X / 2
            + buttonFrameSize.X * buyButtonCount + Gap * buyButtonCount
            - (buttonFrameSize.X / 2) * (towers - 1) - (Gap / 2 * (towers - 1));
        var yPos = game.NativeScreenHeight - buttonFrameSize.Y - Margin;
        var pos = new Vector2(xPos, yPos);
        var buttonCenter = pos + new Vector2(buttonFrameSize.X / 2, buttonFrameSize.Y / 2);
        var iconPosition = buttonCenter - new Vector2(turretIcon.Size.X / 2, turretIcon.Size.Y / 2);

        turretButton.SetPosition(pos);
        turretIcon.SetPosition(iconPosition);
        turretIcon.DrawLayerDepth = 0.7f;
        turretPriceIcon.SetPosition(turretButton.Position + new Vector2(priceIconOffset.X,
            turretButton.Size.Y + priceIconOffset.Y));

        turretPriceText.SetPosition(turretPriceIcon.Position
            + new Vector2(priceIcon.Width + priceTextOffset.X, priceTextOffset.Y));

        buyButtonCount++;
    }

    public override void Initialize()
    {
        HQ.Instance.HealthSystem.Died += ShowGameOverScreen;
        WaveSystem.LevelWin += ShowLevelWinScreen;
        WaveSystem.WaveEnded += ShowWaveCooldownSkipButton;
        WaveSystem.WaveStarted += HideWaveCooldownSkipButton;

        var scrapIconTexture = AssetManager.GetTexture("icon_scrap");
        var scrapIcon = new UIEntity(game, uiElements, scrapIconTexture);
        currencyText = new UIEntity(game, uiElements, pixelsixFont, $"{CurrencyManager.Balance}");

        var balanceTextWidth = pixelsixFont.MeasureString("999").X * currencyText.Scale.X;

        scrapIcon.SetPosition(new Vector2(game.NativeScreenWidth / 2 - scrapIconTexture.Width / 2
            - balanceTextWidth / 2 - scrapTextOffset.X / 2,
            game.NativeScreenHeight - 64));
        scrapIcon.SetPosition(Vector2.Floor(scrapIcon.Position));

        currencyText.SetPosition(scrapIcon.Position);
        currencyText.UpdatePosition(Vector2.UnitX * (scrapIconTexture.Width + scrapTextOffset.X));
        currencyText.UpdatePosition(-Vector2.UnitY * scrapTextOffset.Y);

        waveIndicator = new UIEntity(game, uiElements, pixelsixFont, "Wave 0 of 0");
        waveIndicator.Scale = Vector2.One * 2;
        var waveTextWidth = pixelsixFont.MeasureString("Wave 99 of 99").X * waveIndicator.Scale.X;
        waveIndicator.SetPosition(new Vector2(game.NativeScreenWidth - waveTextWidth, 0));

        waveCooldownTimer = new UIEntity(game, uiElements, pixelsixFont, "Next wave in 00:00");
        waveCooldownTimer.Scale = Vector2.One * 2;
        var timerTextWidth = pixelsixFont.MeasureString("Next wave in 88:88").X * waveCooldownTimer.Scale.X;
        waveCooldownTimer.SetPosition(new Vector2(game.NativeScreenWidth - timerTextWidth,
            waveIndicator.Size.Y + 4));

        var gunTurretSprite = AssetManager.GetTexture("gunTurretBase");
        var turretTwoSprite = AssetManager.GetTexture("turretTwo");

        CreateTowerBuyButton<GunTurret>(gunTurretSprite, BuildingSystem.TowerType.GunTurret);
        CreateTowerBuyButton<Railgun>(turretTwoSprite, BuildingSystem.TowerType.Railgun);
        CreateTowerBuyButton<Drone>(turretTwoSprite, BuildingSystem.TowerType.Drone);
        CreateTowerBuyButton<Crane>(turretTwoSprite, BuildingSystem.TowerType.Crane);
        CreateTowerBuyButton<Mortar>(gunTurretSprite, BuildingSystem.TowerType.Mortar);
        CreateTowerBuyButton<Hovership>(turretTwoSprite, BuildingSystem.TowerType.Hovership);
        CreateTowerBuyButton<PunchTrap>(turretTwoSprite, BuildingSystem.TowerType.PunchTrap);

        var pauseIconTexture = AssetManager.GetTexture("btn_pause");
        var pauseButtonAnimation = new AnimationSystem.AnimationData
        (
            texture: pauseIconTexture,
            frameCount: 2,
            frameSize: new Vector2(pauseIconTexture.Width / 2, pauseIconTexture.Height),
            delaySeconds: 0.5f
        );

        var pauseButton = new UIEntity(game, uiElements, Vector2.Zero, pauseButtonAnimation);
        pauseButton.ButtonPressed += () => TogglePauseMenu(!isPauseMenuVisible);

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

            turretHologram.SetPosition(mouseSnappedScreenPos);
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

        if (WaveSystem.WaveCooldownLeft > 0)
        {
            waveCooldownTimer.Text = $"Next wave in {WaveSystem.WaveCooldownLeft.ToString("#.##")}";
        }
        else
        {
            waveCooldownTimer.Text = "";
        }

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

        if (turretHologram is not null)
        {
            LineUtility.DrawCircle(game.SpriteBatch, turretHologram.Position + turretHologram.Size / 2,
                selectedTurretRange, Color.White, resolution: 24);
        }

        base.Draw(gameTime);
    }

    private void RemoveTurretHologram()
    {
        if (turretHologram is not null)
        {
            turretHologram.Destroy();
            turretHologram = null;
            selectedTurretRange = default;
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
        selectedTurretRange = T.GetBaseRange() * Grid.TileLength;
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

        var resumeButtonText = new UIEntity(game, uiElements, pixelsixFont, "Resume");
        var exitButtonText = new UIEntity(game, uiElements, pixelsixFont, "Exit");
        resumeButtonText.SetPosition(playButtonPos + resumeButton.Size / 2 - resumeButtonText.Size / 2);
        exitButtonText.SetPosition(exitButtonPos + exitButton.Size / 2 - exitButtonText.Size / 2);
        resumeButtonText.DrawLayerDepth = 0.8f;
        exitButtonText.DrawLayerDepth = 0.8f;

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

        var retryButtonText = new UIEntity(game, uiElements, pixelsixFont, "Retry");
        var exitButtonText = new UIEntity(game, uiElements, pixelsixFont, "Exit");
        retryButtonText.SetPosition(retryButtonPos + retryButton.Size / 2 - retryButtonText.Size / 2);
        exitButtonText.SetPosition(quitButtonPos + quitButton.Size / 2 - exitButtonText.Size / 2);

        loseScreenElements.Add(retryButton);
        loseScreenElements.Add(quitButton);
        loseScreenElements.Add(retryButtonText);
        loseScreenElements.Add(exitButtonText);
    }

    private void ShowLevelWinScreen(int zone = -1, int wonLevel = -1)
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
            var nextLevelButtonText = new UIEntity(game, uiElements, pixelsixFont, "Next Level");
            nextLevelButtonText.SetPosition(nextLevelButtonPos + nextLevelButton.Size / 2 - nextLevelButtonText.Size / 2);

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

        var exitButtonText = new UIEntity(game, uiElements, pixelsixFont, "Exit");
        exitButtonText.SetPosition(quitButtonPos + quitButton.Size / 2 - exitButtonText.Size / 2);

        winScreenElements.Add(quitButton);
        winScreenElements.Add(exitButtonText);
    }

    private void ShowWaveCooldownSkipButton()
    {
        if (waveCooldownSkipButton is not null) return;

        var pos = new Vector2(game.NativeScreenWidth - buttonFrameSize.X - 4, 50);
        waveCooldownSkipButton = new UIEntity(game, uiElements, pos, buttonAnimationData);
        waveCooldownSkipButton.ButtonPressed += () => WaveSystem.SkipWaveCooldown();

        waveCooldownSkipText = new UIEntity(game, uiElements, pixelsixFont, "Skip");
        var skipTextSize = pixelsixFont.MeasureString("Skip");
        waveCooldownSkipText.SetPosition(waveCooldownSkipButton.Position + waveCooldownSkipButton.Size / 2
            - skipTextSize / 2);
    }

    private void HideWaveCooldownSkipButton()
    {
        if (waveCooldownSkipButton is null) return;

        waveCooldownSkipButton.Destroy();
        waveCooldownSkipText.Destroy();
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
