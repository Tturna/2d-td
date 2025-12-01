using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public class TowerCore : GameComponent, IClickable
{
    public Entity Turret { get; private set; }
    public TowerUpgradeNode CurrentUpgrade { get; set; }

    public TurretDetailsPrompt? detailsPrompt;
    public bool detailsClosed;

    public delegate void ClickedHandler();
    public event ClickedHandler? Clicked;

    public TowerCore(Entity turret) : base(turret.Game)
    {
        Turret = turret;
        CurrentUpgrade = new TowerUpgradeNode("Default", upgradeIcon: null, price: 0, parent: null,
            leftChild: null, rightChild: null);

        Turret.Game.Components.Add(this);

        InputSystem.LeftClicked += (mouseScreenPosition, _) => HandleCloseDetails(mouseScreenPosition);
        InputSystem.RightClicked += (mouseScreenPosition, _) => HandleCloseDetails(mouseScreenPosition, force: true);
    }

    public Enemy? GetClosestValidEnemy(int tileRange)
    {
        Enemy? closestEnemy = null;
        float closestDistance = float.PositiveInfinity;
        var range = tileRange * Grid.TileLength;
        var towerCenter = Turret.Position + Turret.Size / 2;
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(towerCenter, range);

        foreach (Enemy enemy in enemyCandidates)
        {
            var enemyCenter = enemy.Position + enemy.Size / 2;
            var distanceToEnemy = Vector2.Distance(towerCenter, enemyCenter);

            if (distanceToEnemy > range)
                continue;

            if (distanceToEnemy < closestDistance)
            {
                if (Collision.IsLineInTerrain(towerCenter, enemyCenter, out var _, out var _)) continue;

                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    public void HandleCloseDetails(Vector2 mouseScreenPosition, bool force = false)
    {
        if (detailsPrompt is not null && (force || detailsPrompt.ShouldCloseDetailsView(mouseScreenPosition)))
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
        detailsPrompt?.Destroy();
        detailsPrompt = null;
    }

    public void OnLeftClick()
    {
        if (!detailsClosed && detailsPrompt is null)
        {
            detailsPrompt = new TurretDetailsPrompt(Turret.Game, Turret, UpgradeLeft, UpgradeRight, CurrentUpgrade);
        }

        detailsClosed = false;

        Clicked?.Invoke();
    }

    public void OnRightClick() { }

    public bool IsMouseColliding(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        return Collision.IsPointInEntity(mouseWorldPosition, Turret);
    }

    public TowerUpgradeNode? UpgradeLeft()
    {
        if (CurrentUpgrade.LeftChild is null)
        {
            throw new InvalidOperationException($"Node {CurrentUpgrade.Name} does not have a left child node.");
        }

        if (!CurrencyManager.TryBuyUpgrade(CurrentUpgrade.LeftChild.Price)) return null;

        var costText = $"-{CurrentUpgrade.LeftChild.Price}";
        CurrentUpgrade = CurrentUpgrade.LeftChild;
        ((ITower)Turret).UpgradeTower(CurrentUpgrade);

        var costTextPosition = Turret.Position - Vector2.UnitY * 6;
        var textVelocity = -Vector2.UnitY * 25f;
        UIComponent.SpawnFlyoutText(costText, costTextPosition, textVelocity, lifetime: 1f);

        return CurrentUpgrade;
    }

    public TowerUpgradeNode UpgradeRight()
    {
        if (CurrentUpgrade.RightChild is null)
        {
            throw new InvalidOperationException($"Node {CurrentUpgrade.Name} does not have a right child node.");
        }

        if (!CurrencyManager.TryBuyUpgrade(CurrentUpgrade.RightChild.Price)) return CurrentUpgrade;

        var costText = $"-{CurrentUpgrade.RightChild.Price}";
        CurrentUpgrade = CurrentUpgrade.RightChild;
        ((ITower)Turret).UpgradeTower(CurrentUpgrade);

        var costTextPosition = Turret.Position - Vector2.UnitY * 6;
        var textVelocity = -Vector2.UnitY * 25f;
        UIComponent.SpawnFlyoutText(costText, costTextPosition, textVelocity, lifetime: 1f);

        return CurrentUpgrade;
    }

    public static bool DefaultCanPlaceTower(Vector2 towerGridSize, Vector2 targetWorldPosition)
    {
        var targetGridPosition = Grid.SnapPositionToGrid(targetWorldPosition);

        for (int y = 0; y < towerGridSize.Y; y++)
        {
            for (int x = 0; x < towerGridSize.X; x++)
            {
                var position = targetGridPosition + new Vector2(x, y) * Grid.TileLength;

                if (Collision.IsPointInTerrain(position, Game1.Instance.Terrain))
                {
                    return false;
                }
            }
        }

        var turretGridHeight = towerGridSize.Y;

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
