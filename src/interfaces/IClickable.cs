using Microsoft.Xna.Framework;

namespace _2d_td.interfaces;

public interface IClickable
{
    public void OnLeftClick();
    public void OnRightClick();
    public bool IsMouseColliding(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition);
    public void OnStartHover();
    public void OnEndHover();
}
