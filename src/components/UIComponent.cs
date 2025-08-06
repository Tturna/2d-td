using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class UIComponent : DrawableGameComponent
{
    Game1 game;

    private Texture2D slotSprite;
    private Texture2D turretOneSprite;
    private Texture2D turretTwoSprite;

    public UIComponent(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        slotSprite = AssetManager.GetTexture("slot");
        turretOneSprite = AssetManager.GetTexture("turret");
        turretTwoSprite = AssetManager.GetTexture("turretTwo");

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    // Don't override the component's Draw function to prevent it from being called automatically.
    // This is so that it can be called manually after everything else in Game1.
    // This avoids the main sprite batch from translating the UI when the camera moves.
    new public void Draw(GameTime gameTime)
    {
        var xPos = slotSprite.Width / 2 + 20;
        var yPos = game.Graphics.PreferredBackBufferHeight - slotSprite.Height / 2 - 20;
        var pos = new Vector2(xPos, yPos);
        var halfSlotSize = new Vector2(slotSprite.Width / 2, slotSprite.Height / 2);
        var halfTurretSize = new Vector2(turretOneSprite.Width / 2, turretOneSprite.Height / 2);

        game.SpriteBatch.Begin();
        game.SpriteBatch.Draw(slotSprite,
                pos,
                sourceRectangle: null,
                Color.White,
                rotation: 0f,
                origin: halfSlotSize,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: 0f);

        game.SpriteBatch.Draw(slotSprite,
                pos + Vector2.UnitX * (slotSprite.Width + 20),
                sourceRectangle: null,
                Color.White,
                rotation: 0f,
                origin: halfSlotSize,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: 0f);

        game.SpriteBatch.Draw(turretOneSprite,
                pos,
                sourceRectangle: null,
                Color.White,
                rotation: 0f,
                origin: halfTurretSize,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: 0f);

        game.SpriteBatch.Draw(turretTwoSprite,
                pos + Vector2.UnitX * (slotSprite.Width + 20),
                sourceRectangle: null,
                Color.White,
                rotation: 0f,
                origin: halfTurretSize,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: 0f);

        game.SpriteBatch.End();

        base.Draw(gameTime);
    }
}
