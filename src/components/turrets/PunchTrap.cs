using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

#nullable enable
class PunchTrap : Entity, ITower
{
    private TowerCore towerCore;
    private static int baseTileRange = 2;
    private Vector2 direction = new Vector2(-1,0);
    private float knockback = 4f;
    private int damage = 10;
    private float actionsPerSecond = 0.333f;
    private float actionTimer;

    private float chainsawTurnoffDelay = 0.5f;
    private float chainsawTurnoffTimer;
    private float chainsawStartTimer;

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

    public PunchTrap(Game game, Vector2 position) : base(game, position, GetUnupgradedBaseAnimationData())
    {
        var fireAnimationTexture = AssetManager.GetTexture("punchtrap_base_fire");

        var fireAnimation = new AnimationSystem.AnimationData
        (
            texture: fireAnimationTexture,
            frameCount: 9,
            frameSize: new Vector2(fireAnimationTexture.Width / 9, fireAnimationTexture.Height),
            delaySeconds: 0.07f
        );

        // base constructor defines animation system
        AnimationSystem!.AddAnimationState("fire", fireAnimation);

        towerCore = new TowerCore(this);

        towerCore.Health.SetMaxHealth(500);

        var megaPunchIcon = AssetManager.GetTexture("punchtrap_megapunch_icon");
        var rocketGloveIcon = AssetManager.GetTexture("punchtrap_rocketglove_icon");
        var fatfistIcon = AssetManager.GetTexture("punchtrap_fatfist_icon");
        var chainsawIcon = AssetManager.GetTexture("punchtrap_chainsaw_icon");
        var flurryOfBlowsIcon = AssetManager.GetTexture("punchtrap_flurryofblows_icon");
        var quickJabsIcon = AssetManager.GetTexture("punchtrap_quickjabs_icon");

        var megaPunch = new TowerUpgradeNode(Upgrade.MegaPunch.ToString(),
            upgradeIcon: megaPunchIcon, price: 75);
        var rocketGlove = new TowerUpgradeNode(Upgrade.RocketGlove.ToString(),
            upgradeIcon: rocketGloveIcon, price: 80);
        var fatFist = new TowerUpgradeNode(Upgrade.FatFist.ToString(),
            upgradeIcon: fatfistIcon, price: 10,leftChild: megaPunch,rightChild: rocketGlove);

        var chainsaw = new TowerUpgradeNode(Upgrade.Chainsaw.ToString(),
            upgradeIcon: chainsawIcon, price: 70);
        var flurryOfBlows = new TowerUpgradeNode(Upgrade.FlurryOfBlows.ToString(),
            upgradeIcon: flurryOfBlowsIcon, price: 80);
        var quickJabs = new TowerUpgradeNode(Upgrade.QuickJabs.ToString(),
            upgradeIcon: quickJabsIcon, price: 10,leftChild: flurryOfBlows, rightChild: chainsaw);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(),
            upgradeIcon: null, price: 0, parent: null, leftChild: fatFist, rightChild: quickJabs);

        fatFist.Description = "+10 damage\n+1.5x knockback";
        quickJabs.Description = "-1 second recharge";
        megaPunch.Description = "+20 damage\n1.5x knockback\nCharges up the longer it\ngoes without firing,\nup to +100 damage and\n+250% knockback.";
        rocketGlove.Description = "The first flies out and\nexplodes for 60 damage.";
        flurryOfBlows.Description = "0.5 second recharge";
        chainsaw.Description = "Fist is replaced with a chainsaw\nand deals 100 damage over 1s,\nbut has no knockback.";

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
        if (InputSystem.IsKeyTapped(Keys.Up))
        {
            UpdatePosition(-Vector2.UnitY);
        }
        else if (InputSystem.IsKeyTapped(Keys.Down))
        {
            UpdatePosition(Vector2.UnitY);
        }
        else if (InputSystem.IsKeyTapped(Keys.Left))
        {
            UpdatePosition(-Vector2.UnitX);
        }
        else if (InputSystem.IsKeyTapped(Keys.Right))
        {
            UpdatePosition(Vector2.UnitX);
        }

