using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class UIEntity : Entity
{
    public UIEntity(Game game, Texture2D sprite) : base(game, sprite) { }
    public UIEntity(Game game, Vector2 position, Texture2D sprite) : base(game, position, sprite) { }

    public delegate void ButtonPressedHandler();
    public event ButtonPressedHandler ButtonPressed;

    public override void Update(GameTime gameTime)
    {
        if (ButtonPressed is null) return;
        if (!InputSystem.IsLeftMouseButtonClicked()) return;
        
        var mousePos = InputSystem.GetRealMousePosition();

        // TODO: Consider implementing a system that prevents buttons from being clicked if
        // another UI element is on top of it.
        if (!Collision.IsPointInEntity(mousePos, this)) return;

        OnButtonPressed();
    }

    // Draw is called automatically if this is added as a component, which is not desired
    // because automatically drawn entities move with the camera. UI elements should
    // be absolute. UI elements are added as entities to components so that collision
    // would be simple.
    public override void Draw(GameTime gameTime) { }

    public void DrawCentered()
    {
        var spriteCenter = new Vector2(Sprite.Width, Sprite.Height) * 0.5f;

        Game.SpriteBatch.Draw(Sprite,
                Position,
                sourceRectangle: null,
                Color.White,
                rotation: 0f,
                origin: spriteCenter,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: 0.9f);
    }

    private void OnButtonPressed()
    {
        ButtonPressed?.Invoke();
    }
}
