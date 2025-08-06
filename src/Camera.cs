using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public static class Camera
{
    private static Vector2 _position = new(0, 0);
    private static GraphicsDevice _graphics;
    private static Matrix _translation;

    // INITIALIZE FIRST!
    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        _graphics = graphicsDevice;
    }

    public static void SetPosition(Vector2 newPosition)
    {
        _position = newPosition;
    }

    public static Matrix CalculateTranslation()
    {
        int windowWidth = _graphics.Viewport.Width;
        int windowHeight = _graphics.Viewport.Width;
        var dx = (windowWidth / 2) - _position.X;
        var dy = (windowHeight / 2) - _position.Y;
        _translation = Matrix.CreateTranslation(dx, dy, 0f);
        return _translation;
    }

    // public static 
}