using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
class Hovership : Entity, ITower
{
    private TowerCore towerCore;
    private Vector2 spawnOffset = new (0, 0);
    private Entity turretHovership;
    private Vector2 turretSpawnAxisCenter;
    private static int baseHovershipHangarRange = 25;
    int realHovershipHangarRange;
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
        UFO
    }

    public Hovership(Game game, Vector2 position) : base(game, position, GetUnupgradedPlatformAnimation())
    {
        towerCore = new TowerCore(this);

        var tempIcon = AssetManager.GetTexture("gunTurret_botshot_icon");

        var OrbitalLaser = new TowerUpgradeNode(Upgrade.OrbitalLaser.ToString(), tempIcon, price: 160);
        var CarpetofFire = new TowerUpgradeNode(Upgrade.CarpetofFire.ToString(), tempIcon, price: 120);
        var BombierBay = new TowerUpgradeNode(Upgrade.BombierBay.ToString(), tempIcon, price: 40, leftChild: OrbitalLaser, rightChild: CarpetofFire);

        var EMPShip = new TowerUpgradeNode(Upgrade.EMPShip.ToString(), tempIcon, price: 110);
        var UFO = new TowerUpgradeNode(Upgrade.UFO.ToString(), tempIcon, price: 200);
        var EfficientEngines = new TowerUpgradeNode(Upgrade.EfficientEngines.ToString(), tempIcon, price: 15, leftChild: EMPShip, rightChild: UFO);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0,
            leftChild: BombierBay, rightChild: EfficientEngines);

        BombierBay.Description = "+2 projectiles";
        EfficientEngines.Description = "+10 tile range";
        OrbitalLaser.Description = "-0.85 shots/s\nInstead of bombs,\nfires a massive orbital laser\nthat deals 300 damage\nover 4s.\nUnlimited pierce";
        CarpetofFire.Description = "+3 projectiles.\nProjectiles inflict 1 burn\nstack and leave fire tiles\non the ground that\ndeal 10 DPS for 5s.";
        EMPShip.Description = "-2 projectiles.\n+10 tile hover height.\n+5 tile area of effect\nNow Shocks enemies for 5s.";
        UFO.Description = "Sucks up to 5 bots up toward it and\ndrops them back at the entrance.\nWhile held, they take 10 DPS.";

        towerCore.CurrentUpgrade = defaultNode;

        turretHovership = new Entity(Game, position, GetUnupgradedBaseAnimationData());
        turretHovership.DrawLayerDepth = 0.8f;

        UpdatePosition(Vector2.UnitY * Grid.TileLength);

        realHovershipHangarRange = baseHovershipHangarRange;
    }

    // TODO: Handle upgraded stats sensibly by using variables and updating them in
    // UpgradeTower().
    public override void Update(GameTime gameTime)
    {
        turretSpawnAxisCenter = turretHovership.Position + spawnOffset;
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // TODO: add the effects for a lot of these upgrades
        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, hoverHeight, baseProjectileAmount);
            HandleHovershipPosition(deltaTime, hoverHeight);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.BombierBay.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, hoverHeight, baseProjectileAmount);
            HandleHovershipPosition(deltaTime, hoverHeight);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.OrbitalLaser.ToString())
        {

        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.CarpetofFire.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, hoverHeight, baseProjectileAmount);
            HandleHovershipPosition(deltaTime, hoverHeight);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.EfficientEngines.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, hoverHeight, baseProjectileAmount);
            HandleHovershipPosition(deltaTime, hoverHeight);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.EMPShip.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, hoverHeight, baseProjectileAmount);
            HandleHovershipPosition(deltaTime, hoverHeight);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.UFO.ToString())
        {

        } 

        base.Update(gameTime);
    }

    private void HandleHovershipPosition(float deltaTime, int hoverHeight)
    {
        var closestEnemy = towerCore.GetClosestValidEnemy(realHovershipHangarRange);

        if (closestEnemy is null)
        {
            return;
        }

        var target = closestEnemy.Position - Vector2.UnitY * (hoverHeight-2) * Grid.TileLength;
        var difference = target - turretHovership.Position;
        difference.Normalize();

        turretHovership.UpdatePosition(difference * hovershipSpeed * deltaTime);
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
        turretHovership?.Destroy();
        Game.Components.Remove(towerCore);

        base.Destroy();
    }

    public static AnimationSystem.AnimationData GetUnupgradedBaseAnimationData()
    {
        var sprite = AssetManager.GetTexture("hovership_base_idle");

        return new AnimationSystem.AnimationData
        (
            texture: sprite,
            frameCount: 3,
            frameSize: new Vector2(sprite.Width / 3, sprite.Height),
            delaySeconds: 0.1f
        );
    }

    public static List<KeyValuePair<UIEntity, Vector2>> GetUnupgradedPartIcons(List<UIEntity> uiElements)
    {
        var baseSprite = AssetManager.GetTexture("hovership_base_idle");

        var baseData = new AnimationSystem.AnimationData
        (
            texture: baseSprite,
            frameCount: 1,
            frameSize: new Vector2(baseSprite.Width / 3, baseSprite.Height),
            delaySeconds: float.PositiveInfinity
        );

        var baseEntity = new UIEntity(Game1.Instance, uiElements, Vector2.Zero, baseData);

        var list = new List<KeyValuePair<UIEntity, Vector2>>();
        list.Add(KeyValuePair.Create(baseEntity, Vector2.Zero));

        return list;
    }

    private static AnimationSystem.AnimationData GetUnupgradedPlatformAnimation()
    {
        var sprite = AssetManager.GetTexture("hovership_base_platform");

        return new AnimationSystem.AnimationData(
            texture: sprite,
            frameCount: 2,
            frameSize: new Vector2(sprite.Width / 2, sprite.Height),
            delaySeconds: 0.5f);
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(4, 2);
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
        Texture2D newIdleTexture;
        Texture2D newPlatformTexture;
        var newIdleFrameCount = 1;
        var newPlatformFrameCount = 1;

        if (newUpgrade.Name == Upgrade.BombierBay.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("hovership_bombierbay_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_bombierbay_platform");
            newIdleFrameCount = 3;
            newPlatformFrameCount = 2;
            // offset platform because its sprite changes size
            UpdatePosition(-Vector2.UnitY * 2);
        }
        else if (newUpgrade.Name == Upgrade.EfficientEngines.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("hovership_efficientengines_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_efficientengines_platform");
            newIdleFrameCount = 3;
            newPlatformFrameCount = 2;
            UpdatePosition(-Vector2.UnitY * 2);
        }
        else if (newUpgrade.Name == Upgrade.OrbitalLaser.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("hovership_orbitallaser_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_orbitallaser_platform");
            newIdleFrameCount = 8;
            newPlatformFrameCount = 2;
            UpdatePosition(-Vector2.UnitY * 8);
        }
        else if (newUpgrade.Name == Upgrade.CarpetofFire.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("hovership_carpetoffire_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_carpetoffire_platform");
            newIdleFrameCount = 4;
            newPlatformFrameCount = 2;
            UpdatePosition(-Vector2.UnitY);
        }
        else if (newUpgrade.Name == Upgrade.EMPShip.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("hovership_emp_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_emp_platform");
            newIdleFrameCount = 4;
            newPlatformFrameCount = 2;
            UpdatePosition(-Vector2.UnitY * 4);
        }
        else
        {
            newIdleTexture = AssetManager.GetTexture("hovership_ufo_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_ufo_platform");
            newIdleFrameCount = 3;
            newPlatformFrameCount = 2;
            UpdatePosition(Vector2.UnitY * 5);
        }

        var newIdleAnimation = new AnimationSystem.AnimationData
        (
            texture: newIdleTexture,
            frameCount: newIdleFrameCount,
            frameSize: new Vector2(newIdleTexture.Width / newIdleFrameCount, newIdleTexture.Height),
            delaySeconds: 0.1f
        );

        var newPlatformAnimation = new AnimationSystem.AnimationData
        (
            texture: newPlatformTexture,
            frameCount: newPlatformFrameCount,
            frameSize: new Vector2(newPlatformTexture.Width / newPlatformFrameCount, newPlatformTexture.Height),
            delaySeconds: 0.5f
        );

        turretHovership!.AnimationSystem!.ChangeAnimationState(null, newIdleAnimation);
        AnimationSystem!.ChangeAnimationState(null, newPlatformAnimation);

    }

    public static float GetBaseRange() => baseHovershipHangarRange;

    public float GetRange()
    {
        return realHovershipHangarRange;
    }
}
