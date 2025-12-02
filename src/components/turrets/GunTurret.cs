using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
class GunTurret : Entity, ITower
{
    private TowerCore towerCore;
    private Entity? turretHead;
    private Vector2 turretHeadAxisCenter;
    private float photonCannonTargetDistance;

    private static int baseRange = 12;
    private int realRange;
    private int baseDamage = 10;
    private int realDamage;
    private float actionsPerSecond = 1f;
    private float actionTimer;
    private float bulletPixelsPerSecond = 360f;
    private float muzzleOffsetFactor = 20f;
    private float turretSmoothSpeed = 5f;

    private Random random = new();

    public enum Upgrade
    {
        NoUpgrade,
        DoubleGun,
        PhotonCannon,
        BotShot,
        ImprovedBarrel,
        RocketShots
    }

    public GunTurret(Game game, Vector2 position) : base(game, position, GetUnupgradedBaseAnimationData())
    {
        towerCore = new TowerCore(this);

        var photonCannonIcon = AssetManager.GetTexture("gunTurret_photoncannon_icon");
        var botShotIcon = AssetManager.GetTexture("gunTurret_botshot_icon");
        var doubleGunIcon = AssetManager.GetTexture("gunTurret_doublegun_icon");
        var rocketShotsIcon = AssetManager.GetTexture("gunTurret_rocketshots_icon");
        var improvedBarrelIcon = AssetManager.GetTexture("gunTurret_improvedbarrel_icon");

        var photonCannon = new TowerUpgradeNode(Upgrade.PhotonCannon.ToString(), photonCannonIcon, price: 75);
        var botShot = new TowerUpgradeNode(Upgrade.BotShot.ToString(), botShotIcon, price: 50);
        var doubleGun = new TowerUpgradeNode(Upgrade.DoubleGun.ToString(), doubleGunIcon, price: 20, leftChild: photonCannon, rightChild: botShot);

        var rocketShots = new TowerUpgradeNode(Upgrade.RocketShots.ToString(), rocketShotsIcon, price: 70);
        var improvedBarrel = new TowerUpgradeNode(Upgrade.ImprovedBarrel.ToString(), improvedBarrelIcon, price: 15, leftChild: rocketShots);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0, parent: null,
            leftChild: doubleGun, rightChild: improvedBarrel);

        doubleGun.Description = "+1 shots/sec";
        improvedBarrel.Description = "+3 damage,\n+4 range";
        photonCannon.Description = "Fires a constant beam\nthat deals 60 DPS\nto one unit.";
        botShot.Description = "-75% fire rate,\n-2 range,\n+8 damage,\nx5 projectiles.\nIncreased knockback.";
        rocketShots.Description = "+20 damage,\n+4 range,\n2 tile radius explosion\non impact ";

