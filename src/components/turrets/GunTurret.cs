using System;
using Microsoft.Xna.Framework;

namespace _2d_td;

class GunTurret : Entity
{
    int tileRange = 12;
    int damage = 10;
    float actionsPerSecond = 1;
    float actionTimer;

    public GunTurret(Game game) : base(game, AssetManager.GetTexture("turret"))
    {
    }

    public GunTurret(Game game, Vector2 position) : base(game, position, AssetManager.GetTexture("turret"))
    {
    }

    public override void Update(GameTime gameTime)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += gameTime.ElapsedGameTime.Milliseconds / 1000f;

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

        closestEnemy.HealthSystem.TakeDamage(damage);
    }
}
