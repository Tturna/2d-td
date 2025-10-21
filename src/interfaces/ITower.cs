using Microsoft.Xna.Framework;

namespace _2d_td.interfaces;

public interface ITower
{
    public abstract static AnimationSystem.AnimationData GetTowerBaseAnimationData();
    public abstract static Vector2 GetDefaultGridSize();
    public abstract static BuildingSystem.TowerType GetTowerType();
    public abstract static bool CanPlaceTower(Vector2 targetWorldPosition);
    public abstract static Entity CreateNewInstance(Game game, Vector2 worldPosition);
    public abstract void UpgradeTower(TowerUpgradeNode newUpgrade);
}
