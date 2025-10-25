using Microsoft.Xna.Framework;

namespace _2d_td;

public static class Camera
{
    private static Game1 game;
    private static Matrix _translation;

    public static float Scale { get; set; } = 1f;
    public static Vector2 Position { get; set; } = new(0, 0);

    // INITIALIZE FIRST!
    public static void Initialize(Game1 game)
    {
        Camera.game = game;
    }

    public static Matrix CalculateTranslation()
    {
        int windowWidth = game.GraphicsDevice.Viewport.Width;
        int windowHeight = game.GraphicsDevice.Viewport.Height;
        var dx = (windowWidth / 2) - Position.X*Scale;
        var dy = (windowHeight / 2) - Position.Y*Scale;
        var scale = Matrix.CreateScale(Scale);
        _translation = Matrix.CreateTranslation(dx, dy, 0f);
        var result = scale * _translation;
        return result;
    }

    public static Vector2 ScreenToWorldPosition(Vector2 pos)
    {
        int gameViewWidth = (int)game.NativeScreenWidth;
        int gameViewHeight = (int)game.NativeScreenHeight;

        Vector2 viewportCenter = new Vector2(gameViewWidth / 2f, gameViewHeight / 2f);
        Vector2 relativeScreenPos = pos - viewportCenter;
        Vector2 scaledRelativePos = relativeScreenPos / Scale; 
        Vector2 worldPos = scaledRelativePos + Position;
        
        return worldPos;
    }

    public static Vector2 WorldToScreenPosition(Vector2 pos)
    {
        int gameViewWidth = (int)game.NativeScreenWidth;
        int gameViewHeight = (int)game.NativeScreenHeight;

        Vector2 relativeWorldPos = pos - Position;
        Vector2 scaledRelativePos = relativeWorldPos * Scale;
        Vector2 screenPos = scaledRelativePos + new Vector2(gameViewWidth / 2f, gameViewHeight / 2f);
        screenPos = Vector2.Round(screenPos);

        return screenPos;
    }
}
