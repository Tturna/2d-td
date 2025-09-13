using Microsoft.Xna.Framework;

namespace _2d_td;

class Railgun : Entity
{
    private TowerCore towerCore;
    int tileRange = 18;
    int damage = 30;
    float actionsPerSecond = 0.5f;
    float actionTimer;

    public Railgun(Game game) : base(game, AssetManager.GetTexture("turretTwo"))
    {
        towerCore = new TowerCore(this);
    }

    public Railgun(Game game, Vector2 position) : this(game)
    {
        Position = position;
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
        var closestEnemy = towerCore.GetClosestEnemy(tileRange);

        if (closestEnemy is null) return;

        closestEnemy.HealthSystem.TakeDamage(damage);
    }

    public override void Destroy()
    {
        towerCore.CloseDetailsView();
        // Game.Components.Remove(turretHead);
        Game.Components.Remove(towerCore);

        base.Destroy();
    }
}
