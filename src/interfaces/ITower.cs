using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td.interfaces;

public interface ITower
{
    public abstract static Texture2D GetTowerBaseSprite();
    public abstract static Vector2 GetDefaultGridSize();
    public abstract static BuildingSystem.TowerType GetTowerType();
    public abstract static bool CanPlaceTower(Vector2 targetWorldPosition);
    public abstract static Entity CreateNewInstance(Game game);
}
