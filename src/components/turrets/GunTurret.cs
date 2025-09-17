using System;
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

    private int baseRange = 12;
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

    public GunTurret(Game game) : base(game, GetTowerBaseSprite())
    {
        towerCore = new TowerCore(this);

        var photonCannon = new TowerUpgradeNode(Upgrade.PhotonCannon.ToString());
        var botShot = new TowerUpgradeNode(Upgrade.BotShot.ToString());
        var doubleGun = new TowerUpgradeNode(Upgrade.DoubleGun.ToString(), leftChild: photonCannon, rightChild: botShot);

        var rocketShots = new TowerUpgradeNode(Upgrade.RocketShots.ToString());
        var improvedBarrel = new TowerUpgradeNode(Upgrade.ImprovedBarrel.ToString(), leftChild: rocketShots);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), parent: null,
            leftChild: doubleGun, rightChild: improvedBarrel);

        towerCore.CurrentUpgrade = defaultNode;
    }

    public GunTurret(Game game, Vector2 position) : this(game)
    {
        Position = position;
    }

    public override void Initialize()
    {
        // Position turret head to match where turret base expects it.
        float TurretHeadXOffset = Sprite!.Width * 0.7f;
        float TurretHeadYOffset = 9f;
        turretHeadAxisCenter = Position + new Vector2(TurretHeadXOffset, TurretHeadYOffset);

        // Offset turret base pos by 2 pixels;
        Position += Vector2.UnitX * 2;

        turretHead = new Entity(Game, turretHeadAxisCenter, AssetManager.GetTexture("gunTurretHead"));

        // Draw turret head with the origin in its axis of rotation
        const float TurretHeadDrawXOffset = 0.85f;
        var drawOrigin = new Vector2(turretHead!.Sprite!.Width * TurretHeadDrawXOffset, turretHead.Sprite.Height / 2);

        turretHead.DrawOrigin = drawOrigin;
        turretHead.DrawLayerDepth = 0.8f;

        Game.Components.Add(turretHead);
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage: 10, tileRange: baseRange);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.DoubleGun.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond + 1, damage: 10, tileRange: baseRange);
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
            HandleBasicShots(deltaTime, actionsPerSecond, damage: 13, tileRange: baseRange + 4);
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

    private void HandleBasicShots(float deltaTime, float actionsPerSecond, int damage, int tileRange)
    {
        var actionInterval = 1f / actionsPerSecond;
        var closestEnemy = towerCore.GetClosestValidEnemy(tileRange);

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
        Game.Components.Add(bullet);
    }

    public override void Destroy()
    {
        towerCore.CloseDetailsView();
        Game.Components.Remove(turretHead);
        Game.Components.Remove(towerCore);

        base.Destroy();
    }

    public static Texture2D GetTowerBaseSprite()
    {
        return AssetManager.GetTexture("gunTurretBase");
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

    public static Entity CreateNewInstance(Game game)
    {
        return new GunTurret(game);
    }
}
