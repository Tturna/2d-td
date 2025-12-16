using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
class Drone : Entity, ITower
{
    private TowerCore towerCore;
    private static int baseTileRange = 10;
    private int realTileRange;
    private int damage = 10;
    private float bulletSpeed = 400f;
    private float actionsPerSecond = 2f;
    private float actionTimer;
    private float firingSectorWidthDegrees = 45f;
    private float firingDirectionDegrees = 180;
    private Vector2 turretAxisCenter;
    private float muzzleOffsetMagnitude = 8;
    private Texture2D projectileSprite = AssetManager.GetTexture("gunTurret_base_bullet");
    private Texture2D? gunSprite = AssetManager.GetTexture("drone_base_gun");
    private Vector2 gunSpriteOrigin;

    public enum Upgrade
    {
        NoUpgrade,
        AdvancedWeaponry,
        Artificer,
        FlyingArsenal,
        ImprovedRadar,
        AssassinDrone,
        Quadcopter,
    }

    public Drone(Game game, Vector2 position) : base(game, position, GetUnupgradedBaseAnimationData())
    {
        towerCore = new TowerCore(this);

        var tempIcon = AssetManager.GetTexture("gunTurret_botshot_icon");

        var flyingArsenal = new TowerUpgradeNode(Upgrade.FlyingArsenal.ToString(), tempIcon, price: 220);
        var quadcopter = new TowerUpgradeNode(Upgrade.Quadcopter.ToString(), tempIcon, price: 180);
        var advancedWeaponry = new TowerUpgradeNode(Upgrade.AdvancedWeaponry.ToString(), tempIcon,
            price: 30, leftChild: flyingArsenal, rightChild: quadcopter);

        var assassinDrone = new TowerUpgradeNode(Upgrade.AssassinDrone.ToString(), tempIcon, price: 140);
        var artificer = new TowerUpgradeNode(Upgrade.Artificer.ToString(), tempIcon, price: 160);
        var ImprovedRadar = new TowerUpgradeNode(Upgrade.ImprovedRadar.ToString(), tempIcon, price: 15,
            leftChild: assassinDrone, rightChild: artificer);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0,
            leftChild: advancedWeaponry, rightChild: ImprovedRadar);

        advancedWeaponry.Description = "+10 damage,\n+ 0.5 shots/s";
        ImprovedRadar.Description = "+8 range";
        flyingArsenal.Description = "+50 damage,\n+1 shots/s";
        quadcopter.Description = "10 shots/s,\n120 degree sight angle\ndirectly downwards";
        assassinDrone.Description = "+20 range,\n-35 degrees to sight angle,\n-1 shot/s,\n+200 damage";
        artificer.Description = "-4 range, 0.5 shots/s\nAttack replaced with an energy pulse\nthat deals 20 damage and\nheals towers";

        towerCore.CurrentUpgrade = defaultNode;

