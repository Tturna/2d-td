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
    private AnimationData currentAnimationData;
    private const string BaseStateName = "base";
    private string? currentStateName;
    private string? oneShotPreviousState;

    private float frameTimer;
    private float oneShotTimer;
    private int currentFrame;

    private Texture2D? overrideTexture;
    private float overrideTimer;

    public AnimationSystem(AnimationData baseAnimationData)
    {
        BaseAnimationData = baseAnimationData;
        this.frameTimer = BaseAnimationData.DelaySeconds;
        currentAnimationData = BaseAnimationData;
        currentStateName = BaseStateName;
    }

    public void AddAnimationState(string stateName, AnimationData animationData)
    {
        if (AltAnimationStates is null) AltAnimationStates = new();

        AltAnimationStates.Add(stateName, animationData);
    }

    /// <summary>
    /// Switch animation state to given state. Pass null to switch to base state.
    /// Return new animation state data.
    /// </summary>
    public AnimationData ToggleAnimationState(string? stateName)
    {
        currentFrame = 0;

        if (stateName is null || stateName == BaseStateName)
        {
            currentAnimationData = BaseAnimationData;
            currentStateName = BaseStateName;
            return currentAnimationData;
        }

        // Assume animation state exists. Let it throw otherwise.
        if (AltAnimationStates!.TryGetValue(stateName, out var animationData))
        {
            currentAnimationData = animationData;
            currentStateName = stateName;
        }

        return currentAnimationData;
    }

    /// <summary>
    /// Play given animation state and immediately switch back to the previous state.
    /// Pass null to play base state. Will override any active oneshot animation but will
    /// switch back to the original state, not the overridden one.
    /// </summary>
    public void OneShotAnimationState(string? stateName)
    {
        // If a one shot animation is triggered while another one is already active,
        // override the previous animation but keep the previous state. This way the
        // new animation state won't transition to the overridden one.
        if (oneShotTimer <= 0f)
        {
            oneShotPreviousState = currentStateName;
        }

        var newAnimationData = ToggleAnimationState(stateName);
        oneShotTimer = (newAnimationData.FrameCount - 1) * newAnimationData.DelaySeconds;
    }

    public void UpdateAnimation(float deltaTime)
    {
        if (oneShotTimer > 0f)
        {
            oneShotTimer -= deltaTime;

            if (oneShotTimer <= 0f)
            {
                oneShotTimer = 0f;
            }
        }

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

        if (oneShotPreviousState is not null && oneShotTimer <= 0f)
        {
            ToggleAnimationState(oneShotPreviousState);
            oneShotPreviousState = null;
        }

        frameTimer -= deltaTime;

        if (frameTimer <= 0f)
        {
            frameTimer = currentAnimationData.DelaySeconds;
            currentFrame = (currentFrame + 1) % currentAnimationData.FrameCount;
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
        Texture2D texture = currentAnimationData.Texture;

        if (overrideTimer > 0f && overrideTexture is not null)
        {
            texture = (Texture2D)overrideTexture;
        }
        else
        {
            var xPosHorizontal = currentAnimationData.FrameSize.X * currentFrame;
            var x = (int)Math.Floor(xPosHorizontal % currentAnimationData.Texture.Width);
            var y = (int)(Math.Floor(xPosHorizontal / currentAnimationData.Texture.Width) * currentAnimationData.FrameSize.Y);
            sourceRect = new Rectangle(x, y, (int)currentAnimationData.FrameSize.X, (int)currentAnimationData.FrameSize.Y);
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
