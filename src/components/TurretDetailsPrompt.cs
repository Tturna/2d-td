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
        Func<TowerUpgradeNode> upgradeRightCallback, TowerUpgradeNode currentUpgrade) :
        base(game, position: null, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, AssetManager.GetTexture("upgradebg"))
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

        sellBtn = new UIEntity(game, UIComponent.Instance.AddUIEntity, 
            UIComponent.Instance.RemoveUIEntity, Vector2.Zero, buttonAnimationData);

        var turretType = BuildingSystem.GetTurretTypeFromEntity(targetTurret);

        sellBtn.ButtonPressed += () =>
        {
            CurrencyManager.SellTower(turretType);
            targetTurret.Destroy();
        };

        if (currentUpgrade.LeftChild is not null)
        {
            leftUpgradeBtn = new UIEntity(game, UIComponent.Instance.AddUIEntity,
                UIComponent.Instance.RemoveUIEntity, Vector2.Zero, buttonAnimationData);
            leftUpgradeBtn.DrawLayerDepth = 0.8f;
            leftUpgradeBtn.ButtonPressed += () => Upgrade(upgradeLeftCallback);
            leftUpgradePrice = currentUpgrade.LeftChild.Price;
        }
        else
        {
            leftUpgradeBtn = null;
        }

        if (currentUpgrade.RightChild is not null)
        {
            rightUpgradeBtn = new UIEntity(game, UIComponent.Instance.AddUIEntity,
                UIComponent.Instance.RemoveUIEntity, Vector2.Zero, buttonAnimationData);
            rightUpgradeBtn.DrawLayerDepth = 0.8f;
            rightUpgradeBtn.ButtonPressed += () => Upgrade(upgradeRightCallback);
            rightUpgradePrice = currentUpgrade.RightChild.Price;
        }
        else
        {
            rightUpgradeBtn = null;
        }
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
        sellBtn.Destroy();
        leftUpgradeBtn.Destroy();
        rightUpgradeBtn.Destroy();

        base.Destroy();
    }

    public bool ShouldCloseDetailsView(Vector2 mouseScreenPosition)
    {
        if (Collision.IsPointInEntity(mouseScreenPosition, this)) return false;
        if (Collision.IsPointInEntity(mouseScreenPosition, sellBtn)) return false;

        // The details view won't close when upgrade buttons are clicked because the
        // click also hits the background (this), which prevents closing.

        return true;
    }

    private void Upgrade(Func<TowerUpgradeNode> upgradeCallback)
    {
        var newUpgrade = upgradeCallback();

        if (newUpgrade.LeftChild is not null)
        {
            leftUpgradePrice = newUpgrade.LeftChild.Price;
        }
        else
        {
            leftUpgradeBtn.Destroy();
            leftUpgradeBtn = null;
        }

        if (newUpgrade.RightChild is not null)
        {
            rightUpgradePrice = newUpgrade.RightChild.Price;
        }
        else
        {
            rightUpgradeBtn.Destroy();
            rightUpgradeBtn = null;
        }
    }
}
