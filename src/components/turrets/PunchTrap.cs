using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class PunchTrap : Entity, ITower
{
    private TowerCore towerCore;
    int tileRange = 2;
    Vector2 direction = new Vector2(-1,0);
    float knockback = 2.5f;
    int damage = 10;
    float actionsPerSecond = 0.333f;
    float actionTimer;

    public enum Upgrade
    {
        NoUpgrade,
        FatFist,
        MegaPunch,
        RocketGlove,
        QuickJabs,
        FlurryOfBlows,
        Chainsaw

    }

    public PunchTrap(Game game, Vector2 position) : base(game, position, GetTowerBaseAnimationData())
    {
        //var fireAnimationTexture = AssetManager.GetTexture("punchtrap_base");

        /*var fireAnimation = new AnimationSystem.AnimationData
        (
            texture: fireAnimationTexture,
            frameCount: 5,
            frameSize: new Vector2(fireAnimationTexture.Width / 5, fireAnimationTexture.Height),
            delaySeconds: 0.05f
        );

        // base constructor defines animation system
        AnimationSystem!.AddAnimationState("fire", fireAnimation);*/

        towerCore = new TowerCore(this);

        var nukeIcon = AssetManager.GetTexture("mortar_nuke_icon");

        var MegaPunch = new TowerUpgradeNode(Upgrade.MegaPunch.ToString(),
            upgradeIcon: nukeIcon, price: 75);
        var RocketGlove = new TowerUpgradeNode(Upgrade.RocketGlove.ToString(),
            upgradeIcon: nukeIcon, price: 80);
        var FatFist = new TowerUpgradeNode(Upgrade.FatFist.ToString(),
            upgradeIcon: nukeIcon, price: 10,leftChild: MegaPunch,rightChild: RocketGlove);
        

        var Chainsaw = new TowerUpgradeNode(Upgrade.Chainsaw.ToString(),
            upgradeIcon: nukeIcon, price: 70);
        var FlurryOfBlows = new TowerUpgradeNode(Upgrade.FlurryOfBlows.ToString(),
            upgradeIcon: nukeIcon, price: 80);
        var QuickJabs = new TowerUpgradeNode(Upgrade.QuickJabs.ToString(),
            upgradeIcon: nukeIcon, price: 10,leftChild: FlurryOfBlows, rightChild: Chainsaw);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(),
            upgradeIcon: null, price: 0, parent: null, leftChild: FatFist, rightChild: QuickJabs);

        towerCore.CurrentUpgrade = defaultNode;
    }

    public override void Initialize()
    {
        // Offset so the tower is flat on the ground
        //Position += Vector2.UnitY * 3;

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, tileRange, knockback);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.FatFist.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage + 10, tileRange, knockback * 1.5f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.MegaPunch.ToString())
        {
            HandleMegaPunch(deltaTime, actionsPerSecond, damage + 20, tileRange, knockback * 1.5f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.QuickJabs.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond + .167f, damage, tileRange, knockback);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Chainsaw.ToString())
        {
            HandleBasicShots(deltaTime, 10f, 10, tileRange, 0f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.RocketGlove.ToString())
        {
            HandleRocketGlove(deltaTime, actionsPerSecond, damage + 50, tileRange, knockback);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.FlurryOfBlows.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond+1.5f, damage, tileRange, knockback);
        }

        base.Update(gameTime);
    }

    private void HandleBasicShots(float deltaTime, float actionsPerSecond, int damage, int tileRange, float knockback)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        if (actionTimer >= actionInterval && DetectEnemies(tileRange))
        {
            Shoot(damage, knockback);
            actionTimer = 0f;
            //AnimationSystem!.OneShotAnimationState("fire");
        }
    }

    private void HandleMegaPunch(float deltaTime, float actionsPerSecond, int damage, int tileRange, float knockback)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        var chargeTime = 10;
        var chargeRatio = actionTimer / chargeTime;

        damage = (int)MathHelper.Lerp(damage, damage * 2, chargeRatio);

        knockback = (int)MathHelper.Lerp(knockback, knockback * 3.5f, chargeRatio);

        if (actionTimer >= actionInterval && DetectEnemies(tileRange))
        {
            Shoot(damage, knockback);
            actionTimer = 0f;
            //AnimationSystem!.OneShotAnimationState("fire");
        }
    }

    private void Shoot(int damage, float knockback)
    {
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(Position, tileRange);
        var hitCount = 0;

        foreach (Enemy enemy in enemyCandidates)
        {
            if (IsEnemyInRange(enemy, tileRange))
            {
                hitCount++;
                enemy.HealthSystem.TakeDamage(damage);
                enemy.Knockback(direction,knockback);
            }
        }
    }

    public bool IsEnemyInRange(Enemy enemy, int tileRange)
    {
        var pointA = Position + direction * Grid.TileLength;
        var pointB = Position + direction * tileRange * Grid.TileLength;

        return Collision.IsLineInEntity(pointA, pointB, enemy, out var _,out var _);
    }

    public void HandleRocketGlove(float deltaTime, float actionsPerSecond, int damage, int tileRange, float knockback)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        if (actionTimer >= actionInterval && DetectEnemies(tileRange))
        {
            RocketGlove rocket = new RocketGlove(Game, Position + direction * 8, knockback);
            rocket.Direction = direction;
            actionTimer = 0f;
            //AnimationSystem!.OneShotAnimationState("fire");
        }
    }
    
    public bool DetectEnemies(int tileRange)
    {
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(Position, tileRange);

        foreach (Enemy enemy in enemyCandidates)
        {
            if (IsEnemyInRange(enemy, tileRange)) { return true; }
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
        var sprite = AssetManager.GetTexture("punchtrap_base");

        return new AnimationSystem.AnimationData
        (
            texture: sprite,
            frameCount: 1,
            frameSize: new Vector2(sprite.Width, sprite.Height),
            delaySeconds: 0
        );
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(2, 1);
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.PunchTrap;
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        return TowerCore.DefaultCanPlaceTower(GetDefaultGridSize(), targetWorldPosition);
    }

    public static Entity CreateNewInstance(Game game, Vector2 worldPosition)
    {
        return new PunchTrap(game, worldPosition);
    }

    public void UpgradeTower(TowerUpgradeNode newUpgrade)
    {
    }
}
