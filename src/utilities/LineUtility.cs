using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

// Source for the majority of this line drawing system:
// https://community.monogame.net/t/line-drawing/6962/5
#nullable enable
public static class LineUtility
{
    private static Texture2D? pixelTexture = null;

    private static Texture2D GetPixelTexture(SpriteBatch spriteBatch)
    {
        if (pixelTexture is null)
        {
            pixelTexture = TextureUtility.GetBlankTexture(spriteBatch, 1, 1, Color.White);
        }

        return pixelTexture;
    }

    public static void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
    {
        var distance = Vector2.Distance(point1, point2);
        var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
        DrawLine(spriteBatch, point1, distance, angle, color, thickness);
    }

    public static void DrawLine(SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
    {
        var origin = new Vector2(0f, 0.5f);
        var scale = new Vector2(length, thickness);
        spriteBatch.Draw(GetPixelTexture(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, 0);
    }
}
