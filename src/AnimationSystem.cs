using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public class AnimationSystem
{
    public struct AnimationData
    {
        public Texture2D Texture;
        public int FrameCount;
        public Vector2 FrameSize;
        public float DelaySeconds;

        public AnimationData(Texture2D texture, int frameCount, Vector2 frameSize, float delaySeconds)
        {
            Texture = texture;
            FrameCount = frameCount;
            FrameSize = frameSize;
            DelaySeconds = delaySeconds;
        }

        public AnimationData()
        {
            throw new InvalidOperationException("Pass all parameters via constructor!");
        }
    }

    public AnimationData Data { get; private set; }
    private float frameTimer;
    private int currentFrame;

    private Texture2D? overrideTexture;
    private float overrideTimer;

    public AnimationSystem(AnimationData animationData)
    {
        Data = animationData;
        this.frameTimer = Data.DelaySeconds;
    }

    public void UpdateAnimation(float deltaTime)
    {
        if (overrideTimer > 0f)
        {
            overrideTimer -= deltaTime;

            if (overrideTimer <= 0f)
            {
                overrideTimer = 0f;
                overrideTexture = null;
            }

            return;
        }

        frameTimer -= deltaTime;

        if (frameTimer <= 0f)
        {
            frameTimer = Data.DelaySeconds;
            currentFrame = (currentFrame + 1) % Data.FrameCount;
        }
    }

    public void OverrideTexture(Texture2D texture, float durationSeconds)
    {
        overrideTexture = texture;
        overrideTimer = durationSeconds;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotationRadians = 0f,
        Vector2 drawOrigin = default, float drawLayerDepth = 0.9f)
    {
        Rectangle? sourceRect = null;
        Texture2D texture = Data.Texture;

        if (overrideTimer > 0f && overrideTexture is not null)
        {
            texture = (Texture2D)overrideTexture;
        }
        else
        {
            var xPosHorizontal = Data.FrameSize.X * currentFrame;
            var x = (int)Math.Floor(xPosHorizontal % Data.Texture.Width);
            var y = (int)(Math.Floor(xPosHorizontal / Data.Texture.Width) * Data.FrameSize.Y);
            sourceRect = new Rectangle(x, y, (int)Data.FrameSize.X, (int)Data.FrameSize.Y);
        }

        spriteBatch.Draw(texture,
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
