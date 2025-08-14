using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class GunTurret : Entity
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
    List<Enemy> hitEnemies = new();

    private int tileRange = 12;
    private int damage = 10;
    private float actionsPerSecond = 4f;
    private float actionTimer;
    private float bulletLifetime = 1f;
    private float bulletLength = 16f;
    private float bulletPixelsPerSecond = 360f;
    private float muzzleOffsetFactor = 20f;
    private float turretSmoothSpeed = 5f;

    public GunTurret(Game game) : base(game, AssetManager.GetTexture("turretBase")) { }

    public GunTurret(Game game, Vector2 position) : base(game, position, AssetManager.GetTexture("turretBase")) { }

    public override void Initialize()
    {
        DrawLayerDepth = 0.8f;

        // Position turret head to match where turret base expects it.
        const float TurretHeadXOffset = Grid.TileLength / 2f;
        const float TurretHeadYOffset = 10f;
        turretHeadAxisCenter = Position + new Vector2(TurretHeadXOffset, TurretHeadYOffset);

        turretHead = new Entity(Game, turretHeadAxisCenter, AssetManager.GetTexture("gunTurretHead"));

        // Draw turret head with the origin in its axis of rotation
        const float TurretHeadDrawXOffset = 0.7f;
        var drawOrigin = new Vector2(turretHead!.Sprite!.Width * TurretHeadDrawXOffset, turretHead.Sprite.Height / 2);

        turretHead.DrawOrigin = drawOrigin;

        Game.Components.Add(turretHead);
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var actionInterval = 1f / actionsPerSecond;
        var closestEnemy = GetClosestEnemy();

        actionTimer += deltaTime;
        UpdateBullets(deltaTime);

        if (closestEnemy is null) return;

        AimAtClosestEnemy(closestEnemy.Position, deltaTime);

        if (actionTimer >= actionInterval)
        {
            Shoot(closestEnemy);
            actionTimer = 0f;
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

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

    private Enemy? GetClosestEnemy()
    {
        Enemy? closestEnemy = null;
        float closestDistance = float.PositiveInfinity;

        // TODO: Don't loop over all enemies. Just the ones in range.
        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            var distanceToEnemy = Vector2.Distance(Position, enemy.Position);

            if (distanceToEnemy > tileRange * Grid.TileLength)
                continue;

            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    private void AimAtClosestEnemy(Vector2 enemyPosition, float deltaTime)
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

    private void UpdateBullets(float deltaTime)
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
}
