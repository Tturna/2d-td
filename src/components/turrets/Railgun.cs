using Microsoft.Xna.Framework;

namespace _2d_td;

// TODO: Abstract some of this functionality so that methods don't repeat a million times
// across different turret implementations.
class Railgun : AbstractTurret
{
    int tileRange = 18;
    int damage = 30;
    float actionsPerSecond = 0.5f;
    float actionTimer;

    public Railgun(Game game) : base(game, AssetManager.GetTexture("turretTwo"))
    {
    }

    public Railgun(Game game, Vector2 position) : base(game, AssetManager.GetTexture("turretTwo"))
    {
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
        var closestEnemy = GetClosestEnemy(tileRange);

        if (closestEnemy is null) return;

        closestEnemy.HealthSystem.TakeDamage(damage);
    }
}