        towerCore.CurrentUpgrade = defaultNode;
        realRange = baseRange;
        realDamage = baseDamage;
    }

    public override void Initialize()
    {
        // Position turret head to match where turret base expects it.
        float TurretHeadXOffset = AnimationSystem!.BaseAnimationData.FrameSize.X * 0.7f;
        float TurretHeadYOffset = 9f;
        turretHeadAxisCenter = Position + new Vector2(TurretHeadXOffset, TurretHeadYOffset);

        // Offset turret base pos by 2 pixels;
        UpdatePosition(Vector2.UnitX * 2);

        turretHead = new Entity(Game, turretHeadAxisCenter, AssetManager.GetTexture("gunTurretHead"));

        // Draw turret head with the origin in its axis of rotation
        const float TurretHeadDrawXOffset = 0.85f;
        var drawOrigin = new Vector2(turretHead!.Sprite!.Width * TurretHeadDrawXOffset, turretHead.Sprite.Height / 2);

        turretHead.DrawOrigin = drawOrigin;
        turretHead.DrawLayerDepth = 0.8f;
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage: 10);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.DoubleGun.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond + 1, damage: 10);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.PhotonCannon.ToString())
        {
            HandlePhotonCannon(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.BotShot.ToString())
        {
            HandleBotShot(deltaTime);
            // TODO: add knockback
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.ImprovedBarrel.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage: 13);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.RocketShots.ToString())
        {
            HandleRocketShots(deltaTime);
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        if (towerCore.CurrentUpgrade.Name == Upgrade.PhotonCannon.ToString())
        {
            if (photonCannonTargetDistance > 0)
            {
                var rad = -turretHead!.RotationRadians - MathHelper.PiOver2;
                var dirX = Math.Sin(rad);
                var dirY = Math.Cos(rad);
                var dir = new Vector2((float)dirX, (float)dirY);
                var target = turretHead!.Position + dir * photonCannonTargetDistance;
                LineUtility.DrawLine(Game.SpriteBatch, turretHead!.Position + dir * 16, target, Color.Red, thickness: 2f);
            }
        }
    }

    private void HandleBasicShots(float deltaTime, float actionsPerSecond, int damage)
    {
        var actionInterval = 1f / actionsPerSecond;
        var closestEnemy = towerCore.GetClosestValidEnemy(realRange);

        actionTimer += deltaTime;

        if (closestEnemy is null) return;

        AimAtClosestEnemy(closestEnemy.Position + closestEnemy.Size / 2, deltaTime);

        if (actionTimer >= actionInterval)
        {
            var enemyCenter = closestEnemy.Position + closestEnemy.Size / 2;
            var direction = enemyCenter - turretHeadAxisCenter;
            direction.Normalize();
            Shoot(direction, damage: 10);
            actionTimer = 0f;
        }
    }

    private void HandleBotShot(float deltaTime)
    {
        var actionInterval = 1f / (actionsPerSecond * 0.25f);
        var closestEnemy = towerCore.GetClosestValidEnemy(baseRange - 2);

        actionTimer += deltaTime;

        if (closestEnemy is null) return;

        AimAtClosestEnemy(closestEnemy.Position + closestEnemy.Size / 2, deltaTime);

        if (actionTimer >= actionInterval)
        {
            var enemyPosition = closestEnemy.Position + closestEnemy.Size / 2;
            var enemyDirection = enemyPosition - turretHeadAxisCenter;
            enemyDirection.Normalize();

            for (int i = 0; i < 5; i++)
            {
                var randomX = random.Next(-12, 12);
                var randomY = random.Next(-12, 12);
                var targetDirection = enemyDirection + new Vector2(randomX, randomY);
                Shoot(targetDirection, damage: 18);
            }

            actionTimer = 0f;
        }
    }

    private void HandlePhotonCannon(float deltaTime)
    {
        // Deal damage 8 times per second
        var actionInterval = 1f / 8f;
        var closestEnemy = towerCore.GetClosestValidEnemy(baseRange);

        actionTimer += deltaTime;
        photonCannonTargetDistance = 0f;

        if (closestEnemy is null) return;

        var aimAccuracy = AimAtClosestEnemy(closestEnemy.Position + closestEnemy.Size / 2, deltaTime);

        if (aimAccuracy < 0.05f)
        {
            photonCannonTargetDistance = (turretHead!.Position - closestEnemy.Position).Length();

            if (actionTimer >= actionInterval)
            {
                // 60 DPS
                var damage = (int)(60 * actionInterval);
                closestEnemy.HealthSystem.TakeDamage(damage);
                actionTimer = 0f;
            }
        }
    }

    private void HandleRocketShots(float deltaTime)
    {
        // TODO: Centralize upgrade specific data like damage and range definitions.
        // Either have them all defined in Update(), define them in the handler functions
        // or come up with a better structure.

        // TODO: 2 tile explosion on bullet impact
        var actionInterval = 1f / actionsPerSecond;
        var closestEnemy = towerCore.GetClosestValidEnemy(baseRange + 8);

        actionTimer += deltaTime;

        if (closestEnemy is null) return;

        AimAtClosestEnemy(closestEnemy.Position + closestEnemy.Size / 2, deltaTime);

        if (actionTimer >= actionInterval)
        {
            var enemyCenter = closestEnemy.Position + closestEnemy.Size / 2;
            var direction = enemyCenter - turretHeadAxisCenter;
            direction.Normalize();
            Shoot(direction, 33);
            actionTimer = 0f;
        }
    }

    /// <summary>
    /// Aims the turret head smoothly towards given position. Returns similarity between turret head direction
    /// and target direction (0 = same, 1 = opposite).
    /// </summary>
    private float AimAtClosestEnemy(Vector2 enemyPosition, float deltaTime)
    {
        var enemyTurretDiff = enemyPosition - turretHead!.Position;
        // Add MathHelper.Pi to rotate by 180 degrees because the turret sprite's forward direction is opposite to the mathematical zero angle.
        var radiansToEnemy = (float)Math.Atan2(enemyTurretDiff.Y, enemyTurretDiff.X) + MathHelper.Pi;
        var radiansDiff = radiansToEnemy - turretHead.RotationRadians;

        // Wrap the difference to [-pi, pi] range
        while (radiansDiff > MathHelper.Pi)
            radiansDiff -= MathHelper.TwoPi;
        while (radiansDiff < -MathHelper.Pi)
            radiansDiff += MathHelper.TwoPi;

        var targetRadians = turretHead.RotationRadians + radiansDiff;
        var smoothRadians = MathHelper.Lerp(turretHead.RotationRadians, targetRadians, deltaTime * turretSmoothSpeed);
        turretHead.RotationRadians = smoothRadians;

        return radiansDiff / MathHelper.Pi;
    }

    private void Shoot(Vector2 direction, int damage)
    {
        direction.Normalize();
        var muzzleOffset = direction * muzzleOffsetFactor;
        var startLocation = turretHeadAxisCenter+muzzleOffset;

        var bullet = new Projectile(Game, startLocation);
        bullet.Direction = direction;
        bullet.BulletPixelsPerSecond = bulletPixelsPerSecond;
        bullet.Damage = damage;
        bullet.Lifetime = 1f;
        bullet.Pierce = 3;
    }

    public override void Destroy()
    {
        towerCore.CloseDetailsView();
        turretHead?.Destroy();
        Game.Components.Remove(towerCore);

        base.Destroy();
    }

    public static AnimationSystem.AnimationData GetUnupgradedBaseAnimationData()
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

    public static List<KeyValuePair<UIEntity, Vector2>> GetUnupgradedPartIcons(List<UIEntity> uiElements)
    {
        var baseSprite = AssetManager.GetTexture("gunTurretBase");
        var turretSprite = AssetManager.GetTexture("gunTurretHead");

        var baseEntity = new UIEntity(Game1.Instance, uiElements, baseSprite);
        var turretEntity = new UIEntity(Game1.Instance, uiElements, turretSprite);

        const float TurretHeadDrawXOffset = 0.85f;
        var drawOrigin = new Vector2(turretSprite.Width * TurretHeadDrawXOffset, turretSprite.Height / 2);
        turretEntity.DrawOrigin = drawOrigin;
        turretEntity.DrawLayerDepth = 0.6f;

        var list = new List<KeyValuePair<UIEntity, Vector2>>();
        list.Add(KeyValuePair.Create(baseEntity, new Vector2(2, 0)));
        list.Add(KeyValuePair.Create(turretEntity, new Vector2(baseSprite.Width * 0.7f, 9f)));

        return list;
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(2, 2);
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.GunTurret;
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        return TowerCore.DefaultCanPlaceTower(GetDefaultGridSize(), targetWorldPosition);
    }

    public static Entity CreateNewInstance(Game game, Vector2 worldPosition)
    {
        return new GunTurret(game, worldPosition);
    }

    public void UpgradeTower(TowerUpgradeNode newUpgrade)
    {
        Texture2D newBaseTexture = AnimationSystem!.BaseAnimationData.Texture;
        int newFrameCount = 1;

        if (newUpgrade.Name == Upgrade.BotShot.ToString())
        {
            newBaseTexture = AssetManager.GetTexture("gunTurret_botshot_body");
            turretHead!.Sprite = AssetManager.GetTexture("gunTurret_botshot_gun");
            realRange = baseRange - 2;
            realDamage = baseDamage + 8;
        }
        else if (newUpgrade.Name == Upgrade.PhotonCannon.ToString())
        {
            newBaseTexture = AssetManager.GetTexture("gunTurret_photoncannon_body");
            turretHead!.Sprite = AssetManager.GetTexture("gunTurret_photoncannon_gun");
        }
        else if (newUpgrade.Name == Upgrade.RocketShots.ToString())
        {
            newBaseTexture = AssetManager.GetTexture("gunTurret_rocketshots_body");
            turretHead!.Sprite = AssetManager.GetTexture("gunTurret_rocketshots_gun");
            realRange = baseRange + 8;
            realDamage = baseDamage + 23;
        }
        else if (newUpgrade.Name == Upgrade.DoubleGun.ToString())
        {
            turretHead!.Sprite = AssetManager.GetTexture("gunTurret_doublegun_gun");
        }
        else if (newUpgrade.Name == Upgrade.ImprovedBarrel.ToString())
        {
            turretHead!.Sprite = AssetManager.GetTexture("gunTurret_improvedbarrel_gun");
            realRange = baseRange + 4;
            realDamage = baseDamage + 3;
        }

        var newBaseAnimation = new AnimationSystem.AnimationData
        (
            texture: newBaseTexture,
            frameCount: newFrameCount,
            frameSize: new Vector2(newBaseTexture.Width / newFrameCount, newBaseTexture.Height),
            delaySeconds: 0.1f
        );

        AnimationSystem.ChangeAnimationState(null, newBaseAnimation);
    }

    public static float GetBaseRange() => baseRange;

    public float GetRange()
    {
        return realRange;
    }
}
