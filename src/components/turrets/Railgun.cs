using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
class Railgun : Entity, ITower
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

    public static Texture2D GetTowerBaseSprite()
    {
        return AssetManager.GetTexture("turretTwo");
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(2, 2);
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Railgun;
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        return TowerCore.DefaultCanPlaceTower(GetDefaultGridSize(), targetWorldPosition);
    }

    public static Entity CreateNewInstance(Game game)
    {
        return new Railgun(game);
    }
}
