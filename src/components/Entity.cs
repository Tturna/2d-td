using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Entity : DrawableGameComponent
{
    new protected Game1 Game;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float RotationRadians { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 DrawOrigin { get; set; } = Vector2.Zero;
    public float DrawLayerDepth { get; set; } = 0.9f;
    public Texture2D Sprite { get; set; }

    public Entity(Game game, Texture2D sprite) : base(game)
    {
        this.Game = (Game1)base.Game;
        Sprite = sprite;
        Size = new Vector2(sprite.Width, sprite.Height);
    }

    public Entity(Game game, Vector2 position, Texture2D sprite) : base(game)
    {
        this.Game = (Game1)base.Game;
        Sprite = sprite;
        Position = position;
        Size = new Vector2(sprite.Width, sprite.Height);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();
    }

    public override void Draw(GameTime gameTime)
    {
       Game.SpriteBatch.Draw(Sprite,
                Position,
                sourceRectangle: null,
                Color.White,
                rotation: RotationRadians,
                origin: DrawOrigin,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: DrawLayerDepth);

        base.Draw(gameTime);
    }
}
