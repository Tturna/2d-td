using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class MortarShell : Entity
{
    public delegate void DestroyedHandler(Vector2 previousVelocity);
    public event DestroyedHandler Destroyed;

    public PhysicsSystem physics;

    public MortarShell(Game1 game) : base(game, GetShellTexture(game.SpriteBatch))
    {
        physics = new PhysicsSystem(Game);
        Game.Components.Add(this);
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var oldVelocity = physics.Velocity;
        var collided = physics.UpdatePhysics(this, deltaTime);

        if (collided) DestroyShell(oldVelocity);

        base.Update(gameTime);
    }

    public void DestroyShell(Vector2 previousVelocity)
    {
        Destroyed?.Invoke(previousVelocity);
        Destroy();
    }

    private static Texture2D GetShellTexture(SpriteBatch spriteBatch)
    {
        var tex = new Texture2D(spriteBatch.GraphicsDevice, width: 4, height: 4,
                mipmap: false, SurfaceFormat.Color);

        var colorData = new Color[16];

        for (int i = 0; i < 16; i++)
        {
            colorData[i] = Color.White;
        }

        tex.SetData(colorData);

        return tex;
    }
}
