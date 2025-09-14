using Microsoft.Xna.Framework;

namespace _2d_td;

// TODO: Abstract some of this functionality so that methods don't repeat a million times
// across different turret implementations.
#nullable enable
class Railgun : Entity
{
    private TowerCore towerCore;
    private Vector2 spawnOffset = new (0, 3);
    int tileRange = 18;
    int damage = 30;
    float bulletSpeed = 900f;
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

        if (actionTimer >= actionInterval && IsEnemyInLine(tileRange))
        {
            Shoot();
            actionTimer = 0f;
        }

        base.Update(gameTime);
    }

    private void Shoot()
    {
        var direction = new Vector2(-1, 0);
        var bullet = new Projectile(Game, Position+spawnOffset, direction, damage, bulletSpeed, 2);
        bullet.bulletLength = 30f;
        Game.Components.Add(bullet);
    }

    public bool IsEnemyInLine(int tileRange)
    {
        // TODO: Don't loop over all enemies. Just the ones in range.
        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            if (enemy.Position.Y < Position.Y + Size.Y &&
                enemy.Position.Y > Position.Y)
            {
                return true;
            }
        }
        return false;
    }

    public override void Destroy()
    {
        towerCore.CloseDetailsView();
        // Game.Components.Remove(turretHead);
        Game.Components.Remove(towerCore);

        base.Destroy();
    }
}
