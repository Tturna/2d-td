using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public class TowerCore : GameComponent, IClickable
{
    public Entity Turret { get; private set; }
    public TowerUpgradeNode CurrentUpgrade { get; set; }
    public HealthSystem Health { get; private set; }

    public TurretDetailsPrompt? detailsPrompt;
    public bool detailsClosed = true;

    public delegate void ClickedHandler();
    public event ClickedHandler? LeftClicked;
    public event ClickedHandler? RightClicked;

    private HashSet<Enemy> enemiesThatDamagedTurret = new();
    private float brokenParticleInterval = 0.2f;
    private float brokenParticleTimer;

    public TowerCore(Entity turret) : base(turret.Game)
    {
        Turret = turret;
        Health = new HealthSystem(Turret, initialHealth: 100);
        Health.Died += OnDeath;
        CurrentUpgrade = new TowerUpgradeNode("Default", upgradeIcon: null, price: 0, parent: null,
            leftChild: null, rightChild: null);

        Turret.Game.Components.Add(this);

        InputSystem.LeftClicked += (mouseScreenPosition, _) => HandleCloseDetails(mouseScreenPosition);
        InputSystem.RightClicked += (mouseScreenPosition, _) => HandleCloseDetails(mouseScreenPosition, force: true);

        WaveSystem.WaveEnded += () =>
        {
            enemiesThatDamagedTurret.Clear();
            Health.ResetHealth();
        };
    }

    public override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Health.CurrentHealth <= 0)
        {
            brokenParticleTimer += deltaTime;

            if (brokenParticleTimer >= brokenParticleInterval)
            {
                brokenParticleTimer = 0;
                ParticleSystem.PlayBrokenTowerEffect(Turret.Position + Turret.Size / 2);
                ParticleSystem.PlaySingleSmokeParticle(Turret.Position + Turret.Size / 2, -Vector2.UnitY);
            }

            return;
        }

        // Set draw origin to bot right instead of top left and offset tower drawing
        // so that variable idle and fire animation frame sizes don't make the tower look like
        // it's shifting as it changes animations.
        var currentAnimationData = Turret.AnimationSystem!.CurrentAnimationData;
        var baseAnimationData = Turret.AnimationSystem!.BaseAnimationData;
        Turret.DrawOrigin = currentAnimationData.FrameSize;
        Turret.DrawOffset = baseAnimationData.FrameSize;

        // turret damage
        var enemyCandidates = EnemySystem.EnemyBins.GetBinAndNeighborValues(Turret.Position + Turret.Size / 2);

        foreach (var enemy in enemyCandidates)
        {
            if (!enemiesThatDamagedTurret.Contains(enemy) && Collision.AreEntitiesColliding(enemy, Turret))
            {
                enemiesThatDamagedTurret.Add(enemy);
                Health.TakeDamage(enemy.AttackDamage);

                if (Health.CurrentHealth <= 0)
                {
                    // tower broke
                }
            }
        }

        base.Update(gameTime);
    }

    private void OnDeath(Entity diedEntity)
    {
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
        else if (detailsPrompt is not null)
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
        if (detailsClosed && detailsPrompt is null)
        {
            detailsPrompt = new TurretDetailsPrompt(Turret.Game, Turret, UpgradeLeft, UpgradeRight, CurrentUpgrade);
        }

        detailsClosed = false;

        LeftClicked?.Invoke();
    }

    public void OnRightClick()
    {
        RightClicked?.Invoke();
    }

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
        UIComponent.SpawnFlyoutText(costText, costTextPosition, textVelocity, lifetime: 1f,
            color: Color.White);
        ParticleSystem.PlayTowerUpgradeEffect(Turret.Position + Turret.Size / 2);

        return CurrentUpgrade;
    }

    public TowerUpgradeNode? UpgradeRight()
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
        UIComponent.SpawnFlyoutText(costText, costTextPosition, textVelocity, lifetime: 1f,
            color: Color.White);
        ParticleSystem.PlayTowerUpgradeEffect(Turret.Position + Turret.Size / 2);

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
