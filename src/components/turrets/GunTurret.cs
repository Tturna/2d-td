using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

class GunTurret : Entity
{
    private Entity turretHead;
    private List<KeyValuePair<Vector2, float>> bullets = new();

    private int tileRange = 12;
    private int damage = 10;
    private float actionsPerSecond = 1f;
    private float actionTimer;

    public GunTurret(Game game) : base(game, AssetManager.GetTexture("turretBase")) { }

    public GunTurret(Game game, Vector2 position) : base(game, position, AssetManager.GetTexture("turretBase")) { }

    public override void Initialize()
    {
        DrawLayerDepth = 0.8f;

        // Position turret head to match where turret base expects it.
        const float TurretHeadXOffset = Grid.TileLength / 2f;
        const float TurretHeadYOffset = 10f;
        var position = Position + new Vector2(TurretHeadXOffset, TurretHeadYOffset);

        turretHead = new Entity(Game, position, AssetManager.GetTexture("gunTurretHead"));

        // Draw turret head with the origin in its axis of rotation
        const float TurretHeadDrawXOffset = 0.7f;
        var drawOrigin = new Vector2(turretHead.Sprite.Width * TurretHeadDrawXOffset, turretHead.Sprite.Height / 2);

        turretHead.DrawOrigin = drawOrigin;

        Game.Components.Add(turretHead);
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (actionTimer >= actionInterval)
        {
            ShootAtClosestEnemy();
            actionTimer = 0f;
        }

        for (int i = 0; i < bullets.Count; i++)
        {
            (Vector2 target, float lifetime) = bullets[i];

            lifetime -= deltaTime * 2f;

            if (lifetime <= 0f)
            {
                bullets.RemoveAt(i);
                continue;
            }

            bullets[i] = KeyValuePair.Create(target, lifetime);
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        foreach ((Vector2 target, float lifetime) in bullets)
        {
            var muzzleCenter = turretHead.Position + turretHead.Size / 2;
            var positionDiff = muzzleCenter - target;
            var direction = positionDiff;
            direction.Normalize();
            var position = Vector2.Lerp(muzzleCenter, muzzleCenter + direction * 1000f, (lifetime - 1f));
            var bulletLength = 16f;
            var bulletStart = position - direction * bulletLength / 2f;
            var bulletEnd = position + direction * bulletLength / 2f;

            LineUtility.DrawLine(Game.SpriteBatch, bulletStart, bulletEnd, Color.Red, 2f);
        }
    }

    private void ShootAtClosestEnemy()
    {
        Enemy closestEnemy = null;
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

        if (closestEnemy is null) return;

        var enemyTurretDiff = closestEnemy.Position - turretHead.Position;
        // Add MathHelper.Pi to rotate by 180 degrees because the turret sprite's forward direction is opposite to the mathematical zero angle.
        var radiansToEnemy = Math.Atan2(enemyTurretDiff.Y, enemyTurretDiff.X) + MathHelper.Pi;
        turretHead.RotationRadians = (float)radiansToEnemy;
        closestEnemy.HealthSystem.TakeDamage(damage);

        var enemyCenter = closestEnemy.Position + closestEnemy.Size / 2;
        bullets.Add(KeyValuePair.Create(enemyCenter, 1f));
    }
}