        realTileRange = baseTileRange;
        turretAxisCenter = Position + new Vector2(-1, Size.Y / 2);
        gunSpriteOrigin = new Vector2(gunSprite.Width, gunSprite.Height / 2);
    }

    // TODO: Handle upgraded stats sensibly by using variables and updating them in
    // UpgradeTower().
    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.AdvancedWeaponry.ToString())
        {
            HandleBasicShots(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.FlyingArsenal.ToString())
        {
            HandleBasicShots(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Quadcopter.ToString())
        {
            HandleBasicShots(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.ImprovedRadar.ToString())
        {
            HandleBasicShots(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.AssassinDrone.ToString())
        {
            HandleBasicShots(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Artificer.ToString())
        {
            HandleArtificer(deltaTime);
        } 

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        towerCore.Health.DrawHealthBar(Position + new Vector2(Size.X / 2, -4));

        if (gunSprite is not null)
        {
            Game.SpriteBatch.Draw(gunSprite,
                turretAxisCenter,
                sourceRectangle: null,
                Color.White,
                rotation: 0,
                origin: gunSpriteOrigin,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: DrawLayerDepth);
        }

        base.Draw(gameTime);
    }

    private void HandleBasicShots(float deltaTime)
    {
        var actionInterval = 1f / actionsPerSecond;
        actionTimer += deltaTime;

        if (actionTimer <= actionInterval) return;

        var closestEnemy = GetEnemyInFiringSector();

        if (closestEnemy is null) return;

        var enemyCenter = closestEnemy.Position + closestEnemy.Size / 2;
        var direction = enemyCenter - turretAxisCenter;
        direction.Normalize();
        Shoot(damage, direction);
        actionTimer = 0f;
    }

    private void HandleArtificer(float deltaTime)
    {
        var actionInterval = 1f / actionsPerSecond;
        actionTimer += deltaTime;

        if (actionTimer <= actionInterval) return;
        if (WaveSystem.WaveCooldownLeft > 0) return;


    }

    private void Shoot(int damage, Vector2 direction)
    {
        var bullet = new Projectile(Game, this, turretAxisCenter);
        bullet.Direction = direction;
        bullet.BulletPixelsPerSecond = bulletSpeed;
        bullet.Damage = damage;
        bullet.Lifetime = 1f;
        bullet.BulletLength = 20f;
        bullet.BulletWidth = 8f;
        bullet.Sprite = projectileSprite;
    }

    private Enemy? GetEnemyInFiringSector()
    {
        Enemy? closestEnemy = null;
        float closestDistance = float.PositiveInfinity;
        var firingSectorWidthRadians = MathHelper.ToRadians(firingSectorWidthDegrees);
        var halfFiringSectorWidth = firingSectorWidthRadians / 2.0f;

        var firingDirectionRadians = MathHelper.ToRadians(firingDirectionDegrees);
        var range = realTileRange * Grid.TileLength;
        var droneCenter = Position + Size / 2;
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(droneCenter, range);

        // debug lines
        // TODO: Probably should draw these as indicators when opening tower details
        // var firstEndPointDirection = new Vector2(MathF.Cos(firingDirectionRadians + halfFiringSectorWidth), MathF.Sin(firingDirectionRadians + halfFiringSectorWidth));
        // var firstEndPoint = droneCenter + firstEndPointDirection * range;
        // var secondEndPointDirection = new Vector2(MathF.Cos(firingDirectionRadians - halfFiringSectorWidth), MathF.Sin(firingDirectionRadians - halfFiringSectorWidth));
        // var secondEndPoint = droneCenter + secondEndPointDirection * range;
        // DebugUtility.DrawDebugLine(droneCenter, firstEndPoint, Color.Lime);
        // DebugUtility.DrawDebugLine(droneCenter, secondEndPoint, Color.Lime);

        foreach (Enemy enemy in enemyCandidates)
        {
            var distanceToEnemy = Vector2.Distance(droneCenter, enemy.Position);

            if (distanceToEnemy > range) continue;

            var enemyCenter = enemy.Position + enemy.Size / 2;
            var deltaX = enemyCenter.X - droneCenter.X;
            var deltaY = enemyCenter.Y - droneCenter.Y;
            var enemyAngle = (float)Math.Atan2(deltaY, deltaX);
            var angleDifference = Math.Abs(enemyAngle - firingDirectionRadians);

            // Normalize the angle difference to be within -PI and PI.
            if (angleDifference > Math.PI)
            {
                angleDifference = (float)(2 * Math.PI - angleDifference);
            }

            if (angleDifference > halfFiringSectorWidth) continue;

            if (distanceToEnemy < closestDistance)
            {
                if (Collision.IsLineInTerrain(droneCenter, enemyCenter, out var _, out var _)) continue;

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
        Texture2D newIdleTexture;
        var newIdleFrameCount = 1;

        if (newUpgrade.Name == Upgrade.AdvancedWeaponry.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("drone_advancedweaponry_idle");
            gunSprite = AssetManager.GetTexture("drone_advancedweaponry_gun");
            newIdleFrameCount = 4;
            damage += 10;
            actionsPerSecond += 0.5f;
        }
        else if (newUpgrade.Name == Upgrade.Artificer.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("drone_artificer_idle");
            var attackTexture = AssetManager.GetTexture("drone_artificer_attack");
            var explosionTexture = AssetManager.GetTexture("drone_artificer_explosion");
            newIdleFrameCount = 4;
            gunSprite = null;
            realTileRange -= 4;
            actionsPerSecond = 0.5f;
            damage = 20;
        }
        else if (newUpgrade.Name == Upgrade.AssassinDrone.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("drone_assassindrone_idle");
            gunSprite = AssetManager.GetTexture("drone_assassindrone_gun");
            newIdleFrameCount = 4;
            projectileSprite = AssetManager.GetTexture("drone_assassindrone_bullet");

            realTileRange += 20;
            actionsPerSecond -= 1;
            damage = 200;
            firingSectorWidthDegrees -= 35;
        }
        else if (newUpgrade.Name == Upgrade.FlyingArsenal.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("drone_flyingarsenal_idle");
            gunSprite = AssetManager.GetTexture("drone_flyingarsenal_gun");
            newIdleFrameCount = 4;
            damage += 50;
            actionsPerSecond += 1;
        }
        else if (newUpgrade.Name == Upgrade.ImprovedRadar.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("drone_improvedradar_idle");
            newIdleFrameCount = 4;
            realTileRange += 8;
        }
        else
        {
            newIdleTexture = AssetManager.GetTexture("drone_quadcopter_idle");
            gunSprite = AssetManager.GetTexture("drone_quadcopter_gun");
            newIdleFrameCount = 2;
            turretAxisCenter = Position + new Vector2(newIdleTexture.Width / newIdleFrameCount / 2, newIdleTexture.Height + 1);
            gunSpriteOrigin = new Vector2(gunSprite.Width / 2, 0);
            actionsPerSecond += 10;
            firingDirectionDegrees = 90;
            firingSectorWidthDegrees = 120;
        }

        var newIdleAnimation = new AnimationSystem.AnimationData
        (
            texture: newIdleTexture,
            frameCount: newIdleFrameCount,
            frameSize: new Vector2(newIdleTexture.Width / newIdleFrameCount, newIdleTexture.Height),
            delaySeconds: 0.1f
        );

        AnimationSystem!.ChangeAnimationState(null, newIdleAnimation);
        Size = newIdleAnimation.FrameSize;
    }

    public static float GetBaseRange() => baseTileRange;

    public float GetRange()
    {
        return realTileRange;
    }

    public TowerCore GetTowerCore() => towerCore;
}
