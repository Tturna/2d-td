using System;
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
    private Dictionary<Entity, UIEntity> mortarMissingTargetIndicators = new();
    private UIEntity mortarReticle;
    private List<KeyValuePair<UIEntity, Vector2>> towerHologramPieces;
    private UIEntity currencyText;
    private UIEntity waveIndicator;
    private UIEntity waveCooldownTimer;
    private UIEntity waveCooldownSkipButton;
    private SettingsScreen settingsScreen;
    private bool isPauseMenuVisible;
    private bool escHeld;
    private bool isWon;
    private bool isLost;
    private int buyButtonCount = 0;
    private Action<Vector2> drawSelectedTowerRangeFunction;
    private bool shouldUpdateMortarReticle;

    private static SpriteFont pixelsixFont = AssetManager.GetFont("pixelsix");
    private static Texture2D buttonSprite = AssetManager.GetTexture("btn_square_empty");
    private float halfScreenWidth = Game1.Instance.NativeScreenWidth / 2;
    private float halfScreenHeight = Game1.Instance.NativeScreenHeight / 2;
    private static Vector2 buttonFrameSize = new Vector2(buttonSprite.Bounds.Width, buttonSprite.Bounds.Height);
    private readonly Vector2 scrapTextOffset = new Vector2(3, -1);
    private bool drawTileHologram;
    private Tileset selectedTileset;

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

    private void CreateTileBuyButtons()
    {
        const float Gap = 16;
        const float Margin = 20;
        Vector2 priceIconOffset = new Vector2(3, 3);
        Vector2 priceTextOffset = new Vector2(2, -2);
        var xPos = game.NativeScreenWidth - buttonFrameSize.X - Margin*3;
        var yPos = game.NativeScreenHeight - buttonFrameSize.Y - Margin;
        var heavyPos = new Vector2(xPos, yPos);
        var lightPos = heavyPos - new Vector2(buttonFrameSize.X + Gap, 0);

        var priceIcon = AssetManager.GetTexture("icon_scrap_small");
        var heavyTileBuyButton = new UIEntity(game, uiElements, heavyPos, buttonAnimationData);
        var lightTileBuyButton = new UIEntity(game, uiElements, lightPos, buttonAnimationData);
        var heavyTilePriceIcon = new UIEntity(game, uiElements, heavyPos,priceIcon);
        var lightTilePriceIcon = new UIEntity(game, uiElements, lightPos, priceIcon);

        heavyTilePriceIcon.SetPosition(heavyTileBuyButton.Position + new Vector2(priceIconOffset.X,
            heavyTileBuyButton.Size.Y + priceIconOffset.Y));
        lightTilePriceIcon.SetPosition(lightTileBuyButton.Position + new Vector2(priceIconOffset.X,
            lightTileBuyButton.Size.Y + priceIconOffset.Y));
    
        var heavyTilePriceText = new UIEntity(game, uiElements, pixelsixFont, "5");
        heavyTilePriceText.SetPosition(heavyTilePriceIcon.Position
            + new Vector2(priceIcon.Width + priceTextOffset.X, priceTextOffset.Y));
        var lightTilePriceText = new UIEntity(game, uiElements, pixelsixFont, "1");
        lightTilePriceText.SetPosition(lightTilePriceIcon.Position
            + new Vector2(priceIcon.Width + priceTextOffset.X, priceTextOffset.Y));

        var heavyTileIcon = new UIEntity(game, uiElements, AssetManager.GetTexture("heavytilesingle"));
        var lightTileIcon = new UIEntity(game, uiElements, AssetManager.GetTexture("lighttilesingle"));

        heavyTileIcon.SetPosition(heavyTileBuyButton.Position + new Vector2(7, 8));
        lightTileIcon.SetPosition(lightTileBuyButton.Position + new Vector2(7, 8));

        heavyTileBuyButton.ButtonPressed += () => 
        {
            BuildingSystem.SelectTile(game.Terrain.GetPlayerHeavyTileset());
            drawTileHologram = true; 
            selectedTileset = game.Terrain.GetPlayerHeavyTileset(); 
            SoundSystem.PlaySound("menuClick");
        };
        lightTileBuyButton.ButtonPressed += () => 
        {
            BuildingSystem.SelectTile(game.Terrain.GetPlayerLightTileset());
            drawTileHologram = true; 
            selectedTileset = game.Terrain.GetPlayerLightTileset(); 
            SoundSystem.PlaySound("menuClick");
        };
    }
    private void CreateTowerBuyButton<T>(BuildingSystem.TowerType towerType) where T : ITower
    {
        const float Margin = 20;
        const float Gap = 16;
        const int towers = 7;
        Vector2 priceIconOffset = new Vector2(3, 3);
        Vector2 priceTextOffset = new Vector2(2, -2);

        var xPos = game.NativeScreenWidth / 2 - buttonFrameSize.X / 2
            + buttonFrameSize.X * buyButtonCount + Gap * buyButtonCount
            - (buttonFrameSize.X / 2) * (towers - 1) - (Gap / 2 * (towers - 1));
        var yPos = game.NativeScreenHeight - buttonFrameSize.Y - Margin;
        var pos = new Vector2(xPos, yPos);
        var buttonCenter = pos + new Vector2(buttonFrameSize.X / 2, buttonFrameSize.Y / 2);

        var priceIcon = AssetManager.GetTexture("icon_scrap_small");
        var towerButton = new UIEntity(game, uiElements, Vector2.Zero, buttonAnimationData);
        var towerPriceIcon = new UIEntity(game, uiElements, priceIcon);
        var towerPriceText = new UIEntity(game, uiElements, pixelsixFont, CurrencyManager.GetTowerPrice(towerType).ToString());
        towerButton.ButtonPressed += () =>
        {
            SelectTower<T>();
            SoundSystem.PlaySound("menuClick");
        };

        var towerPieces = T.GetUnupgradedPartIcons(uiElements);
        var baseIcon = towerPieces[0].Key;

        var iconPosition = buttonCenter - new Vector2(baseIcon.Size.X / 2, baseIcon.Size.Y / 2);

        for (int i = 0; i < towerPieces.Count; i++)
        {
            var piece = towerPieces[i];
            var pieceEntity = piece.Key;
            var offset = piece.Value;
            pieceEntity.SetPosition(iconPosition + offset);

            if (i == 0)
            {
                pieceEntity.DrawLayerDepth = 0.7f;
            }
        }

        towerButton.SetPosition(pos);
        towerPriceIcon.SetPosition(towerButton.Position + new Vector2(priceIconOffset.X,
            towerButton.Size.Y + priceIconOffset.Y));

        towerPriceText.SetPosition(towerPriceIcon.Position
            + new Vector2(priceIcon.Width + priceTextOffset.X, priceTextOffset.Y));

        buyButtonCount++;
    }

    public override void Initialize()
    {
        HQ.Instance.HealthSystem.Died += ShowGameOverScreen;
        WaveSystem.LevelWin += ShowLevelWinScreen;
        WaveSystem.WaveEnded += ShowWaveCooldownSkipButton;
        WaveSystem.WaveStarted += HideWaveCooldownSkipButton;
        Mortar.StartTargeting += OnMortarStartTargeting;
        Mortar.EndTargeting += OnMortarEndTargeting;
        Mortar.MissingTargeting += OnMortarMissingTargeting;
        Mortar.Destroyed += OnMortarDestroyed;
        CurrencyManager.CurrencyAdded += _ => currencyText.StretchImpact(new Vector2(1.5f, 1.5f), 0.2f);

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

        waveIndicator = new UIEntity(game, uiElements, pixelsixFont, "Zone 5, Level 5, Wave 0 of 0");
        waveIndicator.Scale = Vector2.One * 2;
        var waveTextWidth = pixelsixFont.MeasureString("Zone 5, Level 5, Wave 99 of 99").X * waveIndicator.Scale.X;
        waveIndicator.SetPosition(new Vector2(game.NativeScreenWidth - waveTextWidth, 0));

        waveCooldownTimer = new UIEntity(game, uiElements, pixelsixFont, "Next wave in 00:00");
        waveCooldownTimer.Scale = Vector2.One * 2;
        var timerTextWidth = pixelsixFont.MeasureString("Next wave in 88:88").X * waveCooldownTimer.Scale.X;
        waveCooldownTimer.SetPosition(new Vector2(game.NativeScreenWidth - timerTextWidth,
            waveIndicator.Size.Y + 4));

        CreateTowerBuyButton<GunTurret>(BuildingSystem.TowerType.GunTurret);
        CreateTowerBuyButton<Railgun>(BuildingSystem.TowerType.Railgun);
        CreateTowerBuyButton<Drone>(BuildingSystem.TowerType.Drone);
        CreateTowerBuyButton<Crane>(BuildingSystem.TowerType.Crane);
        CreateTowerBuyButton<Mortar>(BuildingSystem.TowerType.Mortar);
        CreateTowerBuyButton<Hovership>(BuildingSystem.TowerType.Hovership);
        CreateTowerBuyButton<PunchTrap>(BuildingSystem.TowerType.PunchTrap);

        CreateTileBuyButtons();

        var pauseButtonSprite = AssetManager.GetTexture("btn_pause");
        var pauseButton = new UIEntity(game, uiElements, Vector2.Zero, pauseButtonSprite);
        pauseButton.ButtonPressed += () => TogglePauseMenu(!isPauseMenuVisible);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        if (towerHologramPieces is not null && towerHologramPieces.Count > 0)
        {
            var mouseWorldPos = InputSystem.GetMouseWorldPosition();
            var mouseWorldGridPos = Grid.SnapPositionToGrid(mouseWorldPos);
            var mouseSnappedScreenPos = Camera.WorldToScreenPosition(mouseWorldGridPos);

            foreach (var (pieceEntity, offset) in towerHologramPieces)
            {
                pieceEntity.SetPosition(mouseSnappedScreenPos + offset);
                var size = Camera.Scale;
                pieceEntity.Scale = new Vector2(size, size);
            }
        }

        if (InputSystem.IsRightMouseButtonClicked())
        {
            RemoveTowerHologram();
            drawTileHologram = false;
            selectedTileset = null;
            BuildingSystem.DeselectTower();
            BuildingSystem.DeselectTile();
        }

        currencyText.Text = $"{CurrencyManager.Balance}";
        waveIndicator.Text = $"Zone {game.CurrentZone}, Level {game.CurrentLevel}, Wave {WaveSystem.CurrentWaveIndex + 1} of {WaveSystem.MaxWaveIndex}";

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

            if (settingsScreen is null)
            {
                TogglePauseMenu(!isPauseMenuVisible);
            }
            else
            {
                settingsScreen.Destroy();
                settingsScreen = null;
            }
        }

        if (kbdState.IsKeyUp(Keys.Escape)) escHeld = false;

        foreach (var mortarIndicatorPair in mortarMissingTargetIndicators)
        {
            var mortar = mortarIndicatorPair.Key;
            var indicator = mortarIndicatorPair.Value;
            var indicatorOffset = new Vector2(10, -8);
            var indicatorPos = Camera.WorldToScreenPosition(mortar.Position + mortar.Size / 2
                + indicatorOffset);

            indicator.SetPosition(indicatorPos);
        }

        if (mortarReticle is not null && shouldUpdateMortarReticle)
        {
            var mousePos = InputSystem.GetMouseScreenPosition();
            var reticlePos = mousePos - mortarReticle.Size / 2;
            mortarReticle.SetPosition(reticlePos);
        }

        if (game.IsPaused)
        {
            for (int i = uiElements.Count - 1; i >= 0; i--)
            {
                var element = uiElements[i];
                element.Update(gameTime);
            }
        }

        base.Update(gameTime);
    }

    // Don't override the component's Draw function to prevent it from being called automatically.
    // This is so that it can be called manually after everything else in Game1.
    // This avoids the main sprite batch from translating the UI when the camera moves.
    new public void Draw(GameTime gameTime)
    {
        // Call Draw() manually on each UI element so they don't move with the camera.
        // Game1 already creates a new sprite batch for this function call.
        if(drawTileHologram)
        {
            var Sprite = (selectedTileset == game.Terrain.GetPlayerHeavyTileset()) ? 
                AssetManager.GetTexture("heavytilesingle") : 
                AssetManager.GetTexture("lighttilesingle");

            var mouseWorldPos = InputSystem.GetMouseWorldPosition();
            var hologramWorldPosition = Grid.SnapPositionToGrid(mouseWorldPos);
            var hologramPosition = Camera.WorldToScreenPosition(hologramWorldPosition);

            game.SpriteBatch.Draw(Sprite,
                    hologramPosition,
                    sourceRectangle: null,
                    Color.White,
                    rotation: 0,
                    origin: default,
                    scale: Vector2.One,
                    effects: SpriteEffects.None,
                    layerDepth: 0.6f);
        }
        foreach (UIEntity uiElement in uiElements)
        {
            uiElement.DrawCustom(gameTime);
        }

        if (towerHologramPieces is not null && towerHologramPieces.Count > 0)
        {
            var basePiece = towerHologramPieces[0].Key;
            var worldPosition = Camera.ScreenToWorldPosition(basePiece.Position + basePiece.Size / 2);
            drawSelectedTowerRangeFunction(worldPosition);
        }

        base.Draw(gameTime);
    }

    private void RemoveTowerHologram()
    {
        if (towerHologramPieces is not null && towerHologramPieces.Count > 0)
        {
            foreach (var (pieceEntity, _) in towerHologramPieces)
            {
                pieceEntity.Destroy();
            }

            drawSelectedTowerRangeFunction = null;
            towerHologramPieces.Clear();
        }
    }

    private void CreateTowerHologram(List<KeyValuePair<UIEntity, Vector2>> towerPieceIcons)
    {
        RemoveTowerHologram();
        towerHologramPieces = towerPieceIcons;
    }

    private void SelectTower<T>() where T : ITower
    {
        BuildingSystem.SelectTower<T>();

        var towerPieceIcons = T.GetUnupgradedPartIcons(uiElements);
        CreateTowerHologram(towerPieceIcons);
        drawSelectedTowerRangeFunction = T.DrawBaseRangeIndicator;
    }

    private void TogglePauseMenu(bool isPauseMenuVisible)
    {
        SoundSystem.PlaySound("menuClick");
        game.SetPauseState(isPauseMenuVisible);
        this.isPauseMenuVisible = isPauseMenuVisible;

        if (!isPauseMenuVisible)
        {
            foreach (var element in pauseMenuElements)
            {
                element.Destroy();
            }

            settingsScreen?.Destroy();
            settingsScreen = null;
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

        var resumeButtonPos = new Vector2(16, halfScreenHeight);
        var resumeButton = new UIEntity(game, uiElements, resumeButtonPos, AssetManager.GetTexture("pause_resume_button"));

        var settingsButtonPos = new Vector2(16, halfScreenHeight + resumeButton.Size.Y + 4);
        var settingsButton = new UIEntity(game, uiElements, settingsButtonPos, AssetManager.GetTexture("main_settings_button"));

        var exitButtonPos = new Vector2(16, halfScreenHeight + resumeButton.Size.Y + settingsButton.Size.Y + 8);
        var exitButton = new UIEntity(game, uiElements, exitButtonPos, AssetManager.GetTexture("main_quit_button"));

        resumeButton.ButtonPressed += () => TogglePauseMenu(!isPauseMenuVisible);
        settingsButton.ButtonPressed += () =>
        {
            if (settingsScreen is null)
            {
                settingsScreen = new SettingsScreen(game, uiElements);

                foreach (var element in pauseMenuElements)
                {
                    element.Destroy();
                }

                pauseMenuElements.Clear();
                settingsScreen.OnDestroyed += () =>
                {
                    TogglePauseMenu(true);
                    settingsScreen = null;
                };
            }
        };

        exitButton.ButtonPressed += () =>
        {
            TogglePauseMenu(false);
            SceneManager.LoadMainMenu();
            SoundSystem.PlaySound("menuClick");
        };

        pauseMenuElements.Add(resumeButton);
        pauseMenuElements.Add(settingsButton);
        pauseMenuElements.Add(exitButton);
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

        var quitButtonSprite = AssetManager.GetTexture("main_quit_button");
        var quitButtonPos = new Vector2(halfScreenWidth - quitButtonSprite.Width / 2,
            halfScreenHeight + quitButtonSprite.Height / 2 + 10);
        var quitButton = new UIEntity(game, uiElements, quitButtonPos, quitButtonSprite);

        if (!beatTheGame)
        {
            var nextButtonSprite = AssetManager.GetTexture("nextlevel_button");
            var nextLevelButtonPos = new Vector2(halfScreenWidth - nextButtonSprite.Width / 2,
                halfScreenHeight - nextButtonSprite.Height / 2);
            var nextLevelButton = new UIEntity(game, uiElements, nextLevelButtonPos, nextButtonSprite);

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
        }

        quitButton.ButtonPressed += () => SceneManager.LoadMainMenu();

        winScreenElements.Add(quitButton);
    }

    private void ShowWaveCooldownSkipButton()
    {
        if (waveCooldownSkipButton is not null) return;

        var skipButtonSprite = AssetManager.GetTexture("skip_button");
        var pos = new Vector2(game.NativeScreenWidth - skipButtonSprite.Width - 4, 50);
        waveCooldownSkipButton = new UIEntity(game, uiElements, pos, skipButtonSprite);
        waveCooldownSkipButton.ButtonPressed += () =>
        {
            WaveSystem.SkipWaveCooldown();
            SoundSystem.PlaySound("menuClick");
        };
    }

    private void HideWaveCooldownSkipButton()
    {
        if (waveCooldownSkipButton is null) return;

        waveCooldownSkipButton.Destroy();
        waveCooldownSkipButton = null;
    }

    private void OnMortarStartTargeting(Entity mortar)
    {
        if (mortarMissingTargetIndicators.TryGetValue(mortar, out var indicator))
        {
            indicator.Destroy();
            mortarMissingTargetIndicators.Remove(mortar);
        }

        if (mortarReticle is not null) return;

        var reticleSprite = AssetManager.GetTexture("mortar_reticle");
        mortarReticle = new UIEntity(game, uiElements, reticleSprite);
        var mousePos = InputSystem.GetMouseScreenPosition();
        var reticlePos = mousePos - mortarReticle.Size / 2;
        mortarReticle.SetPosition(reticlePos);
        shouldUpdateMortarReticle = true;
    }

    private void OnMortarEndTargeting(Entity mortar)
    {
        if (mortarReticle is not null)
        {
            mortarReticle.Destroy();
            mortarReticle = null;
            shouldUpdateMortarReticle = false;
            SoundSystem.PlaySound("menuClick");
        }
    }

    private void OnMortarMissingTargeting(Entity mortar)
    {
        var missingTargetIndicator = new UIEntity(game, uiElements, pixelsixFont, "No target!");
        var indicatorOffset = new Vector2(10, -8);
        var indicatorPos = Camera.WorldToScreenPosition(mortar.Position + mortar.Size / 2 + indicatorOffset);
        missingTargetIndicator.SetPosition(indicatorPos);
        mortarMissingTargetIndicators[mortar] = missingTargetIndicator;
    }

    private void OnMortarDestroyed(Entity mortar)
    {
        if (mortarMissingTargetIndicators.TryGetValue(mortar, out var indicator))
        {
            indicator.Destroy();
            mortarMissingTargetIndicators.Remove(mortar);
        }
    }

    public static void SpawnFlyoutText(string text, Vector2 startPosition,
        Vector2 flyoutVelocity, float lifetime, Color color, bool slowdown = true)
    {
        new FlyoutText(Instance.game, Instance.uiElements, text, startPosition, flyoutVelocity,
            lifetime, color, slowdown);
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
