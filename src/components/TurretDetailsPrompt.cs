using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public class TurretDetailsPrompt : UIEntity
{
    private UIEntity sellBtn;
    private UIEntity? leftUpgradeBtn;
    private UIEntity? rightUpgradeBtn;
    private UIEntity? leftInfoBtn;
    private UIEntity? rightInfoBtn;
    private UIEntity upgradeIndicator;
    private Entity targetTurret;
    private Vector2 upgradeBgSpriteSize, buttonSpriteSize, upgradeIndicatorSpriteSize;
    private int leftUpgradePrice, rightUpgradePrice;
    private SpriteFont defaultFont;
    private List<UIEntity> tooltipEntities = new();

    public TurretDetailsPrompt(Game game, Entity turret, Func<TowerUpgradeNode?> upgradeLeftCallback,
        Func<TowerUpgradeNode?> upgradeRightCallback, TowerUpgradeNode currentUpgrade) :
        base(game, position: null, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, AssetManager.GetTexture("upgradebg"))
    {
        targetTurret = turret;
        var upgradeBgSprite = AssetManager.GetTexture("upgradebg");
        var buttonSprite = AssetManager.GetTexture("btn_square");
        var upgradeIndicatorSprite = AssetManager.GetTexture("upgrade_indicator");
        var infoIconSprite = AssetManager.GetTexture("btn_info");
        defaultFont = AssetManager.GetFont("default");

        upgradeBgSpriteSize = new Vector2(upgradeBgSprite.Width, upgradeBgSprite.Height);
        buttonSpriteSize = new Vector2(buttonSprite.Width, buttonSprite.Height);
        upgradeIndicatorSpriteSize = new Vector2(upgradeIndicatorSprite.Width, upgradeIndicatorSprite.Height);

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
            leftUpgradeBtn = new UIEntity(game, position: null, UIComponent.Instance.AddUIEntity,
                UIComponent.Instance.RemoveUIEntity, currentUpgrade.LeftChild.UpgradeIcon!);
            leftUpgradeBtn.DrawLayerDepth = 0.8f;
            leftUpgradeBtn.ButtonPressed += () => Upgrade(upgradeLeftCallback);
            leftUpgradePrice = currentUpgrade.LeftChild.Price;

            leftInfoBtn = new UIEntity(game, position: null, UIComponent.Instance.AddUIEntity,
                UIComponent.Instance.RemoveUIEntity, infoIconSprite);

            leftInfoBtn.ButtonPressed += () => ToggleUpgradeTooltip(currentUpgrade.LeftChild);
        }
        else
        {
            leftUpgradeBtn = null;
        }

        if (currentUpgrade.RightChild is not null)
        {
            rightUpgradeBtn = new UIEntity(game, position: null, UIComponent.Instance.AddUIEntity,
                UIComponent.Instance.RemoveUIEntity, currentUpgrade.RightChild.UpgradeIcon!);
            rightUpgradeBtn.DrawLayerDepth = 0.8f;
            rightUpgradeBtn.ButtonPressed += () => Upgrade(upgradeRightCallback);
            rightUpgradePrice = currentUpgrade.RightChild.Price;

            rightInfoBtn = new UIEntity(game, position: null, UIComponent.Instance.AddUIEntity,
                UIComponent.Instance.RemoveUIEntity, infoIconSprite);

            rightInfoBtn.ButtonPressed += () => ToggleUpgradeTooltip(currentUpgrade.RightChild);
        }
        else
        {
            rightUpgradeBtn = null;
        }

        var upgradeIndicatorAnimation = new AnimationSystem.AnimationData
        (
            texture: upgradeIndicatorSprite,
            frameCount: 3,
            frameSize: new Vector2(upgradeIndicatorSprite.Width / 3, upgradeIndicatorSprite.Height),
            delaySeconds: float.PositiveInfinity
        );

        upgradeIndicator = new UIEntity(game, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, position: Vector2.Zero, upgradeIndicatorAnimation);

        if (currentUpgrade.Parent is not null)
        {
            upgradeIndicator.AnimationSystem!.NextFrame();

            if (currentUpgrade.Parent.Parent is not null)
            {
                upgradeIndicator.AnimationSystem.NextFrame();
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        var halfTurretWidth = targetTurret.AnimationSystem!.BaseAnimationData.FrameSize.X / 2;
        var detailsPromptOffset = new Vector2(upgradeBgSpriteSize.X / 2 - halfTurretWidth, 50);
        var sellBtnOffset = new Vector2(halfTurretWidth - 64, 40);
        var upgradeIndicatorOffset = new Vector2(upgradeBgSpriteSize.X / 2 - upgradeIndicatorSpriteSize.X / 6, 2);

        var detailsOffsetPosition = targetTurret.Position - detailsPromptOffset;
        var sellBtnOffsetPosition = targetTurret.Position - sellBtnOffset;
        var upgradeIndicatorOffsetPosition = detailsOffsetPosition + upgradeIndicatorOffset;

        var detailsScreenPosition = Camera.WorldToScreenPosition(detailsOffsetPosition);
        var sellBtnScreenPosition = Camera.WorldToScreenPosition(sellBtnOffsetPosition);
        var upgradeIndicatorScreenPosition = Camera.WorldToScreenPosition(upgradeIndicatorOffsetPosition);

        var baseButtonPosition = detailsOffsetPosition + upgradeBgSpriteSize / 2 - Vector2.UnitY * 26;
        var upgradeLeftBtnPosition = baseButtonPosition - Vector2.UnitX * (buttonSpriteSize.X / 2 + 3);
        var upgradeRightBtnPosition = baseButtonPosition + Vector2.UnitX * 3;
        var upgradeLeftBtnScreenPosition = Camera.WorldToScreenPosition(upgradeLeftBtnPosition);
        var upgradeRightBtnScreenPosition = Camera.WorldToScreenPosition(upgradeRightBtnPosition);

        Position = detailsScreenPosition;
        sellBtn.Position = sellBtnScreenPosition;
        upgradeIndicator.Position = upgradeIndicatorScreenPosition;

        if (leftUpgradeBtn is not null)
        {
            leftUpgradeBtn.Position = upgradeLeftBtnScreenPosition;
            leftInfoBtn!.Position = leftUpgradeBtn.Position - Vector2.UnitX * (leftInfoBtn.Size.X + 4);
        }

        if (rightUpgradeBtn is not null)
        {
            rightUpgradeBtn.Position = upgradeRightBtnScreenPosition;
            rightInfoBtn!.Position = rightUpgradeBtn.Position + Vector2.UnitX * (rightUpgradeBtn.Size.X + 4);
        }

        base.Update(gameTime);
    }

    public override void DrawCustom(GameTime gameTime)
    {
        // Draw upgrade prices directly. No need for UI entity state.
        if (leftUpgradeBtn is not null)
        {
            var pos = leftUpgradeBtn.Position - Vector2.UnitX * 24;
            Game.SpriteBatch.DrawString(defaultFont, leftUpgradePrice.ToString(), pos, Color.White);
        }

        if (rightUpgradeBtn is not null)
        {
            var pos = rightUpgradeBtn.Position + Vector2.UnitX * 24;
            Game.SpriteBatch.DrawString(defaultFont, rightUpgradePrice.ToString(), pos, Color.White);
        }

        base.DrawCustom(gameTime);
    }

    public override void Destroy()
    {
        sellBtn.Destroy();
        leftUpgradeBtn?.Destroy();
        rightUpgradeBtn?.Destroy();
        upgradeIndicator.Destroy();
        leftInfoBtn?.Destroy();
        rightInfoBtn?.Destroy();
        CloseUpgradeTooltip();

        base.Destroy();
    }

    public bool ShouldCloseDetailsView(Vector2 mouseScreenPosition)
    {
        if (Collision.IsPointInEntity(mouseScreenPosition, this)) return false;
        if (Collision.IsPointInEntity(mouseScreenPosition, sellBtn)) return false;
        if (leftInfoBtn is not null &&
            Collision.IsPointInEntity(mouseScreenPosition, leftInfoBtn)) return false;
        if (rightInfoBtn is not null &&
            Collision.IsPointInEntity(mouseScreenPosition, rightInfoBtn)) return false;

        // The details view won't close when upgrade buttons are clicked because the
        // click also hits the background (this), which prevents closing.

        return true;
    }

    private void Upgrade(Func<TowerUpgradeNode?> upgradeCallback)
    {
        var newUpgrade = upgradeCallback();

        if (newUpgrade is null) return;

        upgradeIndicator.AnimationSystem!.NextFrame();

        if (tooltipEntities.Count > 0) CloseUpgradeTooltip();

        if (newUpgrade.LeftChild is not null)
        {
            leftUpgradePrice = newUpgrade.LeftChild.Price;
            leftUpgradeBtn!.Sprite = newUpgrade.LeftChild.UpgradeIcon;
            leftInfoBtn!.ClearButtonHandlers();

            if (newUpgrade.LeftChild is not null)
            {
                leftInfoBtn.ButtonPressed += () => ToggleUpgradeTooltip(newUpgrade.LeftChild);
            }
        }
        else
        {
            leftUpgradeBtn?.Destroy();
            leftInfoBtn?.Destroy();
            leftUpgradeBtn = null;
            leftInfoBtn = null;
        }

        if (newUpgrade.RightChild is not null)
        {
            rightUpgradePrice = newUpgrade.RightChild.Price;
            rightUpgradeBtn!.Sprite = newUpgrade.RightChild.UpgradeIcon;
            rightInfoBtn!.ClearButtonHandlers();

            if (newUpgrade.RightChild is not null)
            {
                rightInfoBtn.ButtonPressed += () => ToggleUpgradeTooltip(newUpgrade.RightChild);
            }
        }
        else
        {
            rightUpgradeBtn?.Destroy();
            rightInfoBtn?.Destroy();
            rightUpgradeBtn = null;
            rightInfoBtn = null;
        }
    }

    private void CloseUpgradeTooltip()
    {
        foreach (var element in tooltipEntities)
        {
            element.Destroy();
        }

        tooltipEntities.Clear();
    }

    private void ToggleUpgradeTooltip(TowerUpgradeNode upgrade)
    {
        if (tooltipEntities.Count > 0)
        {
            CloseUpgradeTooltip();
            return;
        }

        const int Margin = 8;
        var nameWidth = (int)defaultFont.MeasureString(upgrade.Name).X;
        var titleLineWidth = upgrade.UpgradeIcon!.Width + nameWidth + Margin * 3;
        var maxWidth = titleLineWidth;
        var priceText = $"Price: {upgrade.Price.ToString()}";
        var maxHeight = upgrade.UpgradeIcon.Height + (int)defaultFont.MeasureString(priceText).Y
            + Margin * 3;

        if (upgrade.Description is not null)
        {
            var descriptionSize = defaultFont.MeasureString(upgrade.Description);
            var descriptionWidth = (int)descriptionSize.X + Margin * 2;
            maxWidth = MathHelper.Max(maxWidth, descriptionWidth);

            var descriptionHeight = (int)descriptionSize.Y + Margin;
            maxHeight += descriptionHeight;
        }

        var tooltipBgTexture = TextureUtility.GetBlankTexture(Game.SpriteBatch,
            maxWidth, maxHeight, new Color(17, 15, 43));
        var tooltipBgPos = InputSystem.GetMouseScreenPosition()
            + new Vector2(0, -maxHeight - 12);
        var tooltipBg = new UIEntity(Game, tooltipBgPos, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, tooltipBgTexture);
        tooltipBg.DrawLayerDepth = 0.91f;

        var iconPos = tooltipBgPos + Vector2.One * Margin;
        var icon = new UIEntity(Game, iconPos, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, upgrade.UpgradeIcon!);

        var name = new UIEntity(Game, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, defaultFont, upgrade.Name);
        name.Position = iconPos + Vector2.UnitX * (icon.Size.X + Margin);

        var price = new UIEntity(Game, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, defaultFont, priceText);
        price.Position = iconPos + new Vector2(0, icon.Size.Y + Margin);

        if (upgrade.Description is not null)
        {
            var description = new UIEntity(Game, UIComponent.Instance.AddUIEntity,
                UIComponent.Instance.RemoveUIEntity, defaultFont, upgrade.Description);
            description.Position = price.Position + new Vector2(0, price.Size.Y + Margin);
            tooltipEntities.Add(description);
        }

        tooltipEntities.Add(tooltipBg);
        tooltipEntities.Add(icon);
        tooltipEntities.Add(name);
        tooltipEntities.Add(price);
    }
}
