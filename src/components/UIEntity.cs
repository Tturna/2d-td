using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class UIEntity : Entity
{
    public UIEntity(Game game, Texture2D sprite) : base(game, sprite) { }
    public UIEntity(Game game, Vector2 position, Texture2D sprite) : base(game, position, sprite) { }
    public UIEntity(Game game, Vector2 position, AnimationSystem.AnimationData animationData) : base(game, position, animationData) { }

    public delegate void ButtonPressedHandler();
    public event ButtonPressedHandler ButtonPressed;

    public override void Update(GameTime gameTime)
    {
        // event is null when it has no handlers attached
        if (ButtonPressed is not null)
        {
            if (InputSystem.IsLeftMouseButtonClicked())
            {
                var mousePos = InputSystem.GetMouseScreenPosition();

                // TODO: Consider implementing a system that prevents buttons from being clicked if
                // another UI element is on top of it.
                if (Collision.IsPointInEntity(mousePos, this))
                {
                    OnButtonPressed();
                }
            }
        }

        base.Update(gameTime);
    }

    // Draw is called automatically if this is added as a component, which is not desired
    // because automatically drawn entities move with the camera. UI elements should
    // be absolute. UI elements are added as entities to components so that collision
    // would be simple.
    public override void Draw(GameTime gameTime) { }

    public void DrawCustom(GameTime gameTime)
    {
        if (AnimationSystem is not null)
        {
            base.Draw(gameTime);
        }
        else
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
    }

    private void OnButtonPressed()
    {
        ButtonPressed?.Invoke();
    }
}
