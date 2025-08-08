using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Parallax : DrawableGameComponent
{
    Game1 game;
    // layer 1 - closest to the screen
    // layer 2 - farther away from the screen
    private Vector2 _layer1position = new(0, 300);
    private Vector2 _layer2position = new(0, 300);
    private Texture2D mockSprite = AssetManager.GetTexture("tree");
    private Texture2D mockSprite2 = AssetManager.GetTexture("mountain");

    public Parallax(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        var cam = Camera.Position;
        var layer1rate = 0.05f;
        _layer1position.X = cam.X * layer1rate;
        var layer2rate = 0.90f;
        _layer2position.X = cam.X * layer2rate;

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        for (var i = 1; i < 100; i++)
        {
            var dx = (i - 50) * 100;
            var newPos = new Vector2(_layer1position.X + dx, _layer1position.Y);
            game.SpriteBatch.Draw(mockSprite,
                newPos,
                sourceRectangle: null,
                Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: 0.99f);
        }

        for (var i = 1; i < 50; i++)
        {
            var dx = (i - 25) * 200;
            var newPos = new Vector2(_layer2position.X + dx, _layer2position.Y);
            game.SpriteBatch.Draw(mockSprite2,
                newPos,
                sourceRectangle: null,
                Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: 1.0f);
        }

        base.Draw(gameTime);
    }
}
