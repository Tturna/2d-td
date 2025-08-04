using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class InputSystem
{
    private MouseState _mouseState;
    private Vector2 _cachedMousePosition;

    private bool _isMouseLeftDown;
    private bool _isMouseRightDown;
    private bool _isMouseLeftClicked;
    private bool _isMouseRightClicked;

    public void Update()
    {
        _mouseState = Mouse.GetState();
        _cachedMousePosition = -Vector2.One;

        if (IsLeftMouseButtonDown())
        {
            // Clicked is true the first time the button is down, but
            // next update makes it false again.
            _isMouseLeftClicked = !_isMouseLeftDown;
            _isMouseLeftDown = true;
        }
        else
        {
            _isMouseLeftClicked = false;
            _isMouseLeftDown = false;
        }

        if (IsRightMouseButtonDown())
        {
            _isMouseRightClicked = !_isMouseRightDown;
            _isMouseRightDown = true;
        }
        else
        {
            _isMouseRightClicked = false;
            _isMouseRightDown = false;
        }
    }

    public Vector2 GetMousePosition()
    {
        if (_cachedMousePosition != -Vector2.One)
        {
            return _cachedMousePosition;
        }

        var mousePoint = _mouseState.Position;
        var mousePos = new Vector2(mousePoint.X, mousePoint.Y);
        _cachedMousePosition = mousePos;
        
        return mousePos;
    }

    public bool IsLeftMouseButtonDown()
    {
        return _mouseState.LeftButton == ButtonState.Pressed;
    }

    public bool IsRightMouseButtonDown()
    {
        return _mouseState.RightButton == ButtonState.Pressed;
    }

    public bool IsLeftMouseButtonClicked()
    {
        return _isMouseLeftClicked;
    }

    public bool IsRightMouseButtonClicked()
    {
        return _isMouseRightClicked;
    }
}
