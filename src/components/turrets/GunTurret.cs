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
    List<Enemy> hitEnemies = new();

    private TurretDetailsPrompt? detailsPrompt;
    private bool detailsClosed;

    private int tileRange = 12;
    private int damage = 10;
    private float actionsPerSecond = 4f;
    private float actionTimer;
    private float bulletLifetime = 1f;
    private float bulletLength = 16f;
    private float bulletPixelsPerSecond = 360f;
    private float muzzleOffsetFactor = 20f;
    private float turretSmoothSpeed = 5f;

    public GunTurret(Game game) : base(game, AssetManager.GetTexture("gunTurretBase")) { }

    public GunTurret(Game game, Vector2 position) : base(game, position, AssetManager.GetTexture("gunTurretBase")) { }

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

        ClickManager.Clicked += (mouseScreenPosition, _) =>
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

    private void CloseDetailsView()
    {
        UIComponent.Instance.RemoveUIEntity(detailsPrompt);
        detailsPrompt = null;
    }

    public void OnClick()
    {
        if (!detailsClosed && detailsPrompt is null)
        {
            detailsPrompt = new TurretDetailsPrompt(Game, this);
            UIComponent.Instance.AddUIEntity(detailsPrompt);
        }

        detailsClosed = false;
    }

    public bool IsMouseColliding(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        return ClickManager.DefaultCollisionCheck(this, mouseWorldPosition);
    }

    public override void Destroy()
    {
        CloseDetailsView();
        Game.Components.Remove(turretHead);

        base.Destroy();
    }
}
