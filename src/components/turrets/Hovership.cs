using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class Hovership : Entity, ITower
{
    private TowerCore towerCore;
    private Vector2 spawnOffset = new (7, 20);
    private Vector2 turretSpawnAxisCenter;
    int baseRange = 10;
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
        AssassinHovership,
        UAV,
    }

    public Hovership(Game game, Vector2 position) : base(game, position, GetTowerAnimationData())
    {
        towerCore = new TowerCore(this);

        var FlyingArsenal = new TowerUpgradeNode(Upgrade.FlyingArsenal.ToString(), price: 75);
        var AdvancedWeaponry = new TowerUpgradeNode(Upgrade.AdvancedWeaponry.ToString(), price: 25, leftChild: FlyingArsenal);

        var AssassinHovership = new TowerUpgradeNode(Upgrade.AssassinHovership.ToString(), price: 70);
        var UAV = new TowerUpgradeNode(Upgrade.UAV.ToString(), price: 60);
        var ImprovedRadar = new TowerUpgradeNode(Upgrade.ImprovedRadar.ToString(), price: 15, leftChild: AssassinHovership, rightChild: UAV);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), price: 0,
            leftChild: AdvancedWeaponry, rightChild: ImprovedRadar);

        towerCore.CurrentUpgrade = defaultNode;
    }

    public override void Update(GameTime gameTime)
    {
        turretSpawnAxisCenter = Position + spawnOffset;
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, baseRange, sightAngle);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.AdvancedWeaponry.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond + 0.5f, damage + 10, baseRange, sightAngle);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.FlyingArsenal.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond + 1.5f, damage + 30, baseRange, sightAngle);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.ImprovedRadar.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, baseRange + 8, sightAngle);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.AssassinHovership.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond - 1, damage + 100, baseRange + 28, sightAngle - 10f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.UAV.ToString())
        {
            // todo: add the effects
            HandleBasicShots(deltaTime, actionsPerSecond - 1.5f, damage, baseRange + 8, sightAngle);
        } 

        base.Update(gameTime);
    }

    private void HandleBasicShots(float deltaTime, float actionsPerSecond, int damage, int range, float attackAngle)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        var closestEnemy = GetValidEnemy(range, attackAngle);

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
        var towerDirectionAngle = (float)Math.PI/2f; // in radians, assuming it faces down.
        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            var distanceToEnemy = Vector2.Distance(turretSpawnAxisCenter, enemy.Position);
            if (distanceToEnemy > tileRange * Grid.TileLength)
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

    public static AnimationSystem.AnimationData GetTowerAnimationData()
    {
        var sprite = AssetManager.GetTexture("drone");

        return new AnimationSystem.AnimationData
        (
            texture: sprite,
            frameCount: 1,
            frameSize: new Vector2(sprite.Width / 4, sprite.Height),
            delaySeconds: 0
        );
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(2, 3);
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Hovership;
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
        return new Hovership(game, worldPosition);
    }
}
