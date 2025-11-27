using Microsoft.Xna.Framework;

namespace _2d_td.interfaces;

public interface IKnockable
{
    public void ApplyKnockback(Vector2 knockback);
}
