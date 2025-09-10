using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public class Entity : DrawableGameComponent
{
    // Hide Game field of DrawableGameComponent so children can directly use a Game1 instance.
    new protected Game1 Game;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float RotationRadians { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 DrawOrigin { get; set; } = Vector2.Zero;
    // 1 = back, 0 = front
    public float DrawLayerDepth { get; set; } = 0.9f;
    public Texture2D? Sprite { get; set; }
    public AnimationSystem? AnimationSystem;

    public Entity(Game game, Texture2D? sprite = null, Vector2 size = default) : base(game)
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
    }

    public Entity(Game game, Vector2 position, Texture2D? sprite = null, Vector2 size = default)
        : this(game, sprite, size)
    {
        Position = position;
    }

    public Entity(Game game, Vector2 position, AnimationSystem.AnimationData animationData, Texture2D? sprite = null)
        : this(game, sprite, animationData.FrameSize)
    {
        Position = position;
        AnimationSystem = new AnimationSystem(animationData);
    }

    public override void Update(GameTime gameTime)
    {
        if (AnimationSystem is not null)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            AnimationSystem.UpdateAnimation(deltaTime);
        }

        base.Update(gameTime);
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();
    }

    public override void Draw(GameTime gameTime)
    {

        if (AnimationSystem is not null)
        {
            AnimationSystem.Draw(Game.SpriteBatch, Position, RotationRadians, DrawOrigin, DrawLayerDepth);
        }
        else if (Sprite is null) return;
        else
        {
            Game.SpriteBatch.Draw(Sprite,
                    Position,
                    sourceRectangle: null,
                    Color.White,
                    rotation: RotationRadians,
                    origin: DrawOrigin,
                    scale: Vector2.One,
                    effects: SpriteEffects.None,
                    layerDepth: DrawLayerDepth);

        }

        base.Draw(gameTime);
    }
}
