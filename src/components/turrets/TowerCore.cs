using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using static _2d_td.BuildingSystem;

namespace _2d_td;

#nullable enable
public class TowerCore : GameComponent, IClickable
{
    public Entity Turret { get; private set; }
    public TowerUpgradeNode CurrentUpgrade { get; set; }

    public TurretDetailsPrompt? detailsPrompt;
    public bool detailsClosed;

    public TowerCore(Entity turret) : base(turret.Game)
    {
        Turret = turret;
        CurrentUpgrade = new TowerUpgradeNode("Default", parent: null,
            leftChild: null, rightChild: null);

        Turret.Game.Components.Add(this);
    }

    public Enemy? GetClosestEnemy(int tileRange)
    {
        Enemy? closestEnemy = null;
        float closestDistance = float.PositiveInfinity;

        // TODO: Don't loop over all enemies. Just the ones in range.
        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            var distanceToEnemy = Vector2.Distance(Turret.Position, enemy.Position);

            if (distanceToEnemy > tileRange * Grid.TileLength)
                continue;

            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    public void HandleCloseDetails(Vector2 mouseScreenPosition)
    {
        if (detailsPrompt is not null && detailsPrompt.ShouldCloseDetailsView(mouseScreenPosition))
        {
            CloseDetailsView();
            detailsClosed = true;
        }
        else
        {
            detailsClosed = false;
        }
    }

    public void CloseDetailsView()
    {
        UIComponent.Instance.RemoveUIEntity(detailsPrompt);
        detailsPrompt = null;
    }

    public void OnClick()
    {
        if (!detailsClosed && detailsPrompt is null)
        {
            detailsPrompt = new TurretDetailsPrompt(Turret.Game, Turret, UpgradeLeft, UpgradeRight, CurrentUpgrade);
            UIComponent.Instance.AddUIEntity(detailsPrompt);
        }

        detailsClosed = false;
    }

    public bool IsMouseColliding(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        return Collision.IsPointInEntity(mouseWorldPosition, Turret);
    }

    public TowerUpgradeNode UpgradeLeft()
    {
        if (CurrentUpgrade.LeftChild is null)
        {
            throw new InvalidOperationException($"Node {CurrentUpgrade.Name} does not have a left child node.");
        }

        if (!CurrencyManager.TryBuyUpgrade(CurrentUpgrade.LeftChild.Name)) return CurrentUpgrade;

        CurrentUpgrade = CurrentUpgrade.LeftChild;
        return CurrentUpgrade;
    }

    public TowerUpgradeNode UpgradeRight()
    {
        if (CurrentUpgrade.RightChild is null)
        {
            throw new InvalidOperationException($"Node {CurrentUpgrade.Name} does not have a right child node.");
        }

        if (!CurrencyManager.TryBuyUpgrade(CurrentUpgrade.RightChild.Name)) return CurrentUpgrade;

        CurrentUpgrade = CurrentUpgrade.RightChild;
        return CurrentUpgrade;
    }

    public static bool CanPlaceTower(TurretType towerType, Vector2 targetWorldPosition)
    {
        var targetGridPosition = Grid.SnapPositionToGrid(targetWorldPosition);

        var turretGridSize = towerType switch
        {
            TurretType.GunTurret => GunTurret.DefaultGridSize,
            _ => Vector2.One
        };

        for (int y = 0; y < turretGridSize.Y; y++)
        {
            for (int x = 0; x < turretGridSize.X; x++)
            {
                var position = targetGridPosition + new Vector2(x, y) * Grid.TileLength;

                if (Collision.IsPointInTerrain(position, Game1.Instance.Terrain))
                {
                    return false;
                }
            }
       }

        var turretGridHeight = turretGridSize.Y;

        var belowTilePosition = targetGridPosition + Vector2.UnitY * turretGridHeight * Grid.TileLength;
        var aboveTilePosition = targetGridPosition - Vector2.UnitY * Grid.TileLength;

        if (!Collision.IsPointInTerrain(belowTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        if (Collision.IsPointInTerrain(aboveTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        return true;
    }
}
