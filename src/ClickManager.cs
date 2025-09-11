using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

// TODO: Consider combining with InputSystem
public static class ClickManager
{
    private static Game1 game;
    private static bool collectionModified;

    public delegate void ClickedHandler(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition);
    public static event ClickedHandler Clicked;

    public static void Initialize(Game1 game)
    {
        ClickManager.game = game;
        SceneManager.SceneLoaded += _ => collectionModified = true;
    }

    public static void Update()
    {
        if (InputSystem.IsLeftMouseButtonClicked())
        {
            var mouseScreenPos = InputSystem.GetMouseScreenPosition();
            var mouseWorldPos = InputSystem.GetMouseWorldPosition();
            OnClicked(mouseScreenPos, mouseWorldPos);

            collectionModified = false;

            for (int i = game.Components.Count - 1; i >= 0; i--)
            {
                // If a clickable destroys multiple components, i might go out of bounds.
                // Iterate until we find the next available one.
                if (game.Components.Count < i + 1) continue;

                var component = game.Components[i];
                if (component is not IClickable) continue;

                var clickable = (IClickable)component;

                if (clickable.IsMouseColliding(mouseScreenPos, mouseWorldPos))
                {
                    clickable.OnClick();
                }

                // When a clickable causes a scene change, the components list will change.
                // The loop should break to not check for clicks incorrectly.
                if (collectionModified) break;
            }
        }
    }

    public static bool DefaultCollisionCheck(Entity entity, Vector2 mouseWorldPos)
    {
        return Collision.IsPointInEntity(mouseWorldPos, entity);
    }

    private static void OnClicked(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        Clicked?.Invoke(mouseScreenPosition, mouseWorldPosition);
    }
}
