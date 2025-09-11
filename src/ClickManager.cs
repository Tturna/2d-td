using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class ClickManager
{
    private static Game1 game;
    private static bool collectionModified;

    public static void Initialize(Game1 game)
    {
        ClickManager.game = game;
        SceneManager.SceneLoaded += _ => collectionModified = true;
    }

    public static void Update()
    {
        if (InputSystem.IsLeftMouseButtonClicked())
        {
            collectionModified = false;

            foreach (var component in game.Components)
            {
                if (component is not IClickable) continue;

                var clickable = (IClickable)component;
                var mouseScreenPos = InputSystem.GetMouseScreenPosition();
                var mouseWorldPos = InputSystem.GetMouseWorldPosition();

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
}
