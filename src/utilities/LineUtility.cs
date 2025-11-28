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

    public static void DrawCircle(SpriteBatch spriteBatch, Vector2 point, float radius, Color color, float thickness = 1f, int resolution = 12)
    {
        for (int i = 0; i < resolution; i++)
        {
            var angleRad1 = MathHelper.TwoPi / resolution * i;
            var angleRad2 = MathHelper.TwoPi / resolution * (i + 1);
            var xNormal1 = MathF.Sin(angleRad1);
            var yNormal1 = MathF.Cos(angleRad1);
            var xNormal2 = MathF.Sin(angleRad2);
            var yNormal2 = MathF.Cos(angleRad2);
            var x1 = xNormal1 * radius;
            var y1 = yNormal1 * radius;
            var x2 = xNormal2 * radius;
            var y2 = yNormal2 * radius;

            var point1 = point + new Vector2(x1, y1);
            var point2 = point + new Vector2(x2, y2);

            DrawLine(spriteBatch, point1, point2, color, thickness);
        }
    }
}
