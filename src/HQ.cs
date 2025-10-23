using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class HQ : Entity
{
    public HealthSystem HealthSystem;
    private int startingHealth = 50;
    public static HQ Instance;
    
    private static Texture2D GetHQSprite(SpriteBatch spriteBatch)
    {
        var texture = TextureUtility.GetBlankTexture(spriteBatch, 4 * Grid.TileLength, 4 * Grid.TileLength, Color.White);
        return texture;
    }

    public HQ(Game1 game, Vector2 position) : base(game, position: position, sprite: GetHQSprite(game.SpriteBatch))
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
}
