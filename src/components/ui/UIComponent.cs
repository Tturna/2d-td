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
    private UIEntity turretHologram;
    private UIEntity currencyText;
    private bool isPauseMenuVisible;
    private bool escHeld;
    private SpriteFont defaultFont = AssetManager.GetFont("default");

    public static UIComponent Instance;

    public UIComponent(Game game) : base(game)
    {
        this.game = (Game1)game;
        Instance = this;
    }

    public override void Initialize()
    {
        var slotSprite = AssetManager.GetTexture("btn_square");
        var slotFrameSize = new Vector2(slotSprite.Bounds.Width / 2, slotSprite.Bounds.Height);

        var gunTurretSprite = AssetManager.GetTexture("gunTurretBase");
        var turretTwoSprite = AssetManager.GetTexture("turretTwo");

        var gunTurretIcon = new UIEntity(game, uiElements, gunTurretSprite);
        var railgunIcon = new UIEntity(game, uiElements, turretTwoSprite);
        var droneIcon = new UIEntity(game, uiElements, turretTwoSprite);
        var craneIcon = new UIEntity(game, uiElements, turretTwoSprite);
        var mortarIcon = new UIEntity(game, uiElements, gunTurretSprite);

        var slotAnimationData = new AnimationSystem.AnimationData
        (
            texture: slotSprite,
            frameCount: 2,
            frameSize: slotFrameSize,
            delaySeconds: 0.5f
        );

        var gunTurretButton = new UIEntity(game, uiElements, Vector2.Zero, slotAnimationData);
        var railgunButton = new UIEntity(game, uiElements, Vector2.Zero, slotAnimationData);
        var droneButton = new UIEntity(game, uiElements, Vector2.Zero, slotAnimationData);
        var craneButton = new UIEntity(game, uiElements, Vector2.Zero, slotAnimationData);
        var mortarButton = new UIEntity(game, uiElements, Vector2.Zero, slotAnimationData);

        currencyText = new UIEntity(game, uiElements, defaultFont, $"Scrap: {CurrencyManager.Balance}");
        var gunTurretPriceText = new UIEntity(game, uiElements, defaultFont, CurrencyManager.GetTowerPrice(BuildingSystem.TowerType.GunTurret).ToString());
        var railgunPriceText = new UIEntity(game, uiElements, defaultFont, CurrencyManager.GetTowerPrice(BuildingSystem.TowerType.Railgun).ToString());
        var dronePriceText = new UIEntity(game, uiElements, defaultFont, CurrencyManager.GetTowerPrice(BuildingSystem.TowerType.Drone).ToString());
        var cranePriceText = new UIEntity(game, uiElements, defaultFont, CurrencyManager.GetTowerPrice(BuildingSystem.TowerType.Crane).ToString());
        var mortarPriceText = new UIEntity(game, uiElements, defaultFont, CurrencyManager.GetTowerPrice(BuildingSystem.TowerType.Mortar).ToString());

        gunTurretButton.ButtonPressed += () => SelectTurret<GunTurret>();
        railgunButton.ButtonPressed += () => SelectTurret<Railgun>();
        droneButton.ButtonPressed += () => SelectTurret<Drone>();
        craneButton.ButtonPressed += () => SelectTurret<Crane>();
        mortarButton.ButtonPressed += () => SelectTurret<Mortar>();

        const float Margin = 20;
        var xPos = Margin;
        var yPos = game.Graphics.PreferredBackBufferHeight - slotFrameSize.Y - Margin;
        var pos = new Vector2(xPos, yPos);

        var buttonCenter = pos + new Vector2(slotFrameSize.X / 2, slotFrameSize.Y / 2);
        var iconPosition = buttonCenter - new Vector2(gunTurretIcon.Size.X / 2, gunTurretIcon.Size.Y / 2);

        gunTurretButton.Position = pos;
        railgunButton.Position = pos + Vector2.UnitX * (slotFrameSize.X + Margin);
        droneButton.Position = pos + Vector2.UnitX * (slotFrameSize.X + Margin) * 2;
        craneButton.Position = pos + Vector2.UnitX * (slotFrameSize.X + Margin) * 3;
        mortarButton.Position = pos + Vector2.UnitX * (slotFrameSize.X + Margin) * 4;

        gunTurretIcon.Position = iconPosition;
        railgunIcon.Position = iconPosition + Vector2.UnitX * (slotFrameSize.X + Margin);
        droneIcon.Position = iconPosition + Vector2.UnitX * (slotFrameSize.X + Margin) * 2;
        craneIcon.Position = iconPosition + Vector2.UnitX * (slotFrameSize.X + Margin) * 3;
        mortarIcon.Position = iconPosition + Vector2.UnitX * (slotFrameSize.X + Margin) * 4;

        gunTurretIcon.DrawLayerDepth = 0.7f;
        railgunIcon.DrawLayerDepth = 0.7f;
        droneIcon.DrawLayerDepth = 0.7f;
        craneIcon.DrawLayerDepth = 0.7f;
        mortarIcon.DrawLayerDepth = 0.7f;

        currencyText.Position = Vector2.Zero;
        gunTurretPriceText.Position = gunTurretButton.Position + Vector2.UnitY * gunTurretButton.Size.Y;
        railgunPriceText.Position = railgunButton.Position + Vector2.UnitY * railgunButton.Size.Y;
        dronePriceText.Position = droneButton.Position + Vector2.UnitY * droneButton.Size.Y;
        cranePriceText.Position = craneButton.Position + Vector2.UnitY * craneButton.Size.Y;
        mortarPriceText.Position = mortarButton.Position + Vector2.UnitY * mortarButton.Size.Y;

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

        currencyText.Text = $"Scrap: {CurrencyManager.Balance}";

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
        var turretAnimationData = T.GetTowerAnimationData();
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
            return;
        }

        var buttonSprite = AssetManager.GetTexture("btn_square");
        var halfScreenWidth = game.NativeScreenWidth / 2;
        var halfScreenHeight = game.NativeScreenHeight / 2;
        var playBtnFrameSize = new Vector2(buttonSprite.Bounds.Width / 2, buttonSprite.Bounds.Height);

        var playBtnAnimationData = new AnimationSystem.AnimationData
        (
            texture: buttonSprite,
            frameCount: 2,
            frameSize: playBtnFrameSize,
            delaySeconds: 0.5f
        );

        var playButtonPos = new Vector2(halfScreenWidth - playBtnFrameSize.X / 2, halfScreenHeight - playBtnFrameSize.Y / 2);
        var resumeButton = new UIEntity(game, uiElements, playButtonPos, playBtnAnimationData);

        var exitButtonPos = new Vector2(halfScreenWidth - playBtnFrameSize.X / 2, halfScreenHeight + playBtnFrameSize.Y / 2 + 10);
        var exitButton = new UIEntity(game, uiElements, exitButtonPos, playBtnAnimationData);

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

    public void AddUIEntity(UIEntity entity)
    {
        uiElements.Add(entity);
    }

    public bool RemoveUIEntity(UIEntity entity)
    {
        return uiElements.Remove(entity);
    }
}
