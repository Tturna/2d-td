using System.Reflection;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class HQ : Entity
{
    public HealthSystem HealthSystem;
    private int startingHealth = 100;
    public static HQ Instance;
    
    private static Texture2D GetHQSprite(SpriteBatch spriteBatch)
    {
        var texture = new Texture2D(spriteBatch.GraphicsDevice, width: 4*Grid.TileLength, height: 4*Grid.TileLength,
        mipmap: false, SurfaceFormat.Color);

        var colorData = new Color[Grid.TileLength * Grid.TileLength*16];

        for (var i = 0; i < colorData.Length; i++)
        {
            colorData[i] = Color.White;
        }

        texture.SetData(colorData);

        return texture;
    }


    public HQ(Game1 game, Vector2 position) : base(game, position: position,sprite: GetHQSprite(game.SpriteBatch))//size: new Vector2(4, 4) * Grid.TileLength
    {
        HealthSystem = new HealthSystem(this, startingHealth);

        /*HealthSystem.Died += (Entity HQ) =>
        {
            Console.WriteLine("Player lost 🥶");
        };

        HealthSystem.Damaged += (Entity HQ) =>
        {
            Console.WriteLine("HQ damaged");
        };*/

        Instance = this;
    }
    

}