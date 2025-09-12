using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class TurretDetailsPrompt : UIEntity
{
    private UIEntity sellBtn;
    private UIEntity leftUpgradeBtn;
    private UIEntity rightUpgradeBtn;
    private Entity targetTurret;
    private Vector2 upgradeBgSpriteSize, buttonSpriteSize;

    public TurretDetailsPrompt(Game game, Entity turret, Action upgradeLeftCallback,
        Action upgradeRightCallback) : base(game, AssetManager.GetTexture("upgradebg"))
    {
        targetTurret = turret;
        var upgradeBgSprite = AssetManager.GetTexture("upgradebg");
        var buttonSprite = AssetManager.GetTexture("btn_square");

        upgradeBgSpriteSize = new Vector2(upgradeBgSprite.Bounds.Width, upgradeBgSprite.Bounds.Height);
        buttonSpriteSize = new Vector2(buttonSprite.Bounds.Width, buttonSprite.Bounds.Height);

        var buttonAnimationData = new AnimationSystem.AnimationData
        (
            texture: buttonSprite,
            frameCount: 2,
            frameSize: new Vector2(buttonSpriteSize.X / 2, buttonSpriteSize.Y),
            delaySeconds: 0.5f
        );

        sellBtn = new UIEntity(game, Vector2.Zero, buttonAnimationData);
        leftUpgradeBtn = new UIEntity(game, Vector2.Zero, buttonAnimationData);
        rightUpgradeBtn = new UIEntity(game, Vector2.Zero, buttonAnimationData);

        leftUpgradeBtn.DrawLayerDepth = 0.8f;
        rightUpgradeBtn.DrawLayerDepth = 0.8f;

        sellBtn.ButtonPressed += () =>
        {
            CurrencyManager.SellTower(BuildingSystem.GetTurretTypeFromEntity(targetTurret));
            targetTurret.Destroy();
        };

        leftUpgradeBtn.ButtonPressed += () => upgradeLeftCallback();
        rightUpgradeBtn.ButtonPressed += () => upgradeRightCallback();

        game.Components.Add(sellBtn);
        game.Components.Add(leftUpgradeBtn);
        game.Components.Add(rightUpgradeBtn);
    }

    public override void Update(GameTime gameTime)
    {
        var detailsPromptOffset = new Vector2(upgradeBgSpriteSize.X / 2 - targetTurret.Sprite.Bounds.Width / 2, 50);
        var sellBtnOffset = new Vector2(targetTurret.Sprite.Bounds.Width / 2 - 48, 40);

        var detailsOffsetPosition = targetTurret.Position - detailsPromptOffset;
        var sellBtnOffsetPosition = targetTurret.Position - sellBtnOffset;
        var detailsScreenPosition = Camera.WorldToScreenPosition(detailsOffsetPosition);
        var sellBtnScreenPosition = Camera.WorldToScreenPosition(sellBtnOffsetPosition);

        var baseButtonPosition = detailsOffsetPosition + upgradeBgSpriteSize / 2 - Vector2.UnitY * 26;
        var upgradeLeftBtnPosition = baseButtonPosition - Vector2.UnitX * (buttonSpriteSize.X / 2 + 3);
        var upgradeRightBtnPosition = baseButtonPosition + Vector2.UnitX * 3;
        var upgradeLeftBtnScreenPosition = Camera.WorldToScreenPosition(upgradeLeftBtnPosition);
        var upgradeRightBtnScreenPosition = Camera.WorldToScreenPosition(upgradeRightBtnPosition);

        Position = detailsScreenPosition;
        sellBtn.Position = sellBtnScreenPosition;
        leftUpgradeBtn.Position = upgradeLeftBtnScreenPosition;
        rightUpgradeBtn.Position = upgradeRightBtnScreenPosition;

        base.Update(gameTime);
    }

    public override void DrawCustom(GameTime gameTime)
    {
        sellBtn.DrawCustom(gameTime);
        leftUpgradeBtn.DrawCustom(gameTime);
        rightUpgradeBtn.DrawCustom(gameTime);

        base.DrawCustom(gameTime);
    }

    public override void Destroy()
    {
        var index = Game.Components.IndexOf(sellBtn);

        if (index >= 0)
        {
            Game.Components.RemoveAt(index);
        }

        base.Destroy();
    }

    public bool ShouldCloseDetailsView(Vector2 mouseScreenPosition)
    {
        if (Collision.IsPointInEntity(mouseScreenPosition, this)) return false;
        if (Collision.IsPointInEntity(mouseScreenPosition, sellBtn)) return false;

        // TODO: When there are upgrade buttons, don't close the details view when they're
        // clicked.

        return true;
    }
}
