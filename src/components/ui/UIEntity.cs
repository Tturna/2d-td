using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public class UIEntity : Entity, IClickable
{
    public delegate void ButtonPressedHandler();
    public event ButtonPressedHandler? ButtonPressed;
    public event ButtonPressedHandler? ButtonStartHover;
    public event ButtonPressedHandler? ButtonEndHover;

    private Func<UIEntity, bool> removeUIEntityCallback;
    private SpriteFont? font;
    public string? Text;
    public Color TextColor;

    public UIEntity(Game game, Vector2? position, Action<UIEntity> addUIEntityCallback,
        Func<UIEntity, bool> removeUIEntityCallback, Texture2D sprite) : base(game, position, sprite)
    {
        addUIEntityCallback(this);
        this.removeUIEntityCallback = removeUIEntityCallback;
    }

    public UIEntity(Game game, List<UIEntity> uiEntities, Texture2D sprite) :
        this(game, position: null, uiEntities.Add, uiEntities.Remove, sprite) { }

    public UIEntity(Game game, List<UIEntity> uiEntities, Vector2 position, Texture2D sprite) :
        this(game, position, uiEntities.Add, uiEntities.Remove, sprite) { }

    public UIEntity(Game game, Action<UIEntity> addUIEntityCallback,
        Func<UIEntity, bool> removeUIEntityCallback, Vector2 position,
        AnimationSystem.AnimationData animationData) : base(game, position, animationData)
    {
        addUIEntityCallback(this);
        this.removeUIEntityCallback = removeUIEntityCallback;
    }

    public UIEntity(Game game, List<UIEntity> uiEntities, Vector2 position, AnimationSystem.AnimationData animationData) :
        this(game, uiEntities.Add, uiEntities.Remove, position, animationData) { }

    public UIEntity(Game game, Action<UIEntity> addUIEntityCallback,
        Func<UIEntity, bool> removeUIEntityCallback, SpriteFont font, string text,
        Color? color = null) :
        base(game, size: font.MeasureString(text))
    {
        addUIEntityCallback(this);
        this.removeUIEntityCallback = removeUIEntityCallback;
        this.font = font;
        Text = text;

        TextColor = color is null ? Color.White : (Color)color;
    }

    public UIEntity(Game game, List<UIEntity> uiEntities, SpriteFont font, string text) :
        this(game, uiEntities.Add, uiEntities.Remove, font, text) { }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    // Draw is called automatically if this is added as a component, which is not desired
    // because automatically drawn entities move with the camera. UI elements should
    // be absolute. UI elements are added as entities to components so that collision
    // would be simple.
    public override void Draw(GameTime gameTime) { }

    public virtual void DrawCustom(GameTime gameTime)
    {
        if (AnimationSystem is not null)
        {
            base.Draw(gameTime);
        }
        else if (Sprite is not null)
        {
            Game.SpriteBatch.Draw(Sprite,
                    Position,
                    sourceRectangle: null,
                    Color.White,
                    rotation: 0f,
                    origin: DrawOrigin,
                    scale: Scale,
                    effects: SpriteEffects.None,
                    layerDepth: DrawLayerDepth);
        }

        if (font is not null && Text is not null)
        {
            Game.SpriteBatch.DrawString(font,
                Text,
                Position,
                TextColor,
                rotation: 0f,
                origin: DrawOrigin,
                scale: Scale,
                effects: SpriteEffects.None,
                layerDepth: DrawLayerDepth);
        }
    }

    public override void Destroy()
    {
        removeUIEntityCallback(this);
        base.Destroy();
    }

    public void ClearAllButtonHandlers()
    {
        ButtonPressed = null;
        ButtonStartHover = null;
        ButtonEndHover = null;
    }

    public void OnLeftClick()
    {
        ButtonPressed?.Invoke();
    }

    public void OnRightClick() { }

    public bool IsMouseColliding(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        if (ButtonPressed is null && ButtonStartHover is null && ButtonEndHover is null) return false;

        // TODO: Consider implementing a system that prevents buttons from being clicked if
        // another UI element is on top of it.
        return Collision.IsPointInEntity(mouseScreenPosition, this);
    }

    public void OnStartHover()
    {
        ButtonStartHover?.Invoke();
    }

    public void OnEndHover()
    {
        ButtonEndHover?.Invoke();
    }
}
