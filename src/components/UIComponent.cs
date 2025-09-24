using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class UIComponent : DrawableGameComponent
{
    Game1 game;

    private List<UIEntity> uiElements = new();
    private UIEntity turretHologram;

    public static UIComponent Instance;

    public UIComponent(Game game) : base(game)
    {
        this.game = (Game1)game;
        Instance = this;
    }

    public override void Initialize()
    {
        var slotSprite = AssetManager.GetTexture("btn_square");

        // Turret buttons
        var gunTurretSprite = AssetManager.GetTexture("gunTurretBase");
        var turretTwoSprite = AssetManager.GetTexture("turretTwo");

        var gunTurretIcon = new UIEntity(game, uiElements, gunTurretSprite);
        var railgunIcon = new UIEntity(game, uiElements, turretTwoSprite);
        var droneIcon = new UIEntity(game, uiElements, turretTwoSprite);
        var craneIcon = new UIEntity(game, uiElements, turretTwoSprite);
        var mortarIcon = new UIEntity(game, uiElements, gunTurretSprite);

        var gunTurretButton = new UIEntity(game, uiElements, slotSprite);
        var railgunButton = new UIEntity(game, uiElements, slotSprite);
        var droneButton = new UIEntity(game, uiElements, slotSprite);
        var craneButton = new UIEntity(game, uiElements, slotSprite);
        var mortarButton = new UIEntity(game, uiElements, slotSprite);

        gunTurretButton.ButtonPressed += () => SelectTurret<GunTurret>();
        railgunButton.ButtonPressed += () => SelectTurret<Railgun>();
        droneButton.ButtonPressed += () => SelectTurret<Drone>();
        craneButton.ButtonPressed += () => SelectTurret<Crane>();
        mortarButton.ButtonPressed += () => SelectTurret<Mortar>();

        const float Margin = 20;
        var xPos = Margin;
        var yPos = game.Graphics.PreferredBackBufferHeight - slotSprite.Height - Margin;
        var pos = new Vector2(xPos, yPos);

        var buttonCenter = pos + new Vector2(slotSprite.Width / 2, slotSprite.Height / 2);
        var iconPosition = buttonCenter - new Vector2(gunTurretIcon.Size.X / 2, gunTurretIcon.Size.Y / 2);

        gunTurretButton.Position = pos;
        railgunButton.Position = pos + Vector2.UnitX * (slotSprite.Width + Margin);
        droneButton.Position = pos + Vector2.UnitX * (slotSprite.Width + Margin) * 2;
        craneButton.Position = pos + Vector2.UnitX * (slotSprite.Width + Margin) * 3;
        mortarButton.Position = pos + Vector2.UnitX * (slotSprite.Width + Margin) * 4;

        gunTurretIcon.Position = iconPosition;
        railgunIcon.Position = iconPosition + Vector2.UnitX * (slotSprite.Width + Margin);
        droneIcon.Position = iconPosition + Vector2.UnitX * (slotSprite.Width + Margin) * 2;
        craneIcon.Position = iconPosition + Vector2.UnitX * (slotSprite.Width + Margin) * 3;
        mortarIcon.Position = iconPosition + Vector2.UnitX * (slotSprite.Width + Margin) * 4;

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

        var defaultFont = AssetManager.GetFont("default");
        game.SpriteBatch.DrawString(defaultFont, $"Scrap: {CurrencyManager.Balance}", Vector2.Zero, Color.White);
        game.SpriteBatch.DrawString(defaultFont, "10", new Vector2(24, 460), Color.White);
        game.SpriteBatch.DrawString(defaultFont, "25", new Vector2(68, 460), Color.White);
        game.SpriteBatch.DrawString(defaultFont, "20", new Vector2(156, 460), Color.White);

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

    private void CreateTurretHologram(Texture2D sprite)
    {
        RemoveTurretHologram();

        turretHologram = new UIEntity(game, uiElements, sprite);
    }

    private void SelectTurret<T>() where T : ITower
    {
        BuildingSystem.SelectTurret<T>();
        var turretSprite = T.GetTowerBaseSprite();
        CreateTurretHologram(turretSprite);
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
