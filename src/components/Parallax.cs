using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Parallax : DrawableGameComponent
{
    Game1 game;
    // layer 1 - closest to the screen
    // layer 2 - farther away from the screen
    private Vector2 _layer1position = new(0, 0);
    private Vector2 _layer2position = new(0, 0);

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
        var cam = Camera.GetCameraPosition();
        var layer1rate = 0.1f;
        _layer1position.X = cam.X * layer1rate;
        var layer2rate = 0.9f;
        _layer2position.X = cam.X * layer2rate;

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }
}