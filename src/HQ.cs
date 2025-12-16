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
        // HealthSystem.Damaged += (hq, amount) =>
        // {
        //     Console.WriteLine("HQ damaged");
        // };

        Instance = this;
    }

    private static Texture2D GetHQSprite()
    {
        return AssetManager.GetTexture("hq");
    }
}