        if (towerCore.Health.CurrentHealth <= 0) return;

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.FatFist.ToString())
        {
            HandleBasicShots(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.MegaPunch.ToString())
        {
            HandleMegaPunch(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.QuickJabs.ToString())
        {
            HandleBasicShots(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Chainsaw.ToString())
        {
            HandleChainsaw(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.RocketGlove.ToString())
        {
            HandleRocketGlove(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.FlurryOfBlows.ToString())
        {
            HandleBasicShots(deltaTime);
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        towerCore.Health.DrawHealthBar(Position + new Vector2(Size.X / 2, -4));

        base.Draw(gameTime);
    }

    private void HandleBasicShots(float deltaTime)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        if (actionTimer >= actionInterval && DetectEnemies(baseTileRange))
        {
            Shoot();
            actionTimer = 0f;
            AnimationSystem!.OneShotAnimationState("fire");
        }
    }

    private void HandleChainsaw(float deltaTime)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        if (chainsawStartTimer > 0)
        {
            chainsawStartTimer -= deltaTime;

            if (chainsawStartTimer <= 0)
            {
                AnimationSystem!.ToggleAnimationState("firing");
            }
        }

        if (actionTimer >= actionInterval && DetectEnemies(baseTileRange))
        {
            Shoot();
            actionTimer = 0f;
            
            if (chainsawTurnoffTimer <= 0)
            {
                AnimationSystem!.OneShotAnimationState("fire");
                var animation = AnimationSystem.CurrentAnimationData;
                chainsawStartTimer = animation.FrameCount * animation.DelaySeconds;
            }

            chainsawTurnoffTimer = chainsawTurnoffDelay;
        }
        else if (chainsawTurnoffTimer > 0)
        {
            chainsawTurnoffTimer -= deltaTime;

            if (chainsawTurnoffTimer <= 0)
            {
                AnimationSystem!.ToggleAnimationState(null);
            }
        }
    }

    private void HandleMegaPunch(float deltaTime)
    {
        var actionInterval = 1f / actionsPerSecond;
        actionTimer += deltaTime;

        var chargeTime = 10;
        var chargeRatio = actionTimer / chargeTime;

        damage = (int)MathHelper.Lerp(damage, damage + 100, chargeRatio);

        knockback = (int)MathHelper.Lerp(knockback, knockback * 3.5f, chargeRatio);

        if (actionTimer >= actionInterval && DetectEnemies(baseTileRange))
        {
            Shoot();
            actionTimer = 0f;
            AnimationSystem!.OneShotAnimationState("fire");
        }
    }

    private void Shoot()
    {
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(Position, baseTileRange * Grid.TileLength);

        foreach (Enemy enemy in enemyCandidates)
        {
            if (IsEnemyInRange(enemy, baseTileRange))
            {
                enemy.HealthSystem.TakeDamage(this, damage);
                var finalKnockback = direction * knockback - Vector2.UnitY * (knockback);
                enemy.UpdatePosition(-Vector2.One);
                enemy.ApplyKnockback(finalKnockback);
            }
        }
    }

    public bool IsEnemyInRange(Enemy enemy, int tileRange)
    {
        var pointA = Position + new Vector2(0, Size.Y / 2) + direction;
        var pointB = Position + new Vector2(0, Size.Y / 2) + direction * tileRange * Grid.TileLength;

        return Collision.IsLineInEntity(pointA, pointB, enemy, out var _,out var _);
    }

    public void HandleRocketGlove(float deltaTime)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        if (actionTimer >= actionInterval && DetectEnemies(baseTileRange))
        {
            RocketGlove rocket = new RocketGlove(Game, Position + direction * 8, knockback);
            rocket.Direction = direction;
            rocket.Damage = damage;
            rocket.ExplosionTileRadius = 3;
            actionTimer = 0f;
            AnimationSystem!.OneShotAnimationState("fire");
        }
    }
    
    public bool DetectEnemies(int tileRange)
    {
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(Position, tileRange * Grid.TileLength);

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

    public static AnimationSystem.AnimationData GetUnupgradedBaseAnimationData()
    {
        var sprite = AssetManager.GetTexture("punchtrap_base_idle");

        return new AnimationSystem.AnimationData
        (
            texture: sprite,
            frameCount: 1,
            frameSize: new Vector2(sprite.Width, sprite.Height),
            delaySeconds: 0
        );
    }

    public static List<KeyValuePair<UIEntity, Vector2>> GetUnupgradedPartIcons(List<UIEntity> uiElements)
    {
        var baseData = GetUnupgradedBaseAnimationData();

        var baseEntity = new UIEntity(Game1.Instance, uiElements, Vector2.Zero, baseData);

        var list = new List<KeyValuePair<UIEntity, Vector2>>();
        list.Add(KeyValuePair.Create(baseEntity, Vector2.Zero));

        return list;
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
        Texture2D newIdleTexture;
        Texture2D newFireTexture;
        var newIdleFrameCount = 1;
        var newFireFrameCount = 1;

        if (newUpgrade.Name == Upgrade.FatFist.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("punchtrap_fatfist_idle");
            newFireTexture = AssetManager.GetTexture("punchtrap_fatfist_fire");
            newIdleFrameCount = 1;
            newFireFrameCount = 9;

            damage += 10;
            knockback *= 1.5f;
            UpdatePosition(-Vector2.One);
        }
        else if (newUpgrade.Name == Upgrade.QuickJabs.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("punchtrap_quickjabs_idle");
            newFireTexture = AssetManager.GetTexture("punchtrap_quickjabs_fire");
            newIdleFrameCount = 1;
            newFireFrameCount = 7;

            actionsPerSecond = 0.5f;
        }
        else if (newUpgrade.Name == Upgrade.MegaPunch.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("punchtrap_fatfist_idle");
            newFireTexture = AssetManager.GetTexture("punchtrap_megapunch_fire");
            newIdleFrameCount = 1;
            newFireFrameCount = 6;

            damage += 20;
            knockback *= 1.5f;
        }
        else if (newUpgrade.Name == Upgrade.RocketGlove.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("punchtrap_rocketglove_idle");
            newFireTexture = AssetManager.GetTexture("punchtrap_rocketglove_fire");
            newIdleFrameCount = 2;
            newFireFrameCount = 7;

            damage = 60;
            UpdatePosition(-Vector2.One);
        }
        else if (newUpgrade.Name == Upgrade.FlurryOfBlows.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("punchtrap_base_idle");
            newFireTexture = AssetManager.GetTexture("punchtrap_flurryofblows_fire");
            newIdleFrameCount = 1;
            newFireFrameCount = 4;

            actionsPerSecond = 2f;
        }
        else
        {
            newIdleTexture = AssetManager.GetTexture("punchtrap_chainsaw_idle");
            newFireTexture = AssetManager.GetTexture("punchtrap_chainsaw_fire");
            var firingTexture = AssetManager.GetTexture("punchtrap_chainsaw_firing");
            newIdleFrameCount = 1;
            newFireFrameCount = 3;

            var firingAnimation = new AnimationSystem.AnimationData
            (
                texture: firingTexture,
                frameCount: 2,
                frameSize: new Vector2(firingTexture.Width / 2, firingTexture.Height),
                delaySeconds: 0.05f
            );

            AnimationSystem!.AddAnimationState("firing", firingAnimation);

            damage = 10;
            actionsPerSecond = 10f;
            knockback = 0;
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
            delaySeconds: 0.07f
        );

        AnimationSystem!.ChangeAnimationState(null, newIdleAnimation);
        AnimationSystem.ChangeAnimationState("fire", newFireAnimation);
    }

    public static float GetBaseRange() => baseTileRange;

    public float GetRange()
    {
        return baseTileRange;
    }

    public TowerCore GetTowerCore() => towerCore;

    public static void DrawBaseRangeIndicator(Vector2 worldPosition)
    {
        var towerScreenCenter = Camera.WorldToScreenPosition(worldPosition);
        var towerRange = (int)GetBaseRange();
        var towerTileRange = towerRange * Grid.TileLength;

        LineUtility.DrawCircle(Game1.Instance.SpriteBatch, towerScreenCenter, towerTileRange, Color.White,
            resolution: MathHelper.Max(12, towerRange * 2));
    }

    public void DrawRangeIndicator()
    {
        var towerCenter = Position + Size / 2;
        var towerScreenCenter = Camera.WorldToScreenPosition(towerCenter);
        var towerRange = (int)GetRange();
        var towerTileRange = towerRange * Grid.TileLength;

        LineUtility.DrawCircle(Game.SpriteBatch, towerScreenCenter, towerTileRange, Color.White,
            resolution: MathHelper.Max(12, towerRange * 2));
    }
}
