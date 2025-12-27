using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class HQ : Entity
{
    public HealthSystem HealthSystem;
    private int startingHealth = 50;
    public static HQ Instance;
    
    public HQ(Game1 game, Vector2 position) : base(game, position, GetHQSprite())
    {
        HealthSystem = new HealthSystem(this, startingHealth);

        // HealthSystem.Died += (hq) =>
        // {
        //     Console.WriteLine("Player lost 🥶");
        // };
        //

        HealthSystem.Damaged += (source, hq, amount) =>
        {
            SoundSystem.PlaySound("hqDamage");
        };

        Instance = this;
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        HealthSystem.UpdateHealthBarGraphics(deltaTime);

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        if (HealthSystem.CurrentHealth > 0)
        {
            HealthSystem.DrawHealthBar(Position + new Vector2(Size.X / 2, -4));
        }

        base.Draw(gameTime);
    }

    private static Texture2D GetHQSprite()
    {
        return AssetManager.GetTexture("hq");
    }
}
