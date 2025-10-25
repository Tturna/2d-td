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
    int baseHovershipHangarRange = 25;
    int hoverHeight = 10;
    int damage = 15;
    int baseProjectileAmount = 3;
    float hovershipSpeed = 50f;
    float bulletSpeed = 400f;
    float actionsPerSecond = 1f;
    float actionTimer;
    float sightAngle = 30f;

    private Random random = new();

    public enum Upgrade
    {
        NoUpgrade,
        BombierBay,
        OrbitalLaser,
        CarpetofFire,
        EfficientEngines,
        EMPShip,
        FloatingFactory
    }

    public Hovership(Game game, Vector2 position) : base(game, position, GetTowerBaseAnimationData())
    {
        towerCore = new TowerCore(this);

        var tempIcon = AssetManager.GetTexture("gunTurret_botshot_icon");

        var OrbitalLaser = new TowerUpgradeNode(Upgrade.OrbitalLaser.ToString(), tempIcon, price: 160);
        var CarpetofFire = new TowerUpgradeNode(Upgrade.CarpetofFire.ToString(), tempIcon, price: 120);
        var BombierBay = new TowerUpgradeNode(Upgrade.BombierBay.ToString(), tempIcon, price: 40, leftChild: OrbitalLaser, rightChild: CarpetofFire);

        var EMPShip = new TowerUpgradeNode(Upgrade.EMPShip.ToString(), tempIcon, price: 110);
        var FloatingFactory = new TowerUpgradeNode(Upgrade.FloatingFactory.ToString(), tempIcon, price: 150);
        var EfficientEngines = new TowerUpgradeNode(Upgrade.EfficientEngines.ToString(), tempIcon, price: 15, leftChild: EMPShip, rightChild: FloatingFactory);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0,
            leftChild: BombierBay, rightChild: EfficientEngines);

        BombierBay.Description = "+2 projectiles";
        EfficientEngines.Description = "+10 tile range";
        OrbitalLaser.Description = "-0.85 shots/s\nInstead of bombs,\nfires a massive orbital laser\nthat deals 300 damage\nover 4s.\nUnlimited pierce";
        CarpetofFire.Description = "+3 projectiles.\nProjectiles inflict 1 burn\nstack and leave fire tiles\non the ground that\ndeal 10 DPS for 5s.";
        EMPShip.Description = "-2 projectiles.\n+10 tile hover height.\n+5 tile area of effect\nNow Shocks enemies for 5s.";
        FloatingFactory.Description = "Drops 2 ground troops which run\nat enemies, stalling them and\nattacking for 20 DPS.\nTroops drop worthless scrap on death.";

        towerCore.CurrentUpgrade = defaultNode;

        turretHovership = new Entity(Game, position, AssetManager.GetTexture("gunTurretHead"));
        turretHovership.DrawLayerDepth = 0.8f;
    }

    public override void Update(GameTime gameTime)
    {
        turretSpawnAxisCenter = turretHovership.Position + spawnOffset;
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // todo: add the effects for a lot of these upgrades
        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, hoverHeight, baseProjectileAmount);
            HandleHovershipPosition(deltaTime, baseHovershipHangarRange, hoverHeight);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.BombierBay.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, hoverHeight, baseProjectileAmount + 2);
            HandleHovershipPosition(deltaTime, baseHovershipHangarRange, hoverHeight);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.OrbitalLaser.ToString())
        {

        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.CarpetofFire.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, hoverHeight, baseProjectileAmount + 5);
            HandleHovershipPosition(deltaTime, baseHovershipHangarRange, hoverHeight);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.EfficientEngines.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, hoverHeight, baseProjectileAmount);
            HandleHovershipPosition(deltaTime, baseHovershipHangarRange + 10, hoverHeight);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.EMPShip.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, hoverHeight, baseProjectileAmount - 2);
            HandleHovershipPosition(deltaTime, baseHovershipHangarRange + 15, hoverHeight + 10);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.FloatingFactory.ToString())
        {

        } 

        base.Update(gameTime);
    }

    private void HandleHovershipPosition(float deltaTime, int tileRange, int hoverHeight)
    {
        var closestEnemy = towerCore.GetClosestValidEnemy(tileRange);

        if (closestEnemy is null) return;

        var target = closestEnemy.Position - Vector2.UnitY * (hoverHeight-2) * Grid.TileLength;
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
                var randomXDirection = random.Next(-12, 12);
                var randomY = random.Next(-12, 12);
                var targetDirection = enemyDirection + new Vector2(randomXDirection, randomY);
                targetDirection.Normalize();

                var randomXPosition = random.Next(-8, 8);
                var newPosition = turretSpawnAxisCenter + Vector2.UnitX * randomXPosition;
                Shoot(damage, targetDirection, newPosition);
                actionTimer = 0f;
            }
        }
    }

    private void Shoot(int damage, Vector2 direction, Vector2 position)
    {
        var bullet = new Projectile(Game, position);
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

    public static AnimationSystem.AnimationData GetTowerBaseAnimationData()
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

    public void UpgradeTower(TowerUpgradeNode newUpgrade)
    {
    }
}
