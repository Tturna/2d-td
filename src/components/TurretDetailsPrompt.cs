using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class TurretDetailsPrompt : UIEntity
{
    private UIEntity sellBtn;
    private UIEntity leftUpgradeBtn;
    private UIEntity rightUpgradeBtn;
    private Entity targetTurret;
    private Vector2 upgradeBgSpriteSize, buttonSpriteSize;
    private int leftUpgradePrice, rightUpgradePrice;
    private SpriteFont defaultFont;

    public TurretDetailsPrompt(Game game, Entity turret, Func<TowerUpgradeNode> upgradeLeftCallback,
        Func<TowerUpgradeNode> upgradeRightCallback, TowerUpgradeNode currentUpgrade) : base(game, AssetManager.GetTexture("upgradebg"))
    {
        targetTurret = turret;
        var upgradeBgSprite = AssetManager.GetTexture("upgradebg");
        var buttonSprite = AssetManager.GetTexture("btn_square");
        defaultFont = AssetManager.GetFont("default");

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

        var turretType = BuildingSystem.GetTurretTypeFromEntity(targetTurret);

        sellBtn.ButtonPressed += () =>
        {
            CurrencyManager.SellTower(turretType);
            targetTurret.Destroy();
        };

        leftUpgradeBtn.ButtonPressed += () => Upgrade(currentUpgrade, upgradeLeftCallback);
        rightUpgradeBtn.ButtonPressed += () => Upgrade(currentUpgrade, upgradeRightCallback);

        leftUpgradePrice = CurrencyManager.GetUpgradePrice(currentUpgrade.LeftChild.Name);
        rightUpgradePrice = CurrencyManager.GetUpgradePrice(currentUpgrade.RightChild.Name);

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

        if (leftUpgradeBtn is not null)
        {
            leftUpgradeBtn.Position = upgradeLeftBtnScreenPosition;
        }

        if (rightUpgradeBtn is not null)
        {
            rightUpgradeBtn.Position = upgradeRightBtnScreenPosition;
        }

        base.Update(gameTime);
    }

    public override void DrawCustom(GameTime gameTime)
    {
        sellBtn.DrawCustom(gameTime);

        if (leftUpgradeBtn is not null)
        {
            leftUpgradeBtn.DrawCustom(gameTime);
            Game.SpriteBatch.DrawString(defaultFont, leftUpgradePrice.ToString(), leftUpgradeBtn.Position, Color.White);
        }

        if (rightUpgradeBtn is not null)
        {
            rightUpgradeBtn.DrawCustom(gameTime);
            Game.SpriteBatch.DrawString(defaultFont, rightUpgradePrice.ToString(), rightUpgradeBtn.Position, Color.White);
        }

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

    private void Upgrade(TowerUpgradeNode currentUpgrade, Func<TowerUpgradeNode> callback)
    {
        currentUpgrade = callback();

        if (currentUpgrade.LeftChild is not null)
        {
            leftUpgradePrice = CurrencyManager.GetUpgradePrice(currentUpgrade.LeftChild.Name);
        }
        else
        {
            leftUpgradeBtn = null;
        }

        if (currentUpgrade.RightChild is not null)
        {
            rightUpgradePrice = CurrencyManager.GetUpgradePrice(currentUpgrade.RightChild.Name);
        }
        else
        {
            rightUpgradeBtn = null;
        }
    }
}
