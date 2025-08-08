using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public static class Camera
{
    public static float Scale { get; set; } = 1f;
    public static Vector2 Position { get; set; } = new(0, 0);
    private static GraphicsDevice _graphics;
    private static Matrix _translation;

    // INITIALIZE FIRST!
    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        _graphics = graphicsDevice;
    }

    public static Matrix CalculateTranslation()
    {
        int windowWidth = _graphics.Viewport.Width;
        int windowHeight = _graphics.Viewport.Height;
        var dx = (windowWidth / 2) - Position.X*Scale;
        var dy = (windowHeight / 2) - Position.Y*Scale;
        var scale = Matrix.CreateScale(Scale);
        _translation = Matrix.CreateTranslation(dx, dy, 0f);
        var result = scale * _translation;
        return result;
    }

    public static Vector2 ScreenToWorldPosition(Vector2 pos)
    {
        int windowWidth = _graphics.Viewport.Width;
        int windowHeight = _graphics.Viewport.Height;
        var newPos = pos;
        newPos.X += Position.X - windowWidth / 2;
        newPos.Y += Position.Y - windowHeight / 2;
        return newPos;
    }

    public static Vector2 WorldToScreenPosition(Vector2 pos)
    {
        int windowWidth = _graphics.Viewport.Width;
        int windowHeight = _graphics.Viewport.Height;
        var newPos = pos;
        newPos.X -= Position.X - windowWidth / 2;
        newPos.Y -= Position.Y - windowHeight / 2;
        return newPos;
    }
}
