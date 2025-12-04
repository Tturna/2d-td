using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public class Entity : DrawableGameComponent
{
    // Hide Game field of DrawableGameComponent so children can directly use a Game1 instance.
    new protected Game1 Game;
    public Vector2 Position { get; private set; } = Vector2.Zero;
    public float RotationRadians { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 DrawOrigin { get; set; } = Vector2.Zero;
    public Vector2 DrawOffset { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    // 1 = back, 0 = front
    public float DrawLayerDepth { get; set; } = 0.9f;
    public Texture2D? Sprite { get; set; }
    public AnimationSystem? AnimationSystem;

    protected bool IsDestroyed = false;

    protected Vector2 preStretchScale;
    protected Vector2 stretchScale;
    protected float stretchDuration;
    protected float stretchDurationLeft;

    public Entity(Game game, Vector2? position = null, Texture2D? sprite = null, Vector2 size = default) : base(game)
    {
        this.Game = (Game1)game;
        Sprite = sprite;

        if (Sprite is null)
        {
            if (size == default)
            {
                throw new ArgumentException("Given sprite is null and size was not passed. Either pass a sprite or specify a size.", nameof(size));
            }
            else
            {
                Size = size;
            }
        }
        else
        {
            Size = new Vector2(Sprite.Width, Sprite.Height);
        }

        if (position is not null)
        {
            Position = (Vector2)position;
        }

        Game.Components.Add(this);
    }

    public Entity(Game game, Vector2 position, AnimationSystem.AnimationData animationData, Texture2D? sprite = null)
        : base(game)
    {
        this.Game = (Game1)game;
        Position = position;
        AnimationSystem = new AnimationSystem(animationData);
        Size = animationData.FrameSize;
        Game.Components.Add(this);
    }

    public override void Initialize()
    {
        preStretchScale = Scale;
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        if (IsDestroyed) return;

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (AnimationSystem is not null)
        {
            AnimationSystem.UpdateAnimation(deltaTime);
        }

        if (stretchDurationLeft > 0)
        {
            stretchDurationLeft -= deltaTime;

            if (stretchDurationLeft <= 0)
            {
                Scale = preStretchScale;
            }
            else
            {
                var normalDuration = stretchDurationLeft / stretchDuration;
                Scale = Vector2.Lerp(stretchScale, preStretchScale, 1f - normalDuration);
            }
        }

        base.Update(gameTime);
    }

    public virtual void FixedUpdate(float deltaTime) { }

    protected override void LoadContent()
    {
        base.LoadContent();
    }

    public override void Draw(GameTime gameTime)
    {
        if (AnimationSystem is not null)
        {
            AnimationSystem.Draw(Game.SpriteBatch, Position, RotationRadians, DrawOrigin, DrawOffset, Scale, DrawLayerDepth);
        }
        else if (Sprite is null) return;
        else
        {
            Game.SpriteBatch.Draw(Sprite,
                    Position + DrawOffset,
                    sourceRectangle: null,
                    Color.White,
                    rotation: RotationRadians,
                    origin: DrawOrigin,
                    scale: Scale,
                    effects: SpriteEffects.None,
                    layerDepth: DrawLayerDepth);

        }

        base.Draw(gameTime);
    }

    public virtual void Destroy()
    {
        var index = Game.Components.IndexOf(this);

        if (index >= 0)
        {
            Game.Components.RemoveAt(index);
            IsDestroyed = true;
        }
    }

    public virtual void UpdatePosition(Vector2 positionChange)
    {
        Position += positionChange;
    }

    public virtual void SetPosition(Vector2 newPosition)
    {
        Position = newPosition;
    }

    public virtual void Rotate(float radians)
    {
        RotationRadians += radians;

        while (RotationRadians >= MathHelper.Tau)
        {
            RotationRadians -= MathHelper.Tau;
        }

        while (RotationRadians < 0)
        {
            RotationRadians += MathHelper.Tau;
        }
    }

    public virtual void StretchImpact(Vector2 scale, float duration)
    {
        Scale = preStretchScale;
        stretchScale = scale;
        stretchDuration = duration;
        stretchDurationLeft = duration;
    }
}
