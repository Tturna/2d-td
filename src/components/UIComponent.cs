using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class UIComponent : DrawableGameComponent
{
    Game1 game;

    private List<UIEntity> uiElements = new();
    private UIEntity turretHologram;

    public UIComponent(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        var slotSprite = AssetManager.GetTexture("slot");
        var turretOneSprite = AssetManager.GetTexture("turret");
        var turretTwoSprite = AssetManager.GetTexture("turretTwo");

        var gunTurretIcon = new UIEntity(game, turretOneSprite);
        var railgunIcon = new UIEntity(game, turretTwoSprite);
        var gunTurretButton = new UIEntity(game, slotSprite);
        var railgunButton = new UIEntity(game, slotSprite);

        gunTurretButton.ButtonPressed += () => SelectTurret(BuildingSystem.TurretType.GunTurret);
        railgunButton.ButtonPressed += () => SelectTurret(BuildingSystem.TurretType.Railgun);

        var xPos = slotSprite.Width / 2 + 20;
        var yPos = game.Graphics.PreferredBackBufferHeight - slotSprite.Height / 2 - 20;
        var pos = new Vector2(xPos, yPos);

        gunTurretButton.Position = pos;
        gunTurretIcon.Position = pos;
        railgunButton.Position = pos + Vector2.UnitX * (slotSprite.Width + 20);
        railgunIcon.Position = pos + Vector2.UnitX * (slotSprite.Width + 20);

        game.Components.Add(gunTurretButton);
        game.Components.Add(railgunButton);
        game.Components.Add(gunTurretIcon);
        game.Components.Add(railgunIcon);

        uiElements.Add(gunTurretButton);
        uiElements.Add(railgunButton);
        uiElements.Add(gunTurretIcon);
        uiElements.Add(railgunIcon);

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
            var mouseWorldPos = InputSystem.GetMousePosition();
            var mouseWorldGridPos = Grid.SnapPositionToGrid(mouseWorldPos);
            var mouseSnappedScreenPos = Camera.RealPosToScreenPos(mouseWorldGridPos);
            var halfSpriteSize = new Vector2(turretHologram.Sprite.Width / 2, turretHologram.Sprite.Height / 2);
            turretHologram.Position = mouseSnappedScreenPos + halfSpriteSize;
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
            uiElement.DrawCentered();
        }

        base.Draw(gameTime);
    }

    private void CreateTurretHologram(Texture2D sprite)
    {
        if (turretHologram is not null)
        {
            uiElements.Remove(turretHologram);
        }

        turretHologram = new UIEntity(game, sprite);
        uiElements.Add(turretHologram);

        // No need to add this to game components because it shouldn't collide with anything ever.
    }

    private void SelectTurret(BuildingSystem.TurretType turretType)
    {
        var turretSprite = BuildingSystem.SelectTurret(turretType);
        CreateTurretHologram(turretSprite);
    }
}
