using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class TurretDetailsPrompt : UIEntity
{
    private UIEntity sellBtn;
    private Entity targetTurret;
    private Vector2 upgradeBgSpriteSize, sellBtnSpriteSize;

    public TurretDetailsPrompt(Game game, Entity turret) : base(game, AssetManager.GetTexture("upgradebg"))
    {
        targetTurret = turret;
        var upgradeBgSprite = AssetManager.GetTexture("upgradebg");
        var sellButtonSprite = AssetManager.GetTexture("btn_square");

        upgradeBgSpriteSize = new Vector2(upgradeBgSprite.Bounds.Width, upgradeBgSprite.Bounds.Height);
        sellBtnSpriteSize = new Vector2(sellButtonSprite.Bounds.Width, sellButtonSprite.Bounds.Height);

        var sellBtnAnimationData = new AnimationSystem.AnimationData
        (
            texture: sellButtonSprite,
            frameCount: 2,
            frameSize: new Vector2(sellBtnSpriteSize.X / 2, sellBtnSpriteSize.Y),
            delaySeconds: 0.5f
        );

        sellBtn = new UIEntity(game, Vector2.Zero, sellBtnAnimationData);
        sellBtn.ButtonPressed += () =>
        {
            CurrencyManager.SellTower(BuildingSystem.GetTurretTypeFromEntity(targetTurret));
            targetTurret.Destroy();
        };

        game.Components.Add(sellBtn);
    }

    public override void Update(GameTime gameTime)
    {
        var detailsPromptOffset = new Vector2(upgradeBgSpriteSize.X / 2 - targetTurret.Sprite.Bounds.Width / 2, 50);
        var sellBtnOffset = new Vector2(targetTurret.Sprite.Bounds.Width / 2 - 48, 40);

        var detailsOffsetPosition = targetTurret.Position - detailsPromptOffset;
        var sellBtnOffsetPosition = targetTurret.Position - sellBtnOffset;
        var detailsScreenPosition = Camera.WorldToScreenPosition(detailsOffsetPosition);
        var sellBtnScreenPosition = Camera.WorldToScreenPosition(sellBtnOffsetPosition);

        Position = detailsScreenPosition;
        sellBtn.Position = sellBtnScreenPosition;

        base.Update(gameTime);
    }

    public override void DrawCustom(GameTime gameTime)
    {
        sellBtn.DrawCustom(gameTime);

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
