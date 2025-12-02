using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class Drone : Entity, ITower
{
    private TowerCore towerCore;
    private Vector2 spawnOffset = new (0, 11);
    private Vector2 turretSpawnAxisCenter;
    private static int baseRange = 10;
    int realRange;
    int damage = 10;
    float bulletSpeed = 400f;
    float actionsPerSecond = 2f;
    float actionTimer;
    float sightAngle = 30f;

    public enum Upgrade
    {
        NoUpgrade,
        AdvancedWeaponry,
        FlyingArsenal,
        ImprovedRadar,
        AssassinDrone,
        UAV,
    }

    public Drone(Game game, Vector2 position) : base(game, position, GetUnupgradedBaseAnimationData())
    {
        towerCore = new TowerCore(this);

        var tempIcon = AssetManager.GetTexture("gunTurret_botshot_icon");

        var FlyingArsenal = new TowerUpgradeNode(Upgrade.FlyingArsenal.ToString(), tempIcon, price: 75);
        var AdvancedWeaponry = new TowerUpgradeNode(Upgrade.AdvancedWeaponry.ToString(), tempIcon, price: 25, leftChild: FlyingArsenal);

        var AssassinDrone = new TowerUpgradeNode(Upgrade.AssassinDrone.ToString(), tempIcon, price: 70);
        var UAV = new TowerUpgradeNode(Upgrade.UAV.ToString(), tempIcon, price: 60);
        var ImprovedRadar = new TowerUpgradeNode(Upgrade.ImprovedRadar.ToString(), tempIcon, price: 15, leftChild: AssassinDrone, rightChild: UAV);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0,
            leftChild: AdvancedWeaponry, rightChild: ImprovedRadar);

        AdvancedWeaponry.Description = "+10 damage,\n+ 0.5 shots/s";
        ImprovedRadar.Description = "+8 range";
        FlyingArsenal.Description = "+20 damage,\n+1 shots/s";
        AssassinDrone.Description = "+20 range,\n-35 degrees to sight angle,\n-1 shot/s,\n+100 damage";
        UAV.Description = "-1.5 shots/s\nShoots a radar shot that makes\nenemies take 50% more damage\nfor X seconds. Other towers\nin range gain +4 range.";

        towerCore.CurrentUpgrade = defaultNode;

        realRange = baseRange;
    }

    public override void Update(GameTime gameTime)
    {
        turretSpawnAxisCenter = Position + spawnOffset;
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, sightAngle);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.AdvancedWeaponry.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond + 0.5f, damage + 10, sightAngle);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.FlyingArsenal.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond + 1.5f, damage + 30, sightAngle);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.ImprovedRadar.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage + 8, sightAngle);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.AssassinDrone.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond - 1, damage + 100 + 28, sightAngle - 10f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.UAV.ToString())
        {
            // todo: add the effects
            HandleBasicShots(deltaTime, actionsPerSecond - 1.5f, damage + 8, sightAngle);
        } 

        base.Update(gameTime);
    }

    private void HandleBasicShots(float deltaTime, float actionsPerSecond, int damage, float attackAngle)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        var closestEnemy = GetValidEnemy(realRange, attackAngle);

        if (closestEnemy is null) return;

        if (actionTimer >= actionInterval)
        {
            var enemyCenter = closestEnemy.Position + closestEnemy.Size / 2;
            var direction = enemyCenter - turretSpawnAxisCenter;
            direction.Normalize();
            Shoot(damage, direction);
            actionTimer = 0f;
        }
    }

    private void Shoot(int damage, Vector2 direction)
    {
        var bullet = new Projectile(Game, turretSpawnAxisCenter);
        bullet.Direction = direction;
        bullet.BulletPixelsPerSecond = bulletSpeed;
        bullet.Damage = damage;
        bullet.Lifetime = 1f;
        bullet.BulletLength = 20f;
        bullet.BulletWidth = 8f;
        bullet.Sprite = AssetManager.GetTexture("tempprojectile");
    }

    private Enemy? GetValidEnemy(int tileRange, float attackAngleInDegrees)
    {
        Enemy? closestEnemy = null;
        float closestDistance = float.PositiveInfinity;
        var attackAngleInRadians = (float)(attackAngleInDegrees * Math.PI / 180.0);
        var halfAttackAngle = attackAngleInRadians / 2.0f;

        // Define the direction the tower is facing
        var towerDirectionAngle = (float)Math.PI; // in radians, assuming it faces left.
        var range = tileRange * Grid.TileLength;
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(
            turretSpawnAxisCenter, range);

        foreach (Enemy enemy in enemyCandidates)
        {
            var distanceToEnemy = Vector2.Distance(turretSpawnAxisCenter, enemy.Position);
            if (distanceToEnemy > range)
                continue;
            var enemyCenter = enemy.Position + enemy.Size / 2;
            var deltaX = enemyCenter.X - turretSpawnAxisCenter.X;
            var deltaY = enemyCenter.Y - turretSpawnAxisCenter.Y;
            var enemyAngle = (float)Math.Atan2(deltaY, deltaX);

            var angleDifference = Math.Abs(enemyAngle - towerDirectionAngle);
            // Normalize the angle difference to be within -PI and PI.
            if (angleDifference > Math.PI)
            {
                angleDifference = (float)(2 * Math.PI - angleDifference);
            }

            if (angleDifference > halfAttackAngle)
            {
                continue;
            }
            if (distanceToEnemy < closestDistance)
            {
                var towerCenter = turretSpawnAxisCenter + Size / 2;
                if (Collision.IsLineInTerrain(towerCenter, enemyCenter, out var _, out var _)) continue;
                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }

    public override void Destroy()
    {
        towerCore.CloseDetailsView();
        Game.Components.Remove(towerCore);

        base.Destroy();
    }

    public static AnimationSystem.AnimationData GetUnupgradedBaseAnimationData()
    {
        var sprite = AssetManager.GetTexture("drone_base_idle");

        return new AnimationSystem.AnimationData
        (
            texture: sprite,
            frameCount: 4,
            frameSize: new Vector2(sprite.Width / 4, sprite.Height),
            delaySeconds: 0.1f
        );
    }

    public static List<KeyValuePair<UIEntity, Vector2>> GetUnupgradedPartIcons(List<UIEntity> uiElements)
    {
        var baseSprite = AssetManager.GetTexture("drone_base_idle");

        var baseData = new AnimationSystem.AnimationData
        (
            texture: baseSprite,
            frameCount: 1,
            frameSize: new Vector2(baseSprite.Width / 4, baseSprite.Height),
            delaySeconds: float.PositiveInfinity
        );

        var baseEntity = new UIEntity(Game1.Instance, uiElements, Vector2.Zero, baseData);

        var list = new List<KeyValuePair<UIEntity, Vector2>>();
        list.Add(KeyValuePair.Create(baseEntity, Vector2.Zero));

        return list;
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(2, 3);
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Drone;
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        // todo: improve
        var towerGridSize = GetDefaultGridSize();
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
        var leftTilePosition = targetGridPosition - Vector2.UnitX * Grid.TileLength;
        var rightTilePosition = targetGridPosition + Vector2.UnitX * Grid.TileLength;

        if (Collision.IsPointInTerrain(belowTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        if (Collision.IsPointInTerrain(aboveTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        if (Collision.IsPointInTerrain(leftTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        if (Collision.IsPointInTerrain(rightTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        return true;
    }

    public static Entity CreateNewInstance(Game game, Vector2 worldPosition)
    {
        return new Drone(game, worldPosition);
    }

    public void UpgradeTower(TowerUpgradeNode newUpgrade)
    {
        throw new NotImplementedException();
    }

    public static float GetBaseRange() => baseRange;

    public float GetRange()
    {
        return realRange;
    }
}
