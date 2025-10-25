using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public static class TextureUtility
{
    public static Texture2D GetBlankTexture(SpriteBatch spriteBatch, int width, int height, Color color)
    {
        var tex = new Texture2D(spriteBatch.GraphicsDevice, width, height,
                mipmap: false, SurfaceFormat.Color);

        var colorData = new Color[width * height];

        for (int i = 0; i < width * height; i++)
        {
            colorData[i] = color;
        }

        tex.SetData(colorData);

        return tex;
    }
}
