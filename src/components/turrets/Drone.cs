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
    private static float baseFiringSectorWidthDegrees = 45f;
    private static float baseFiringDirectionDegrees = 180;
    private float firingSectorWidthDegrees;
    private float firingDirectionDegrees;
    private Vector2 turretAxisCenter;
    private float muzzleOffsetMagnitude = 8;
    private Texture2D projectileSprite = AssetManager.GetTexture("gunTurret_base_bullet");
    private Texture2D? gunSprite = AssetManager.GetTexture("drone_base_gun");
    private Vector2 gunSpriteOrigin;

    private Entity? artificerExplosion;
    private float artificerExplosionTimer;
    private float artificerExplosionDelayTimer;

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

        var flyingArsenalIcon = AssetManager.GetTexture("drone_flyingarsenal_icon");
        var quadcopterIcon = AssetManager.GetTexture("drone_quadcopter_icon");
        var advancedWeaponryIcon = AssetManager.GetTexture("drone_advancedweaponry_icon");
        var assassinDroneIcon = AssetManager.GetTexture("drone_assassindrone_icon");
        var artificerIcon = AssetManager.GetTexture("drone_artificer_icon");
        var improvedRadarIcon = AssetManager.GetTexture("drone_improvedradar_icon");

        var flyingArsenal = new TowerUpgradeNode(Upgrade.FlyingArsenal.ToString(), flyingArsenalIcon, price: 220);
        var quadcopter = new TowerUpgradeNode(Upgrade.Quadcopter.ToString(), quadcopterIcon, price: 180);
        var advancedWeaponry = new TowerUpgradeNode(Upgrade.AdvancedWeaponry.ToString(), advancedWeaponryIcon,
            price: 30, leftChild: flyingArsenal, rightChild: quadcopter);

        var assassinDrone = new TowerUpgradeNode(Upgrade.AssassinDrone.ToString(), assassinDroneIcon, price: 140);
        var artificer = new TowerUpgradeNode(Upgrade.Artificer.ToString(), artificerIcon, price: 160);
        var improvedRadar = new TowerUpgradeNode(Upgrade.ImprovedRadar.ToString(), improvedRadarIcon, price: 15,
            leftChild: assassinDrone, rightChild: artificer);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0,
            leftChild: advancedWeaponry, rightChild: improvedRadar);

        advancedWeaponry.Description = "+10 damage,\n+ 0.5 shots/s";
        improvedRadar.Description = "+8 range";
        flyingArsenal.Description = "+50 damage,\n+1 shots/s";
        quadcopter.Description = "10 shots/s,\n120 degree sight angle\ndirectly downwards";
        assassinDrone.Description = "+20 range,\n-35 degrees to sight angle,\n-1 shot/s,\n+200 damage";
        artificer.Description = "-4 range, 0.5 shots/s\nAttack replaced with an energy pulse\nthat deals 20 damage and\nheals towers";

        towerCore.CurrentUpgrade = defaultNode;

        realTileRange = baseTileRange;
        turretAxisCenter = Position + new Vector2(-1, Size.Y / 2);
        gunSpriteOrigin = new Vector2(gunSprite.Width, gunSprite.Height / 2);

        firingSectorWidthDegrees = baseFiringSectorWidthDegrees;
        firingDirectionDegrees = baseFiringDirectionDegrees;
    }

    public override void Update(GameTime gameTime)
    {
        if (towerCore.Health.CurrentHealth <= 0) return;

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

        if (!towerCore.OffsetDrawing)
        {
            DrawOrigin = AnimationSystem!.CurrentAnimationData.FrameSize / 2;
            DrawOffset = AnimationSystem!.BaseAnimationData.FrameSize / 2;
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

        if (artificerExplosionTimer > 0)
        {
            var animData = artificerExplosion!.AnimationSystem!.BaseAnimationData;
            var totalLifetime = animData.FrameCount * animData.DelaySeconds;
            var normalReverseLifetime = artificerExplosionTimer / totalLifetime;
            var x = 1f - MathF.Pow(1f - normalReverseLifetime, 3f); // ease out cubic
            var color = Color.FromNonPremultiplied(new Vector4(1f, 1f, 1f, x));
            artificerExplosion.Color = color;
            artificerExplosionTimer -= deltaTime;

            if (artificerExplosionTimer <= 0)
            {
                artificerExplosion?.Destroy();
                artificerExplosion = null;
            }
        }

        if (artificerExplosionDelayTimer > 0)
        {
            artificerExplosionDelayTimer -= deltaTime;

            if (artificerExplosionDelayTimer <= 0)
            {
                var explosionTexture = AssetManager.GetTexture("drone_artificer_explosion");

                var explosionAnimation = new AnimationSystem.AnimationData(
                        texture: explosionTexture,
                        frameCount: 5,
                        frameSize: new Vector2(explosionTexture.Width / 5, explosionTexture.Height),
                        delaySeconds: 0.075f);

                var explosionSize = explosionAnimation.FrameSize * 1.5f;
                artificerExplosion = new Entity(Game, Position + Size / 2 - explosionSize / 2, explosionAnimation);
                artificerExplosion.Scale = Vector2.One * 1.5f;
                artificerExplosionTimer = explosionAnimation.FrameCount * explosionAnimation.DelaySeconds;

                var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(Position + Size / 2, realTileRange * Grid.TileLength);

                foreach (var enemy in enemyCandidates)
                {
                    var distance = Vector2.Distance(Position + Size / 2, enemy.Position + enemy.Size / 2);

                    if (distance > realTileRange * Grid.TileLength) continue;

                    enemy.HealthSystem.TakeDamage(this, damage);
                }

                foreach (var tower in BuildingSystem.Towers)
                {
                    var distance = Vector2.Distance(Position + Size / 2, tower.Position + tower.Size / 2);

                    if (distance > realTileRange * Grid.TileLength) continue;

                    var core = ((ITower)tower).GetTowerCore();
                    core.Health.Heal(damage);
                }
            }

            return;
        }

        if (actionTimer <= actionInterval) return;
        if (WaveSystem.WaveCooldownLeft > 0) return;

        actionTimer = 0f;
        artificerExplosionDelayTimer = 0.15f;
        AnimationSystem!.OneShotAnimationState("artificerAttack");
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
        SoundSystem.PlaySound("shoot");
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
        artificerExplosion?.Destroy();

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

                foreach (var tower in BuildingSystem.Towers)
                {
                    if (Collision.IsPointInEntity(position, tower))
                    {
                        return false;
                    }
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
            newIdleFrameCount = 4;
            gunSprite = null;
            realTileRange -= 4;
            actionsPerSecond = 0.5f;
            damage = 20;

            var artificerAttackTexture = AssetManager.GetTexture("drone_artificer_attack");
            
            var attackAnimation = new AnimationSystem.AnimationData(
                texture: artificerAttackTexture,
                frameCount: 8,
                frameSize: new Vector2(artificerAttackTexture.Width / 8, artificerAttackTexture.Height),
                delaySeconds: 0.05f);

            AnimationSystem!.AddAnimationState("artificerAttack", attackAnimation);

            // Disable drawing the tower with anchor at bottom left so it can be manually
            // offset to anchor at center. This way the artificer attack doesn't jerk the tower.
            towerCore.OffsetDrawing = false;
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

    public static void DrawBaseRangeIndicator(Vector2 worldPosition)
    {
        var firingSectorWidthRadians = MathHelper.ToRadians(baseFiringSectorWidthDegrees);
        var firingDirectionRadians = MathHelper.ToRadians(baseFiringDirectionDegrees);
        var halfFiringSectorWidth = firingSectorWidthRadians / 2.0f;

        var range = baseTileRange * Grid.TileLength;
        var firstEndPointDirection = new Vector2(MathF.Cos(firingDirectionRadians + halfFiringSectorWidth), MathF.Sin(firingDirectionRadians + halfFiringSectorWidth));
        var firstEndPoint = worldPosition + firstEndPointDirection * range;
        var secondEndPointDirection = new Vector2(MathF.Cos(firingDirectionRadians - halfFiringSectorWidth), MathF.Sin(firingDirectionRadians - halfFiringSectorWidth));
        var secondEndPoint = worldPosition + secondEndPointDirection * range;

        var droneScreenPosition = Camera.WorldToScreenPosition(worldPosition);
        var firstEndPointScreenPosition = Camera.WorldToScreenPosition(firstEndPoint);
        var secondEndPointScreenPosition = Camera.WorldToScreenPosition(secondEndPoint);

        LineUtility.DrawLine(Game1.Instance.SpriteBatch, droneScreenPosition, firstEndPointScreenPosition, Color.White);
        LineUtility.DrawLine(Game1.Instance.SpriteBatch, droneScreenPosition, secondEndPointScreenPosition, Color.White);
    }

    public void DrawRangeIndicator()
    {
        var firingSectorWidthRadians = MathHelper.ToRadians(firingSectorWidthDegrees);
        var firingDirectionRadians = MathHelper.ToRadians(firingDirectionDegrees);
        var halfFiringSectorWidth = firingSectorWidthRadians / 2.0f;

        var range = realTileRange * Grid.TileLength;
        var droneCenter = Position + Size / 2;
        var firstEndPointDirection = new Vector2(MathF.Cos(firingDirectionRadians + halfFiringSectorWidth), MathF.Sin(firingDirectionRadians + halfFiringSectorWidth));
        var firstEndPoint = droneCenter + firstEndPointDirection * range;
        var secondEndPointDirection = new Vector2(MathF.Cos(firingDirectionRadians - halfFiringSectorWidth), MathF.Sin(firingDirectionRadians - halfFiringSectorWidth));
        var secondEndPoint = droneCenter + secondEndPointDirection * range;

        var droneScreenPosition = Camera.WorldToScreenPosition(droneCenter);
        var firstEndPointScreenPosition = Camera.WorldToScreenPosition(firstEndPoint);
        var secondEndPointScreenPosition = Camera.WorldToScreenPosition(secondEndPoint);

        LineUtility.DrawLine(Game.SpriteBatch, droneScreenPosition, firstEndPointScreenPosition, Color.White);
        LineUtility.DrawLine(Game.SpriteBatch, droneScreenPosition, secondEndPointScreenPosition, Color.White);
    }
}
