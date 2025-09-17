using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
class Railgun : Entity, ITower
{
    private TowerCore towerCore;
    private Vector2 spawnOffset = new (0, 15);
    int tileRange = 18;
    int damage = 30;
    int pierce = 3;
    float bulletSpeed = 900f;
    float actionsPerSecond = 0.5f;
    float actionTimer;

    public enum Upgrade
    {
        NoUpgrade,
        Momentum,
        AntimatterLaser,
        PolishedRound,
        Cannonball,
        GoldenGatling
    }

    public Railgun(Game game) : base(game, GetTowerBaseSprite())
    {
        towerCore = new TowerCore(this);

        var AntimatterLaser = new TowerUpgradeNode(Upgrade.AntimatterLaser.ToString(), price: 85);
        var Momentum = new TowerUpgradeNode(Upgrade.Momentum.ToString(), price: 25, leftChild: AntimatterLaser);

        var Cannonball = new TowerUpgradeNode(Upgrade.Cannonball.ToString(), price: 70);
        var GoldenGatling = new TowerUpgradeNode(Upgrade.GoldenGatling.ToString(), price: 80);
        var PolishedRound = new TowerUpgradeNode(Upgrade.PolishedRound.ToString(), price: 20, leftChild: Cannonball, rightChild: GoldenGatling);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), price: 0, parent: null,
            leftChild: Momentum, rightChild: PolishedRound);

        towerCore.CurrentUpgrade = defaultNode;
    }

    public Railgun(Game game, Vector2 position) : this(game)
    {
        Position = position;
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, tileRange, pierce);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Momentum.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, tileRange, pierce+3);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.AntimatterLaser.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage+20, tileRange+6, pierce+12);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.PolishedRound.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage+25, tileRange, pierce);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Cannonball.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage+275, tileRange, pierce-2);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.GoldenGatling.ToString())
        {
            // todo: inflict burn
            HandleBasicShots(deltaTime, actionsPerSecond+5, 15, tileRange, pierce);
        }

        base.Update(gameTime);
    }

    private void HandleBasicShots(float deltaTime, float actionsPerSecond, int damage, int tileRange, int pierce)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        if (actionTimer >= actionInterval && IsEnemyInLine(tileRange))
        {
            Shoot(damage, pierce);
            actionTimer = 0f;
        }
    }

    private void Shoot(int damage, int pierce)
    {
        var direction = new Vector2(-1, 0);
        var bullet = new Projectile(Game, Position + spawnOffset);
        bullet.Direction = direction;
        bullet.BulletPixelsPerSecond = bulletSpeed;
        bullet.Damage = damage;
        bullet.Lifetime = 1f;
        bullet.BulletLength = 20f;
        bullet.BulletWidth = 8f;
        bullet.Pierce = pierce;
        bullet.Sprite = AssetManager.GetTexture("tempprojectile");
        Game.Components.Add(bullet);
    }

    public bool IsEnemyInLine(int tileRange)
    {
        var range = tileRange * Grid.TileLength;
        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            if (enemy.Position.Y < Position.Y + Size.Y &&
                enemy.Position.Y > Position.Y &&
                enemy.Position.X < Position.X &&
                enemy.Position.X + range > Position.X)
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
        return AssetManager.GetTexture("railgun");
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(3, 2);
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
