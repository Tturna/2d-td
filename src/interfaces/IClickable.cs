using Microsoft.Xna.Framework;

namespace _2d_td.interfaces;

public interface IClickable
{
    public void OnClick();
    public bool IsMouseColliding(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition);
}
