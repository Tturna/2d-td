using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public static class InputSystem
{
    private static MouseState mouseState;
    private static Vector2 cachedMousePosition;

    private static bool isMouseLeftDown;
    private static bool isMouseRightDown;
    private static bool isMouseLeftClicked;
    private static bool isMouseRightClicked;

    public static void Update()
    {
        mouseState = Mouse.GetState();
        cachedMousePosition = -Vector2.One;

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
    }

    public static Vector2 GetMousePosition()
    {
        if (cachedMousePosition != -Vector2.One)
        {
            return cachedMousePosition;
        }

        var mousePoint = mouseState.Position;
        var mousePos = new Vector2(mousePoint.X, mousePoint.Y);
        cachedMousePosition = mousePos;
        
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
}
