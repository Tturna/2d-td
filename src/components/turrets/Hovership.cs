using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class Hovership : Entity, ITower
{
    private TowerCore towerCore;
    private Vector2 spawnOffset = new (0, 0);
    private Entity turretHovership;
    private Vector2 turretSpawnAxisCenter;
    int baseRange = 25;
    int damage = 15;
    float hovershipSpeed = 50f;
    float bulletSpeed = 400f;
    float actionsPerSecond = 1f;
    float actionTimer;
    float sightAngle = 30f;

    private Random random = new();

    public enum Upgrade
    {
        NoUpgrade,
        // AdvancedWeaponry,
        // FlyingArsenal,
        // ImprovedRadar,
        // AssassinHovership,
        // UAV,
    }

    public Hovership(Game game, Vector2 position) : base(game, position, GetTowerAnimationData())
    {
        towerCore = new TowerCore(this);

        // var FlyingArsenal = new TowerUpgradeNode(Upgrade.FlyingArsenal.ToString(), price: 75);
        // var AdvancedWeaponry = new TowerUpgradeNode(Upgrade.AdvancedWeaponry.ToString(), price: 25, leftChild: FlyingArsenal);

        // var AssassinHovership = new TowerUpgradeNode(Upgrade.AssassinHovership.ToString(), price: 70);
        // var UAV = new TowerUpgradeNode(Upgrade.UAV.ToString(), price: 60);
        // var ImprovedRadar = new TowerUpgradeNode(Upgrade.ImprovedRadar.ToString(), price: 15, leftChild: AssassinHovership, rightChild: UAV);

        // var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), price: 0,
        //     leftChild: AdvancedWeaponry, rightChild: ImprovedRadar);

        // towerCore.CurrentUpgrade = defaultNode;

        turretHovership = new Entity(Game, position, AssetManager.GetTexture("gunTurretHead"));
        turretHovership.DrawLayerDepth = 0.8f;
    }

    public override void Update(GameTime gameTime)
    {
        turretSpawnAxisCenter = turretHovership.Position + spawnOffset;
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        
        // if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        // {
        HandleBasicShots(deltaTime, actionsPerSecond, damage, baseRange, 3);
        HandleHovershipPosition(deltaTime, 25);
        // }
        // else if (towerCore.CurrentUpgrade.Name == Upgrade.AdvancedWeaponry.ToString())
        // {
        //     HandleBasicShots(deltaTime, actionsPerSecond + 0.5f, damage + 10, baseRange, sightAngle);
        // }
        // else if (towerCore.CurrentUpgrade.Name == Upgrade.FlyingArsenal.ToString())
        // {
        //     HandleBasicShots(deltaTime, actionsPerSecond + 1.5f, damage + 30, baseRange, sightAngle);
        // }
        // else if (towerCore.CurrentUpgrade.Name == Upgrade.ImprovedRadar.ToString())
        // {
        //     HandleBasicShots(deltaTime, actionsPerSecond, damage, baseRange + 8, sightAngle);
        // }
        // else if (towerCore.CurrentUpgrade.Name == Upgrade.AssassinHovership.ToString())
        // {
        //     HandleBasicShots(deltaTime, actionsPerSecond - 1, damage + 100, baseRange + 28, sightAngle - 10f);
        // }
        // else if (towerCore.CurrentUpgrade.Name == Upgrade.UAV.ToString())
        // {
        //     // todo: add the effects
        //     HandleBasicShots(deltaTime, actionsPerSecond - 1.5f, damage, baseRange + 8, sightAngle);
        // } 

        base.Update(gameTime);
    }

    private void HandleHovershipPosition(float deltaTime, int tileRange)
    {
        var closestEnemy = towerCore.GetClosestValidEnemy(tileRange);

        if (closestEnemy is null) return;

        var target = closestEnemy.Position - Vector2.UnitY * 10 * Grid.TileLength;
        var difference = target - turretHovership.Position;
        difference.Normalize();

        turretHovership.Position += difference * hovershipSpeed * deltaTime;
        turretSpawnAxisCenter = turretHovership.Position + spawnOffset;
    }

    private void HandleBasicShots(float deltaTime, float actionsPerSecond, int damage, int range, int projectileAmount)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        var closestEnemy = GetValidEnemy(range, sightAngle);

        if (closestEnemy is null) return;


        if (actionTimer >= actionInterval)
        {
            for (int i = 0; i < projectileAmount; i++)
            {
                var enemyCenter = closestEnemy.Position + closestEnemy.Size / 2;
                var enemyDirection = enemyCenter - turretSpawnAxisCenter;
                var randomX = random.Next(-3, 3);
                var randomY = random.Next(-3, 3);
                var targetDirection = enemyDirection + new Vector2(randomX, randomY);
                targetDirection.Normalize();
                Shoot(damage, targetDirection);
                actionTimer = 0f;
            }
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
        bullet.ExplosionTileRadius = 4;
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
        turretHovership?.Destroy();
        Game.Components.Remove(towerCore);

        base.Destroy();
    }

    public static AnimationSystem.AnimationData GetTowerAnimationData()
    {
        var sprite = AssetManager.GetTexture("gunTurretBase");

        return new AnimationSystem.AnimationData
        (
            texture: sprite,
            frameCount: 1,
            frameSize: new Vector2(sprite.Width, sprite.Height),
            delaySeconds: 0
        );
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(2, 2);
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Hovership;
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        return TowerCore.DefaultCanPlaceTower(GetDefaultGridSize(), targetWorldPosition);
    }

    public static Entity CreateNewInstance(Game game, Vector2 worldPosition)
    {
        return new Hovership(game, worldPosition);
    }
}
