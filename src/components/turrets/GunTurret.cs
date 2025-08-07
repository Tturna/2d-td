using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

class GunTurret : Entity
{
    private Entity turretHead;

    private int tileRange = 12;
    private int damage = 10;
    private float actionsPerSecond = 1;
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
        turretHead = new Entity(Game, Position + new Vector2(8f, 10f), AssetManager.GetTexture("gunTurretHead"));
        turretHead.DrawOrigin = new Vector2(turretHead.Sprite.Width * 0.7f, turretHead.Sprite.Height / 2);
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
        foreach (Enemy enemy in Game.Enemies)
        {
            var distanceToEnemy = Vector2.Distance(Position, enemy.Position);

            if (distanceToEnemy > tileRange * 16f)
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
