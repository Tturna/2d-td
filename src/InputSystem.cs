using _2d_td;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public static class InputSystem
{
    private static MouseState mouseState;

    private static bool isMouseLeftDown;
    private static bool isMouseRightDown;
    private static bool isMouseLeftClicked;
    private static bool isMouseRightClicked;

    private static int totalScrollAmount;
    private static int justScrolledAmount;

    public static void Update()
    {
        mouseState = Mouse.GetState();

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

        var newScrollAmount = Mouse.GetState().ScrollWheelValue;
        justScrolledAmount = newScrollAmount - totalScrollAmount;
        totalScrollAmount = newScrollAmount;
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
}
