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
    public AnimationData CurrentAnimationData { get; private set; }

    private Dictionary<string, AnimationData>? altAnimationStates;
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
        CurrentAnimationData = BaseAnimationData;
        currentStateName = BaseStateName;
    }

    public void AddAnimationState(string stateName, AnimationData animationData)
    {
        if (altAnimationStates is null) altAnimationStates = new();

        altAnimationStates.Add(stateName, animationData);
    }

    public void ChangeAnimationState(string? stateName, AnimationData animationData)
    {
        if (stateName is null || stateName == BaseStateName)
        {
            BaseAnimationData = animationData;

            if (currentStateName == BaseStateName)
            {
                ToggleAnimationState(stateName);
            }

            return;
        }

        if (altAnimationStates is null)
        {
            throw new KeyNotFoundException($"No animation states added. Can't change state: {stateName}");
        }

        if (!altAnimationStates.ContainsKey(stateName))
        {
            throw new KeyNotFoundException($"Animation state {stateName} can't be changed because it is not added.");
        }

        altAnimationStates[stateName] = animationData;

        if (currentStateName == stateName)
        {
            ToggleAnimationState(stateName);
        }
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
            CurrentAnimationData = BaseAnimationData;
            currentStateName = BaseStateName;
            frameTimer = CurrentAnimationData.DelaySeconds;
            return CurrentAnimationData;
        }

        if (altAnimationStates is null)
        {
            throw new KeyNotFoundException($"No animation states added. Can't toggle state: {stateName}");
        }

        if (!altAnimationStates.TryGetValue(stateName, out var animationData))
        {
            throw new KeyNotFoundException($"Animation state {stateName} not added.");
        }

        CurrentAnimationData = animationData;
        currentStateName = stateName;
        frameTimer = CurrentAnimationData.DelaySeconds;

        return CurrentAnimationData;
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
        oneShotTimer = newAnimationData.FrameCount * newAnimationData.DelaySeconds;
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
            NextFrame();
        }
    }

    public void NextFrame()
    {
        frameTimer = CurrentAnimationData.DelaySeconds;
        currentFrame = (currentFrame + 1) % CurrentAnimationData.FrameCount;
    }

    public void OverrideTexture(Texture2D texture, float durationSeconds)
    {
        overrideTexture = texture;
        overrideTimer = durationSeconds;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotationRadians = 0f,
        Vector2 drawOrigin = default, Vector2 drawOffset = default, Vector2? scale = null, float drawLayerDepth = 0.9f)
    {
        Rectangle? sourceRect = null;
        Texture2D texture = CurrentAnimationData.Texture;

        if (overrideTimer > 0f && overrideTexture is not null)
        {
            texture = (Texture2D)overrideTexture;
        }
        else
        {
            var xPosHorizontal = CurrentAnimationData.FrameSize.X * currentFrame;
            var x = (int)Math.Floor(xPosHorizontal % CurrentAnimationData.Texture.Width);
            var y = (int)(Math.Floor(xPosHorizontal / CurrentAnimationData.Texture.Width) * CurrentAnimationData.FrameSize.Y);
            sourceRect = new Rectangle(x, y, (int)CurrentAnimationData.FrameSize.X, (int)CurrentAnimationData.FrameSize.Y);
        }

        var usedScale = scale is null ? Vector2.One : (Vector2)scale;

        // TODO: Use offset if scaling entities whose origin is not in the center.
        // var scaleDiff = Vector2.One - usedScale;
        // var sizeRect = sourceRect is not null ? (Rectangle)sourceRect : texture.Bounds;
        // var size = new Vector2(sizeRect.Width, sizeRect.Height);
        // var offset = scaleDiff * size;
        var offset = Vector2.Zero;

        spriteBatch.Draw(texture,
                position + offset + drawOffset,
                sourceRectangle: sourceRect,
                Color.White,
                rotation: rotationRadians,
                origin: drawOrigin,
                scale: usedScale,
                effects: SpriteEffects.None,
                layerDepth: drawLayerDepth);
    }
}
