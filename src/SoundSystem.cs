// Example showing playing sound effects using the simplified audio 
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace _2d_td;

public static class SoundSystem
{
    // static GraphicsDeviceManager graphics;
    // static SpriteBatch spriteBatch;

    // static public void Initialize(Game1 game)
    // {
    //     graphics = game.Graphics;
    //     spriteBatch = game.SpriteBatch;
    // }
    static public SoundEffectInstance playSound(string name)
    {
        var sound = AssetManager.GetSound(name);
        var instance = sound.CreateInstance();
        instance.Play();
        return instance;
    }
}