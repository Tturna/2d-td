using System;
using System.Collections.Generic;
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

    public AnimationData BaseAnimationData { get; private set; }
    public Dictionary<string, AnimationData>? AltAnimationStates;
    private AnimationData currentAnimationData { get; set; }

    private float frameTimer;
    private int currentFrame;

    private Texture2D? overrideTexture;
    private float overrideTimer;

    public AnimationSystem(AnimationData baseAnimationData)
    {
        BaseAnimationData = baseAnimationData;
        this.frameTimer = BaseAnimationData.DelaySeconds;
        currentAnimationData = BaseAnimationData;
    }

    public void AddAnimationState(string stateName, AnimationData animationData)
    {
        if (AltAnimationStates is null) AltAnimationStates = new();

        AltAnimationStates.Add(stateName, animationData);
    }

    /// <summary>
    /// Switch animation state to given state. Pass null to switch to base state.
    /// </summary>
    public void ToggleAnimationState(string? stateName)
    {
        if (stateName is null)
        {
            currentAnimationData = BaseAnimationData;
            return;
        }

        // Assume animation state exists. Let it throw otherwise.
        if (AltAnimationStates!.TryGetValue(stateName, out var animationData))
        {
            currentAnimationData = animationData;
        }
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
            frameTimer = BaseAnimationData.DelaySeconds;
            currentFrame = (currentFrame + 1) % BaseAnimationData.FrameCount;
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
        Texture2D texture = BaseAnimationData.Texture;

        if (overrideTimer > 0f && overrideTexture is not null)
        {
            texture = (Texture2D)overrideTexture;
        }
        else
        {
            var xPosHorizontal = BaseAnimationData.FrameSize.X * currentFrame;
            var x = (int)Math.Floor(xPosHorizontal % BaseAnimationData.Texture.Width);
            var y = (int)(Math.Floor(xPosHorizontal / BaseAnimationData.Texture.Width) * BaseAnimationData.FrameSize.Y);
            sourceRect = new Rectangle(x, y, (int)BaseAnimationData.FrameSize.X, (int)BaseAnimationData.FrameSize.Y);
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
