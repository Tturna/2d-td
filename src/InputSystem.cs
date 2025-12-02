using System.Collections.Generic;
using _2d_td;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public static class InputSystem
{
    private static Game1 game;
    private static MouseState mouseState;
    private static KeyboardState keyboardState;

    private static bool isMouseLeftDown;
    private static bool isMouseRightDown;
    private static bool isMouseLeftClicked;
    private static bool isMouseRightClicked;
    private static Dictionary<Keys, bool> keysDownMap = new();

    private static int totalScrollAmount;
    private static int justScrolledAmount;

    private static bool collectionModified;

    public delegate void ClickedHandler(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition);
    public static event ClickedHandler LeftClicked;
    public static event ClickedHandler RightClicked;

    public static void Initialize(Game1 game)
    {
        InputSystem.game = game;
        SceneManager.SceneLoaded += _ => collectionModified = true;
    }

    public static void Update()
    {
        mouseState = Mouse.GetState();
        keyboardState = Keyboard.GetState();

        if (IsLeftMouseButtonDown())
        {
            // Clicked is true the first time the button is down, but
            // next update makes it false again.
            isMouseLeftClicked = !isMouseLeftDown;
            isMouseLeftDown = true;
        }
        else
        {
            isMouseLeftClicked = false;
            isMouseLeftDown = false;
        }

        if (IsRightMouseButtonDown())
        {
            isMouseRightClicked = !isMouseRightDown;
            isMouseRightDown = true;
        }
        else
        {
            isMouseRightClicked = false;
            isMouseRightDown = false;
        }

        foreach (var keyDownItem in keysDownMap)
        {
            var key = keyDownItem.Key;
            var isDown = keyDownItem.Value;

            if (isDown && keyboardState.IsKeyUp(key))
            {
                keysDownMap[key] = false;
            }
        }

        var newScrollAmount = Mouse.GetState().ScrollWheelValue;
        justScrolledAmount = newScrollAmount - totalScrollAmount;
        totalScrollAmount = newScrollAmount;

        if (IsLeftMouseButtonClicked())
        {
            var mouseScreenPos = InputSystem.GetMouseScreenPosition();
            var mouseWorldPos = InputSystem.GetMouseWorldPosition();
            OnLeftClicked(mouseScreenPos, mouseWorldPos);

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
                    clickable.OnLeftClick();
                }

                // When a clickable causes a scene change, the components list will change.
                // The loop should break to not check for clicks incorrectly.
                if (collectionModified) break;
            }
        }

        if (IsRightMouseButtonClicked())
        {
            var mouseScreenPos = InputSystem.GetMouseScreenPosition();
            var mouseWorldPos = InputSystem.GetMouseWorldPosition();
            OnRightClicked(mouseScreenPos, mouseWorldPos);

            collectionModified = false;

            for (int i = game.Components.Count - 1; i >= 0; i--)
            {
                if (game.Components.Count < i + 1) continue;

                var component = game.Components[i];
                if (component is not IClickable) continue;

                var clickable = (IClickable)component;

                if (clickable.IsMouseColliding(mouseScreenPos, mouseWorldPos))
                {
                    clickable.OnRightClick();
                }

                if (collectionModified) break;
            }
        }
    }

    private static void OnLeftClicked(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        LeftClicked?.Invoke(mouseScreenPosition, mouseWorldPosition);
    }

    private static void OnRightClicked(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        RightClicked?.Invoke(mouseScreenPosition, mouseWorldPosition);
    }

    public static Vector2 GetMouseWorldPosition()
    {
        var mouseScreenPos = GetMouseScreenPosition();
        var worldPos = Camera.ScreenToWorldPosition(mouseScreenPos);

        return worldPos;
    }

    public static Vector2 GetMouseScreenPosition()
    {
        Point mousePoint = mouseState.Position;
        var mousePos = new Vector2(mousePoint.X, mousePoint.Y);
        mousePos -= game.RenderedBlackBoxSize / 2;
        float renderTargetScaleFactor = game.RenderTargetSize.X / game.NativeScreenWidth;

        // Turn real mouse screen position (scaled & stretched) into native screen position.
        // The mouse position is affected by window scaling but nothing should use that value(?).
        // That's why we can turn the mouse position into native space immediately and not
        // need to consider the real, scaled screen space when translating coordinates.
        mousePos /= renderTargetScaleFactor;

        return mousePos;
    }

    public static bool IsLeftMouseButtonDown()
    {
        return mouseState.LeftButton == ButtonState.Pressed;
    }

    public static bool IsRightMouseButtonDown()
    {
        return mouseState.RightButton == ButtonState.Pressed;
    }

    public static bool IsLeftMouseButtonClicked()
    {
        return isMouseLeftClicked;
    }

    public static bool IsRightMouseButtonClicked()
    {
        return isMouseRightClicked;
    }

    public static int mouseJustScrolledAmount()
    {
        return justScrolledAmount;
    }

    public static bool IsKeyDown(Keys key)
    {
        var isDown = keyboardState.IsKeyDown(key);

        if (isDown)
        {
            keysDownMap[key] = true;
        }

        return isDown;
    }

    public static bool IsKeyTapped(Keys key)
    {
        var isDown = keyboardState.IsKeyDown(key);

        if (!isDown) return false;

        var isKeyInMap = keysDownMap.TryGetValue(key, out var isHeld);

        if (!isKeyInMap)
        {
            keysDownMap[key] = true;
            return true;
        }

        if (!isHeld)
        {
            keysDownMap[key] = true;
        }

        return !isHeld;
    }

    /// <summary>
    /// Force registration of clickables to stop for this frame. Useful if for example a button
    /// creates a new button in the same place but the new one should not be clicked in the same
    /// frame.
    /// </summary>
    public static void ForceClickableInterrupt()
    {
        collectionModified = true;
    }
}
