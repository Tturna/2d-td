using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td.interfaces;

public interface ITower
{
    public abstract static AnimationSystem.AnimationData GetUnupgradedBaseAnimationData();
    /// <summary>
    /// Returns a key-value pair for each piece of a tower, including the base. Keys are
    /// the UI entities with the sprite of a certain piece. Values are relative positions
    /// to the base. The base should be the first element in the list. The base element should
    /// have relative position (0, 0) unless it needs an offset.
    /// </summary>
    public abstract static List<KeyValuePair<UIEntity, Vector2>> GetUnupgradedPartIcons(List<UIEntity> uiElements);
    public abstract static Vector2 GetDefaultGridSize();
    public abstract static BuildingSystem.TowerType GetTowerType();
    public abstract static bool CanPlaceTower(Vector2 targetWorldPosition);
    public abstract static Entity CreateNewInstance(Game game, Vector2 worldPosition);
    public abstract void UpgradeTower(TowerUpgradeNode newUpgrade);
    public abstract static float GetBaseRange();
    public abstract float GetRange();
}
