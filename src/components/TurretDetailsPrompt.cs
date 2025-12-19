using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public class TurretDetailsPrompt : UIEntity
{
    private UIEntity sellBtn;
    private string sellText;
    private UIEntity? repairBtn;
    private UIEntity? leftUpgradeBtn;
    private UIEntity? rightUpgradeBtn;
    private UIEntity? leftInfoBtn;
    private UIEntity? rightInfoBtn;
    private UIEntity upgradeIndicator;
    private Entity targetTowerEntity;
    private ITower targetTowerInterface;
    private Vector2 upgradeBgSpriteSize, buttonSpriteSize, upgradeIndicatorSpriteSize;
    private int leftUpgradePrice, rightUpgradePrice;
    private SpriteFont pixelsixFont;
    private List<UIEntity> tooltipEntities = new();
    private BuildingSystem.TowerType towerType;

    // TODO: consider if there's a better way to handle showing tower specific elements
    private bool isMortar;

    public TurretDetailsPrompt(Game game, Entity turret, TowerCore core,
        Func<TowerUpgradeNode?> upgradeLeftCallback, Func<TowerUpgradeNode?> upgradeRightCallback,
        TowerUpgradeNode currentUpgrade) : base(game, position: null, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, AssetManager.GetTexture("upgradebg"))
    {
        targetTowerEntity = turret;
        targetTowerInterface = (ITower)turret;
        var upgradeBgSprite = AssetManager.GetTexture("upgradebg");
        var buttonSprite = AssetManager.GetTexture("btn_square");
        var upgradeIndicatorSprite = AssetManager.GetTexture("upgrade_indicator");
        var infoIconSprite = AssetManager.GetTexture("btn_info");
        pixelsixFont = AssetManager.GetFont("pixelsix");

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

        towerType = BuildingSystem.GetTowerTypeFromEntity(targetTowerEntity);
        sellBtn = new UIEntity(game, UIComponent.Instance.AddUIEntity, 
            UIComponent.Instance.RemoveUIEntity, Vector2.Zero, buttonAnimationData);
        sellText = $"Sell ({CurrencyManager.GetTowerPrice(towerType) / 2})";

        sellBtn.ButtonPressed += () =>
        {
            var returnScrap = CurrencyManager.SellTower(towerType, isBroken: core.Health.CurrentHealth <= 0);
            targetTowerEntity.Destroy();

            var indicatorPos = targetTowerEntity.Position - Vector2.UnitY * 6;

            var indicatorVelocity = -Vector2.UnitY * 25f;
            UIComponent.SpawnFlyoutText($"+{returnScrap}", indicatorPos, indicatorVelocity, 1f,
                color: Color.White);
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

        if (core.Health.CurrentHealth <= 0)
        {
            repairBtn = new UIEntity(game, UIComponent.Instance.AddUIEntity, 
                UIComponent.Instance.RemoveUIEntity, Vector2.Zero, buttonAnimationData);

            repairBtn.ButtonPressed += () =>
            {
                if (CurrencyManager.TryRepairTower(towerType))
                {
                    core.Health.ResetHealth();
                    var repairPrice = CurrencyManager.GetTowerPrice(towerType) / 2;
                    var indicatorPos = turret.Position;
                    var indicatorVelocity = -Vector2.UnitY * 25f;
                    UIComponent.SpawnFlyoutText($"-{repairPrice}", indicatorPos, indicatorVelocity, 1f,
                        color: Color.White);

                    repairBtn.Destroy();
                    repairBtn = null;
                }
            };
        }

        isMortar = turret is Mortar;
    }

    public override void Update(GameTime gameTime)
    {
        var halfTurretWidth = targetTowerEntity.AnimationSystem!.BaseAnimationData.FrameSize.X / 2;
        var detailsPromptOffset = new Vector2(upgradeBgSpriteSize.X / 2 - halfTurretWidth, 50);
        var sellBtnOffset = new Vector2(upgradeBgSpriteSize.X + 20, 4);
        var upgradeIndicatorOffset = new Vector2(upgradeBgSpriteSize.X / 2 - upgradeIndicatorSpriteSize.X / 6, 2);

        var detailsOffsetPosition = targetTowerEntity.Position - detailsPromptOffset;
        var sellBtnOffsetPosition = detailsOffsetPosition + sellBtnOffset;
        var upgradeIndicatorOffsetPosition = detailsOffsetPosition + upgradeIndicatorOffset;

        var detailsScreenPosition = Camera.WorldToScreenPosition(detailsOffsetPosition);
        var sellBtnScreenPosition = Camera.WorldToScreenPosition(sellBtnOffsetPosition);
        var upgradeIndicatorScreenPosition = Camera.WorldToScreenPosition(upgradeIndicatorOffsetPosition);

        var baseButtonPosition = detailsOffsetPosition + upgradeBgSpriteSize / 2 - Vector2.UnitY * 26;
        var upgradeLeftBtnPosition = baseButtonPosition - Vector2.UnitX * (buttonSpriteSize.X / 2 + 3);
        var upgradeRightBtnPosition = baseButtonPosition + Vector2.UnitX * 3;
        var upgradeLeftBtnScreenPosition = Camera.WorldToScreenPosition(upgradeLeftBtnPosition);
        var upgradeRightBtnScreenPosition = Camera.WorldToScreenPosition(upgradeRightBtnPosition);

        SetPosition(detailsScreenPosition);
        sellBtn.SetPosition(sellBtnScreenPosition);
        upgradeIndicator.SetPosition(upgradeIndicatorScreenPosition);

        const int InfoButtonMargin = 5;

        if (leftUpgradeBtn is not null)
        {
            leftUpgradeBtn.SetPosition(upgradeLeftBtnScreenPosition);
            leftInfoBtn!.SetPosition(leftUpgradeBtn.Position - Vector2.UnitX * (leftInfoBtn.Size.X + InfoButtonMargin));
        }

        if (rightUpgradeBtn is not null)
        {
            rightUpgradeBtn.SetPosition(upgradeRightBtnScreenPosition);
            rightInfoBtn!.SetPosition(rightUpgradeBtn.Position + Vector2.UnitX * (rightUpgradeBtn.Size.X + InfoButtonMargin));
        }

        if (repairBtn is not null)
        {
            repairBtn.SetPosition(sellBtnScreenPosition + Vector2.UnitY * 40);
        }

        base.Update(gameTime);
    }

    public override void DrawCustom(GameTime gameTime)
    {
        const int PriceMargin = 5;
        const int PriceYOffset = 10;

        // Draw upgrade prices directly. No need for UI entity state.
        if (leftUpgradeBtn is not null)
        {
            var priceWidth = pixelsixFont.MeasureString(leftUpgradePrice.ToString()).X;
            var pos = leftUpgradeBtn.Position + new Vector2(-priceWidth - PriceMargin, PriceYOffset);
            Game.SpriteBatch.DrawString(pixelsixFont, leftUpgradePrice.ToString(), pos, Color.White);
        }

        if (rightUpgradeBtn is not null)
        {
            var upgradeBtnWidth = rightUpgradeBtn.Size.X;
            var pos = rightUpgradeBtn.Position + new Vector2(upgradeBtnWidth + PriceMargin, PriceYOffset);
            Game.SpriteBatch.DrawString(pixelsixFont, rightUpgradePrice.ToString(), pos, Color.White);
        }

        var sellTextSize = pixelsixFont.MeasureString(sellText);
        var sellTextPos = sellBtn.Position + sellBtn.Size / 2 - sellTextSize / 2;
        Game.SpriteBatch.DrawString(pixelsixFont, sellText, sellTextPos, Color.White);

        if (repairBtn is not null)
        {
            var repairPrice = CurrencyManager.GetTowerPrice(towerType) / 2;
            var repairText = $"Repair ({repairPrice})";
            var textSize = pixelsixFont.MeasureString(repairText);
            var buttonCenter = repairBtn.Position + repairBtn.Size / 2;
            var textPosition = buttonCenter - textSize / 2;
            Game.SpriteBatch.DrawString(pixelsixFont, repairText, textPosition, Color.White);
        }

        targetTowerInterface.DrawRangeIndicator();

        if (isMortar)
        {
            var reticleSprite = AssetManager.GetTexture("mortar_reticle");
            var spriteSize = new Vector2(reticleSprite.Width, reticleSprite.Height);
            var reticleWorldPos = ((Mortar)targetTowerEntity).TargetHitpoint - spriteSize / 2;
            var reticlePos = Camera.WorldToScreenPosition(reticleWorldPos);

            Game.SpriteBatch.Draw(reticleSprite, reticlePos, Color.White);
        }

        base.DrawCustom(gameTime);
    }

    public override void Destroy()
    {
        sellBtn.Destroy();
        repairBtn?.Destroy();
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
        if (Collision.IsPointInEntity(Camera.ScreenToWorldPosition(mouseScreenPosition),
            targetTowerEntity))
        {
            return true;
        }

        if (Collision.IsPointInEntity(mouseScreenPosition, this)) return false;
        if (Collision.IsPointInEntity(mouseScreenPosition, sellBtn)) return false;
        if (repairBtn is not null &&
            Collision.IsPointInEntity(mouseScreenPosition, repairBtn)) return false;
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
            leftInfoBtn.ButtonPressed += () => ToggleUpgradeTooltip(newUpgrade.LeftChild);
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
            rightInfoBtn.ButtonPressed += () => ToggleUpgradeTooltip(newUpgrade.RightChild);
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
        var nameWidth = (int)pixelsixFont.MeasureString(upgrade.Name).X;
        var titleLineWidth = upgrade.UpgradeIcon!.Width + nameWidth + Margin * 3;
        var maxWidth = titleLineWidth;
        var priceText = $"Price: {upgrade.Price.ToString()}";
        var maxHeight = upgrade.UpgradeIcon.Height + (int)pixelsixFont.MeasureString(priceText).Y
            + Margin * 3;

        if (upgrade.Description is not null)
        {
            var descriptionSize = pixelsixFont.MeasureString(upgrade.Description);
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
            UIComponent.Instance.RemoveUIEntity, pixelsixFont, upgrade.Name);
        name.SetPosition(iconPos + Vector2.UnitX * (icon.Size.X + Margin));

        var price = new UIEntity(Game, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, pixelsixFont, priceText);
        price.SetPosition(iconPos + new Vector2(0, icon.Size.Y + Margin));

        if (upgrade.Description is not null)
        {
            var description = new UIEntity(Game, UIComponent.Instance.AddUIEntity,
                UIComponent.Instance.RemoveUIEntity, pixelsixFont, upgrade.Description);
            description.SetPosition(price.Position + new Vector2(0, price.Size.Y + Margin));
            tooltipEntities.Add(description);
        }

        tooltipEntities.Add(tooltipBg);
        tooltipEntities.Add(icon);
        tooltipEntities.Add(name);
        tooltipEntities.Add(price);
    }
}
