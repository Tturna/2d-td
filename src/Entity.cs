using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Entity : DrawableGameComponent
{
    private Game1 game;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; }
    public Texture2D Sprite { get; set; }

    public Entity(Game game, Texture2D sprite) : base(game)
    {
        this.game = (Game1)Game;
        Sprite = sprite;
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
        game.SpriteBatch.Begin();
        game.SpriteBatch.Draw(Sprite, Position, Color.White);
        game.SpriteBatch.End();

        base.Draw(gameTime);
    }
}
