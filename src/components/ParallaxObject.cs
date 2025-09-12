using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class ParallaxObject
{
    public Texture2D Sprite { get; set; }
    public float ParallaxLayer { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Movement { get; set; }

    public ParallaxObject(Vector2 pos, float layer, string sprite, Vector2 movement)
    {
        Sprite = AssetManager.GetTexture(sprite);
        ParallaxLayer = layer;
        Position = pos;
        Movement = movement;
    }
}
