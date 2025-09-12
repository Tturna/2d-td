using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class GunTurret : Entity, IClickable
{
    private struct Bullet
    {
        public Vector2 Target;
        public float InitialLifetime;
        public float Lifetime;
    }

    private Entity? turretHead;
    private Vector2 turretHeadAxisCenter;
    private List<Bullet> bullets = new();
    private List<Enemy> hitEnemies = new();
    private float photonCannonTargetDistance;

    private TurretDetailsPrompt? detailsPrompt;
    private bool detailsClosed;

    private int baseRange = 12;
    private float actionsPerSecond = 1f;
    private float actionTimer;
    private float bulletLifetime = 1f;
    private float bulletLength = 16f;
    private float bulletPixelsPerSecond = 360f;
    private float muzzleOffsetFactor = 20f;
    private float turretSmoothSpeed = 5f;

    public enum Upgrade
    {
        NoUpgrade,
        DoubleGun,
        PhotonCannon,
        BotShot,
        ImprovedBarrel,
        RocketShots
    }

    public TowerUpgradeNode CurrentUpgrade { get; private set; }

    public GunTurret(Game game) : base(game, AssetManager.GetTexture("gunTurretBase"))
    {
        var photonCannon = new TowerUpgradeNode(Upgrade.PhotonCannon.ToString());
        var botShot = new TowerUpgradeNode(Upgrade.BotShot.ToString());
        var doubleGun = new TowerUpgradeNode(Upgrade.DoubleGun.ToString(), leftChild: photonCannon, rightChild: botShot);
        photonCannon.SetParent(doubleGun);
        botShot.SetParent(doubleGun);

        var rocketShots = new TowerUpgradeNode(Upgrade.RocketShots.ToString());
        var improvedBarrel = new TowerUpgradeNode(Upgrade.ImprovedBarrel.ToString(), leftChild: rocketShots);
        rocketShots.SetParent(improvedBarrel);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), parent: null,
            leftChild: doubleGun, rightChild: improvedBarrel);

        doubleGun.SetParent(defaultNode);
        improvedBarrel.SetParent(defaultNode);
        CurrentUpgrade = defaultNode;
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

        InputSystem.Clicked += (mouseScreenPosition, _) =>
        {
            if (detailsPrompt is not null && detailsPrompt.ShouldCloseDetailsView(mouseScreenPosition))
            {
                CloseDetailsView();
                detailsClosed = true;
            }
            else
            {
                detailsClosed = false;
            }
        };
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage: 10, tileRange: baseRange);
        }
        else if (CurrentUpgrade.Name == Upgrade.DoubleGun.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond + 1, damage: 10, tileRange: baseRange);
        }
        else if (CurrentUpgrade.Name == Upgrade.PhotonCannon.ToString())
        {
            HandlePhotonCannon(deltaTime);
        }
        else if (CurrentUpgrade.Name == Upgrade.BotShot.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond * 0.25f, damage: 18, tileRange: baseRange - 2);
            // TODO: multiply projectiles by 5 and add knockback
        }
        else if (CurrentUpgrade.Name == Upgrade.ImprovedBarrel.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage: 13, tileRange: baseRange + 4);
        }
        else if (CurrentUpgrade.Name == Upgrade.RocketShots.ToString())
        {
            // TODO: +20 damage, +4 range, 2 tile radius explosion on impact
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        if (CurrentUpgrade.Name == Upgrade.PhotonCannon.ToString())
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
        else
        {
            foreach (Bullet bullet in bullets)
            {
                var positionDiff = bullet.Target - turretHeadAxisCenter;
                var direction = positionDiff;
                direction.Normalize();

                var muzzleCenter = turretHeadAxisCenter + direction * muzzleOffsetFactor;
                var reverseLifetime = bullet.InitialLifetime - bullet.Lifetime;

                var position = muzzleCenter + direction * (bulletPixelsPerSecond * reverseLifetime);
                var bulletStart = position - direction * bulletLength / 2f;
                var bulletEnd = position + direction * bulletLength / 2f;

                LineUtility.DrawLine(Game.SpriteBatch, bulletStart, bulletEnd, Color.Red, thickness: 2f);
            }
        }
    }

    private void HandleBasicShots(float deltaTime, float actionsPerSecond, int damage, int tileRange)
    {
        var actionInterval = 1f / actionsPerSecond;
        var closestEnemy = TurretHelper.GetClosestEnemy(this, baseRange);

        actionTimer += deltaTime;
        UpdateBullets(deltaTime, damage);

        if (closestEnemy is null) return;

        AimAtClosestEnemy(closestEnemy.Position, deltaTime);

        if (actionTimer >= actionInterval)
        {
            Shoot(closestEnemy);
            actionTimer = 0f;
        }
    }

    private void HandlePhotonCannon(float deltaTime)
    {
        // Deal damage 8 times per second
        var actionInterval = 1f / 8f;
        var closestEnemy = TurretHelper.GetClosestEnemy(this, baseRange);

        actionTimer += deltaTime;
        photonCannonTargetDistance = 0f;

        if (closestEnemy is null) return;

        var aimAccuracy = AimAtClosestEnemy(closestEnemy.Position, deltaTime);

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

    private void Shoot(Enemy enemy)
    {
        var target = enemy.Position + enemy.Size / 2;
        var bullet = new Bullet
        {
            Target = target,
            InitialLifetime = bulletLifetime,
            Lifetime = bulletLifetime
        };

        bullets.Add(bullet);
    }

    private void UpdateBullets(float deltaTime, int damage)
    {
        hitEnemies.Clear();

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];

            var positionDiff = bullet.Target - turretHeadAxisCenter;
            var direction = positionDiff;
            direction.Normalize();

            var muzzleCenter = turretHeadAxisCenter + direction * muzzleOffsetFactor;
            var reverseLifetime = bullet.InitialLifetime - bullet.Lifetime;

            var position = muzzleCenter + direction * (bulletPixelsPerSecond * reverseLifetime);
            var oldPosition = muzzleCenter + direction * (bulletPixelsPerSecond * (reverseLifetime - deltaTime));

            var bulletHit = false;

            foreach (Enemy enemy in EnemySystem.Enemies)
            {
                if (Collision.IsLineInEntity(oldPosition, position, enemy,
                    out Vector2 entryPoint, out Vector2 exitPoint))
                {
                    hitEnemies.Add(enemy);
                    bulletHit = true;
                }
            }

            bullet.Lifetime -= deltaTime;

            if (bulletHit || bullet.Lifetime <= 0f)
            {
                bullets.RemoveAt(i);
                continue;
            }

            bullets[i] = bullet;
        }

        for (int i = 0; i < hitEnemies.Count; i++)
        {
            var enemy = hitEnemies[i];
            enemy.HealthSystem.TakeDamage(damage);
        }
    }

    private void CloseDetailsView()
    {
        UIComponent.Instance.RemoveUIEntity(detailsPrompt);
        detailsPrompt = null;
    }

    public void OnClick()
    {
        if (!detailsClosed && detailsPrompt is null)
        {
            detailsPrompt = new TurretDetailsPrompt(Game, this, UpgradeLeft, UpgradeRight);
            UIComponent.Instance.AddUIEntity(detailsPrompt);
        }

        detailsClosed = false;
    }

    public bool IsMouseColliding(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        return Collision.IsPointInEntity(mouseWorldPosition, this);
    }

    public override void Destroy()
    {
        CloseDetailsView();
        Game.Components.Remove(turretHead);

        base.Destroy();
    }

    public void UpgradeLeft()
    {
        if (CurrentUpgrade.LeftChild is null)
        {
            throw new InvalidOperationException($"Node {CurrentUpgrade.Name} does not have a left child node.");
        }

        if (!CurrencyManager.TryBuyUpgrade(CurrentUpgrade.LeftChild.Name)) return;

        CurrentUpgrade = CurrentUpgrade.LeftChild;
    }

    public void UpgradeRight()
    {
        if (CurrentUpgrade.RightChild is null)
        {
            throw new InvalidOperationException($"Node {CurrentUpgrade.Name} does not have a right child node.");
        }

        if (!CurrencyManager.TryBuyUpgrade(CurrentUpgrade.RightChild.Name)) return;

        CurrentUpgrade = CurrentUpgrade.RightChild;
    }
}
