using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class AnimationSystem
{
    public struct AnimationData
    {
        public Texture2D Texture;
        public int FrameCount;
        public Vector2 FrameSize;
        public float DelaySeconds;
    }

    private AnimationData data;
    private float frameTimer;
    private int currentFrame;

    public AnimationSystem(AnimationData animationData)
    {
        data = animationData;
        this.frameTimer = data.DelaySeconds;
    }

    public void UpdateAnimation(float deltaTime)
    {
        frameTimer -= deltaTime;

        if (frameTimer <= 0f)
        {
            frameTimer = data.DelaySeconds;
            currentFrame = (currentFrame + 1) % data.FrameCount;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotationRadians = 0f,
        Vector2 drawOrigin = default, float drawLayerDepth = 0.9f)
    {
        var xPosHorizontal = data.FrameSize.X * currentFrame;
        var x = (int)Math.Floor(xPosHorizontal % data.Texture.Width);
        var y = (int)(Math.Floor(xPosHorizontal / data.Texture.Width) * data.FrameSize.Y);
        var sourceRect = new Rectangle(x, y, (int)data.FrameSize.X, (int)data.FrameSize.Y);

        spriteBatch.Draw(data.Texture,
                position,
                sourceRectangle: sourceRect,
                Color.White,
                rotation: rotationRadians,
                origin: drawOrigin,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: drawLayerDepth);
    }
}
