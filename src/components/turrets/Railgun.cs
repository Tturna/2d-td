using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
class Railgun : Entity, ITower
{
    private TowerCore towerCore;
    private Vector2 spawnOffset = new (0, 11);
    private static int baseRange = 18;
    int realRange;
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

    public Railgun(Game game, Vector2 position) : base(game, position, GetTowerBaseAnimationData())
    {
        var fireAnimationTexture = AssetManager.GetTexture("railgun_base_fire");

        var fireAnimation = new AnimationSystem.AnimationData
        (
            texture: fireAnimationTexture,
            frameCount: 5,
            frameSize: new Vector2(fireAnimationTexture.Width / 5, fireAnimationTexture.Height),
            delaySeconds: 0.05f
        );

        // base constructor defines animation system
        AnimationSystem!.AddAnimationState("fire", fireAnimation);

        towerCore = new TowerCore(this);

        var antimatterLaserIcon = AssetManager.GetTexture("railgun_antimatterlaser_icon");
        var tungstenShellsIcon = AssetManager.GetTexture("railgun_tungstenshells_icon");
        var cannonballIcon = AssetManager.GetTexture("railgun_cannonball_icon");
        var goldenGatlingIcon = AssetManager.GetTexture("railgun_goldengatling_icon");
        var polishedRoundsIcon = AssetManager.GetTexture("railgun_polishedrounds_icon");

        var AntimatterLaser = new TowerUpgradeNode(Upgrade.AntimatterLaser.ToString(), antimatterLaserIcon, price: 85);
        var Momentum = new TowerUpgradeNode(Upgrade.Momentum.ToString(), tungstenShellsIcon, price: 25, leftChild: AntimatterLaser);

        var Cannonball = new TowerUpgradeNode(Upgrade.Cannonball.ToString(), cannonballIcon, price: 70);
        var GoldenGatling = new TowerUpgradeNode(Upgrade.GoldenGatling.ToString(), goldenGatlingIcon, price: 80);
        var PolishedRound = new TowerUpgradeNode(Upgrade.PolishedRound.ToString(), polishedRoundsIcon, price: 20, leftChild: Cannonball, rightChild: GoldenGatling);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0, parent: null,
            leftChild: Momentum, rightChild: PolishedRound);

        Momentum.Description = "+3 pierce";
        PolishedRound.Description = "+25 damage";
        AntimatterLaser.Description = "+9 pierce,\n+ 6 range,\n+ 20 damage";
        Cannonball.Description = "-2 pierce,\n+250 damage";
        GoldenGatling.Description = "+5 shots/s,\n-40 damage\nAfter firing for 2s straight,\ninflicts burn stacks as well.";

        towerCore.CurrentUpgrade = defaultNode;

        realRange = baseRange;
    }

    public override void Initialize()
    {
        // Offset so the tower is flat on the ground
        UpdatePosition(Vector2.UnitY * 3);

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, pierce);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Momentum.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, pierce+3);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.AntimatterLaser.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage+20, pierce+12);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.PolishedRound.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage+25, pierce);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Cannonball.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage+275, pierce-2);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.GoldenGatling.ToString())
        {
            // todo: inflict burn
            HandleBasicShots(deltaTime, actionsPerSecond+5, 15, pierce);
        }

        base.Update(gameTime);
    }

    private void HandleBasicShots(float deltaTime, float actionsPerSecond, int damage, int pierce)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        if (actionTimer >= actionInterval && IsEnemyInLine(realRange))
        {
            Shoot(damage, pierce);
            actionTimer = 0f;
            AnimationSystem!.OneShotAnimationState("fire");
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
    }

    public bool IsEnemyInLine(int tileRange)
    {
        var range = tileRange * Grid.TileLength;
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(Position, range);

        foreach (Enemy enemy in enemyCandidates)
        {
            // TODO: ensure this condition is correct
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
        Game.Components.Remove(towerCore);

        base.Destroy();
    }

    public static AnimationSystem.AnimationData GetTowerBaseAnimationData()
    {
        var sprite = AssetManager.GetTexture("railgun_base_idle");

        return new AnimationSystem.AnimationData
        (
            texture: sprite,
            frameCount: 7,
            frameSize: new Vector2(sprite.Width / 7, sprite.Height),
            delaySeconds: 0.1f
        );
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

    public static Entity CreateNewInstance(Game game, Vector2 worldPosition)
    {
        return new Railgun(game, worldPosition);
    }

    public void UpgradeTower(TowerUpgradeNode newUpgrade)
    {
        Texture2D newIdleTexture;
        Texture2D newFireTexture;
        var newIdleFrameCount = 1;
        var newFireFrameCount = 1;

        if (newUpgrade.Name == Upgrade.AntimatterLaser.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("railgun_antimatterlaser_idle");
            newFireTexture = AssetManager.GetTexture("railgun_antimatterlaser_fire");
            newIdleFrameCount = 4;
            newFireFrameCount = 6;
            realRange = baseRange + 6;
        }
        else if (newUpgrade.Name == Upgrade.Cannonball.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("railgun_cannonball_idle");
            newFireTexture = AssetManager.GetTexture("railgun_cannonball_fire");
            newIdleFrameCount = 6;
            newFireFrameCount = 7;
        }
        else if (newUpgrade.Name == Upgrade.GoldenGatling.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("railgun_goldengatling_idle");
            newFireTexture = AssetManager.GetTexture("railgun_goldengatling_fire");
            newIdleFrameCount = 3;
            newFireFrameCount = 2;
        }
        else if (newUpgrade.Name == Upgrade.PolishedRound.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("railgun_polishedrounds_idle");
            newFireTexture = AssetManager.GetTexture("railgun_polishedrounds_fire");
            newIdleFrameCount = 8;
            newFireFrameCount = 5;
        }
        else
        {
            newIdleTexture = AssetManager.GetTexture("railgun_tungstenshells_idle");
            newFireTexture = AssetManager.GetTexture("railgun_tungstenshells_fire");
            newIdleFrameCount = 6;
            newFireFrameCount = 5;
        }

        var newIdleAnimation = new AnimationSystem.AnimationData
        (
            texture: newIdleTexture,
            frameCount: newIdleFrameCount,
            frameSize: new Vector2(newIdleTexture.Width / newIdleFrameCount, newIdleTexture.Height),
            delaySeconds: 0.1f
        );

        var newFireAnimation = new AnimationSystem.AnimationData
        (
            texture: newFireTexture,
            frameCount: newFireFrameCount,
            frameSize: new Vector2(newFireTexture.Width / newFireFrameCount, newFireTexture.Height),
            delaySeconds: 0.05f
        );

        AnimationSystem!.ChangeAnimationState(null, newIdleAnimation);
        AnimationSystem.ChangeAnimationState("fire", newFireAnimation);
    }

    public static float GetBaseRange() => baseRange;

    public float GetRange()
    {
        return realRange;
    }
}
