using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

class GunTurret : Entity
{
    private Entity turretHead;

    private int tileRange = 12;
    private int damage = 10;
    private float actionsPerSecond = 1f;
    private float actionTimer;

    public GunTurret(Game game) : base(game, AssetManager.GetTexture("turretBase"))
    {
    }

    public GunTurret(Game game, Vector2 position) : base(game, position, AssetManager.GetTexture("turretBase"))
    {
    }

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
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (actionTimer >= actionInterval)
        {
            ShootAtClosestEnemy();
            actionTimer = 0f;
        }

        base.Update(gameTime);
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
    }
}
