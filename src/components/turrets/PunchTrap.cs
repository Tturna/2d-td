using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
class PunchTrap : Entity, ITower
{
    private TowerCore towerCore;
    private Vector2 spawnOffset = new (0, 11);
    int tileRange = 2;
    Vector2 direction = new Vector2(-1,0);
    float knockback = 10f;
    int damage = 10;
    float actionsPerSecond = 0.333f;
    float actionTimer;

    public enum Upgrade
    {
        NoUpgrade,
        FatFist,
        MegaPunch,
        QuickJabs,
        Chainsaw,
        RocketGlove
    }

    public PunchTrap(Game game, Vector2 position) : base(game, position, GetTowerAnimationData())
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

        var MegaPunch = new TowerUpgradeNode(Upgrade.MegaPunch.ToString(), price: 45);
        var FatFist = new TowerUpgradeNode(Upgrade.FatFist.ToString(), price: 10,leftChild: MegaPunch);
        

        var Chainsaw = new TowerUpgradeNode(Upgrade.Chainsaw.ToString(), price: 40);
        var RocketGlove = new TowerUpgradeNode(Upgrade.RocketGlove.ToString(), price: 50);
        var QuickJabs = new TowerUpgradeNode(Upgrade.QuickJabs.ToString(), price: 10, leftChild: RocketGlove, rightChild: Chainsaw);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), price: 0, parent: null,
            leftChild: FatFist, rightChild: QuickJabs);

        towerCore.CurrentUpgrade = defaultNode;
    }

    public override void Initialize()
    {
        // Offset so the tower is flat on the ground
        Position += Vector2.UnitY * 3;

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage, tileRange,knockback);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.FatFist.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond, damage+10, tileRange,knockback*1.5f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.MegaPunch.ToString())
        {
            HandleMegaPunch(deltaTime, actionsPerSecond, damage+20, tileRange,knockback*1.5f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.QuickJabs.ToString())
        {
            HandleBasicShots(deltaTime, actionsPerSecond+.167f, damage, tileRange,knockback);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Chainsaw.ToString())
        {
            HandleBasicShots(deltaTime, 10f, 10, tileRange-1,0f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.RocketGlove.ToString())
        {
            HandleRocketGlove(deltaTime, actionsPerSecond+5, 15, tileRange,1);
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
            AnimationSystem!.OneShotAnimationState("fire");
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
            AnimationSystem!.OneShotAnimationState("fire");
        }
    }

    private void Shoot(int damage, float knockback)
    {
        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            if (IsEnemyInRange(enemy, tileRange))
            {
                enemy.HealthSystem.TakeDamage(damage);
            }
        }
    }

    public bool IsEnemyInRange(Enemy enemy, int tileRange)
    {
        var range = tileRange * Grid.TileLength;
        var towerToEnemy = new Vector2(enemy.Position.X - Position.X, enemy.Position.Y - Position.Y);
        float alignment = Vector2.Dot(direction, towerToEnemy);
        float distance = towerToEnemy.Length();

        return alignment > 0 && distance <= range;
    }

    public void HandleRocketGlove(float deltaTime, float actionsPerSecond, int damage, int tileRange, float knockback)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        if (actionTimer >= actionInterval && DetectEnemies(tileRange))
        {
            var rocket = new Projectile(Game, Position + spawnOffset);
            rocket.Position = spawnOffset;
            rocket.Direction = direction;
            rocket.Lifetime = 2f;
            rocket.Damage = damage;
            rocket.BulletPixelsPerSecond = 300f;
            rocket.Lifetime = 2f;
            actionTimer = 0f;
            AnimationSystem!.OneShotAnimationState("fire");
        }
    }
    
    public bool DetectEnemies(int tileRange)
    {
        foreach (Enemy enemy in EnemySystem.Enemies)
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

    public static AnimationSystem.AnimationData GetTowerAnimationData()
    {
        var sprite = AssetManager.GetTexture("gunturret_base");

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
        return new Vector2(3, 2);
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
}
