using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class FpsUtility
{
    private double frames = 0;
    private double updates = 0;
    private double elapsed = 0;
    private double last = 0;
    private double now = 0;
    private double msgFrequency = 1.0; 
    private string msg = "";
    private SpriteFont pixelsixFont = AssetManager.GetFont("pixelsix");

    public void Update(GameTime gameTime)
    {
        now = gameTime.TotalGameTime.TotalSeconds;
        elapsed = (now - last);

        if (elapsed > msgFrequency)
        {
            msg = "FPS: " + (frames / elapsed).ToString() + "\nMessage interval: "
                + elapsed.ToString() +  "\nUpdates: " + updates.ToString()
                + "\nFrames: " + frames.ToString();
            elapsed = 0;
            frames = 0;
            updates = 0;
            last = now;
        }

        updates++;
    }

    public void DrawFps(SpriteBatch spriteBatch, Vector2 fpsDisplayPosition, Color fpsTextColor)
    {
        spriteBatch.DrawString(pixelsixFont, msg, fpsDisplayPosition, fpsTextColor);
        frames++;
    }
}
