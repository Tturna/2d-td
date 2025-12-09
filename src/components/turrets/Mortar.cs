using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Mortar : Entity, ITower
{
    private TowerCore towerCore;
    private float actionsPerSecond = 0.5f;
    private float actionTimer;
    private bool isTargeting, canSetTarget;
    private Vector2 projectileVelocity;
    private float projectileGravity = 0.12f;
    private Vector2 spawnOffset = new Vector2(-4, -4);

    private const float FiringAngle = MathHelper.PiOver4;
    private Random random = new();
    private Texture2D projectileSprite = AssetManager.GetTexture("mortar_base_shell");
    private float projectileRotationOffset = MathHelper.Pi;
    private AnimationSystem.AnimationData? explosionAnimation;

    public Vector2 TargetHitpoint { get; private set; }
    public static bool IsMortarTargeting;

    public delegate void TargetingHandler(Entity mortar);
    public static event TargetingHandler StartTargeting;
    public static event TargetingHandler EndTargeting;
    public static event TargetingHandler MissingTargeting;

    public enum Upgrade
    {
        NoUpgrade,
        BigBomb,
        EfficientReload,
        BouncingBomb,
        Nuke,
        MissileSilo,
        Hellrain
    }

    public Mortar(Game game, Vector2 position) : base(game, position, GetUnupgradedBaseAnimationData())
    {
        var fireSprite = AssetManager.GetTexture("mortar_base_fire");

        var fireAnimation = new AnimationSystem.AnimationData
        (
            texture: fireSprite,
            frameCount: 5,
            frameSize: new Vector2(fireSprite.Width / 5, fireSprite.Height),
            delaySeconds: 0.05f
        );

        AnimationSystem.AddAnimationState("fire", fireAnimation);

        towerCore = new TowerCore(this);
        towerCore.RightClicked += OnRightClickTower;

        var bouncingBombIcon = AssetManager.GetTexture("mortar_bouncingbomb_icon");
        var nukeIcon = AssetManager.GetTexture("mortar_nuke_icon");
        var bigBombIcon = AssetManager.GetTexture("mortar_heavyshells_icon");
        var missileSiloIcon = AssetManager.GetTexture("mortar_missilesilo_icon");
        var hellRainIcon = AssetManager.GetTexture("mortar_hellrain_icon");
        var efficientReloadIcon = AssetManager.GetTexture("mortar_efficientreload_icon");

        var bouncingBomb = new TowerUpgradeNode(Upgrade.BouncingBomb.ToString(), bouncingBombIcon, price: 80);
        var nuke = new TowerUpgradeNode(Upgrade.Nuke.ToString(), nukeIcon, price: 200);

        var bigBomb = new TowerUpgradeNode(Upgrade.BigBomb.ToString(), bigBombIcon, price: 35,
            leftChild: bouncingBomb, rightChild: nuke);

        var missileSilo = new TowerUpgradeNode(Upgrade.MissileSilo.ToString(), missileSiloIcon, price: 90);
        var hellrain = new TowerUpgradeNode(Upgrade.Hellrain.ToString(), hellRainIcon, price: 110);
        var efficientReload = new TowerUpgradeNode(Upgrade.EfficientReload.ToString(), efficientReloadIcon, price: 20,
            leftChild: missileSilo, rightChild: hellrain);

        var defaultUpgrade = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0,
            leftChild: bigBomb, rightChild: efficientReload);

        bigBomb.Description = "+2 tile radius,\n+10 damage";
        efficientReload.Description = "+0.3 shots/s";
        bouncingBomb.Description = "+10 damage\nProjectile can bounce up to\n3 times, exploding with\neach impact.";
        nuke.Description = "+8 tile explosion radius\n+300 damage\n-0.3 shots/s\nLeaves a radiation cloud\nthat deals 5DPS for 5s.";
        missileSilo.Description = "+2 projectiles\nFires homing missiles directly\nupwards, dealing 30 damage each.";
        hellrain.Description = "-2 tile explosion radius.\nFires in a 6-shot barrage\nwith considerable spread.";

        towerCore.CurrentUpgrade = defaultUpgrade;

        // notify mainly UIComponent that this new mortar doesn't have a target yet,
        // so it needs an indicator.
        MissingTargeting?.Invoke(this);
    }

    public override void Initialize()
    {
        UpdatePosition(-Vector2.UnitY * 4);

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (canSetTarget && InputSystem.IsRightMouseButtonClicked())
        {
            isTargeting = false;
            IsMortarTargeting = false;
            EndTargeting?.Invoke(this);

            if (projectileVelocity == default)
            {
                MissingTargeting?.Invoke(this);
            }
        }

        if (canSetTarget && InputSystem.IsLeftMouseButtonClicked())
        {
            TargetHitpoint = InputSystem.GetMouseWorldPosition();
            projectileVelocity = CalculateProjectileVelocity(TargetHitpoint, deltaTime);
            isTargeting = false;
            IsMortarTargeting = false;
            EndTargeting?.Invoke(this);
        }

        // Used to prevent one click from both enabling targeting and setting the target.
        canSetTarget = isTargeting;

        if (actionTimer > 0)
        {
            actionTimer -= deltaTime;

            if (actionTimer <= 0)
            {
                actionTimer = 0;
            }

            base.Update(gameTime);
            return;
        }

        if (WaveSystem.WaveCooldownLeft <= 0)
        {
            if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
            {
                HandleBasicShot(explosionTileRadius: 4, damage: 25, actionsPerSecond, deltaTime);
            }
            else if (towerCore.CurrentUpgrade.Name == Upgrade.BigBomb.ToString())
            {
                HandleBasicShot(explosionTileRadius: 6, damage: 35, actionsPerSecond, deltaTime);
            }
            else if (towerCore.CurrentUpgrade.Name == Upgrade.BouncingBomb.ToString())
            {
                HandleBouncingBomb(explosionTileRadius: 6, damage: 50, actionsPerSecond, deltaTime);
            }
            else if (towerCore.CurrentUpgrade.Name == Upgrade.Nuke.ToString())
            {
                HandleBasicShot(explosionTileRadius: 16, damage: 350, actionsPerSecond - 0.3f, deltaTime);
            }
            else if (towerCore.CurrentUpgrade.Name == Upgrade.EfficientReload.ToString())
            {
                HandleBasicShot(explosionTileRadius: 4, damage: 25, actionsPerSecond + 0.3f, deltaTime);
            }
            else if (towerCore.CurrentUpgrade.Name == Upgrade.MissileSilo.ToString())
            {
                HandleMissileSilo(explosionTileRadius: 4, damage: 30, actionsPerSecond + 0.3f, deltaTime);
            }
            else if (towerCore.CurrentUpgrade.Name == Upgrade.Hellrain.ToString())
            {
                HandleHellrain(explosionTileRadius: 3, damage: 25, actionsPerSecond - 0.3f, deltaTime);
            }
        }

        base.Update(gameTime);
    }

    private void HandleBasicShot(int explosionTileRadius, int damage, float shotsPerSecond, float deltaTime)
    {
        if (projectileVelocity == default) return;

        var shell = new MortarShell(Game);
        shell.SetPosition(Position + spawnOffset);
        shell.physics.LocalGravity = projectileGravity;
        shell.physics.DragFactor = 0f;
        shell.physics.AddForce(projectileVelocity);
        shell.Sprite = projectileSprite;
        shell.Size = new Vector2(projectileSprite.Width, projectileSprite.Height);
        shell.RotationOffset = projectileRotationOffset;

        shell.Destroyed += _ => HandleBasicProjectileHit(shell, damage, explosionTileRadius, deltaTime);

        actionTimer = 1f / actionsPerSecond;

        AnimationSystem.OneShotAnimationState("fire");
        ParticleSystem.PlayShotSmokeEffect(Position + spawnOffset);
    }

    private void HandleBouncingBomb(int explosionTileRadius, int damage, float shotsPerSecond, float deltaTime)
    {
        if (projectileVelocity == default) return;

        var shell = new MortarShell(Game);
        shell.SetPosition(Position + spawnOffset);
        shell.physics.LocalGravity = projectileGravity;
        shell.physics.DragFactor = 0f;
        shell.physics.AddForce(projectileVelocity);
        shell.Sprite = projectileSprite;
        shell.Size = new Vector2(projectileSprite.Width, projectileSprite.Height);
        shell.RotationOffset = projectileRotationOffset;

        HandleBouncingHit(shell, damage, explosionTileRadius, bounceCount: 3, deltaTime);

        actionTimer = 1f / actionsPerSecond;

        AnimationSystem.OneShotAnimationState("fire");
        ParticleSystem.PlayShotSmokeEffect(Position + spawnOffset);
    }

    private void HandleMissileSilo(int explosionTileRadius, int damage, float shotsPerSecond, float deltaTime)
    {
        if (projectileVelocity == default) return;

        for (int i = 0; i < 3; i++)
        {
            var shell = new MortarShell(Game);
            var xOffset = i * Grid.TileLength;
            shell.SetPosition(Position + Vector2.UnitX * xOffset);
            shell.physics.LocalGravity = 0f;
            shell.Homing = true;
            shell.Sprite = projectileSprite;
            shell.Size = new Vector2(projectileSprite.Width, projectileSprite.Height);
            shell.RotationOffset = projectileRotationOffset;

            var randomX = (float)random.NextDouble() * 2f - 1f;
            shell.physics.AddForce(-Vector2.UnitY * 4f + Vector2.UnitX * randomX);

            shell.Destroyed += _ => HandleBasicProjectileHit(shell, damage, explosionTileRadius, deltaTime);
        }

        actionTimer = 1f / actionsPerSecond;

        AnimationSystem.OneShotAnimationState("fire");
        ParticleSystem.PlayShotSmokeEffect(Position + spawnOffset);
    }

    private void HandleHellrain(int explosionTileRadius, int damage, float shotsPerSecond, float deltaTime)
    {
        if (projectileVelocity == default) return;

        for (int i = 0; i < 6; i++)
        {
            var shell = new MortarShell(Game);
            shell.SetPosition(Position);
            shell.physics.LocalGravity = projectileGravity;
            shell.physics.DragFactor = 0f;
            shell.Sprite = projectileSprite;
            shell.Size = new Vector2(projectileSprite.Width, projectileSprite.Height);
            shell.RotationOffset = projectileRotationOffset;

            var randomX = (float)random.NextDouble() * 2f - 1f;
            var randomY = (float)random.NextDouble() * 1f - 0.5f;
            var randomAddition = new Vector2(randomX, randomY);
            shell.physics.AddForce(projectileVelocity + randomAddition);

            shell.Destroyed += _ => HandleBasicProjectileHit(shell, damage, explosionTileRadius, deltaTime);
        }

        actionTimer = 1f / actionsPerSecond;

        AnimationSystem.OneShotAnimationState("fire");
        ParticleSystem.PlayShotSmokeEffect(Position + spawnOffset);
    }

    private void HandleBasicProjectileHit(MortarShell shell, int damage, int explosionTileRadius, float deltaTime)
    {
        EffectUtility.Explode(shell.Position + shell.Size / 2, explosionTileRadius * Grid.TileLength,
            magnitude: 10f, damage, animation: explosionAnimation);
    }

    private void HandleBouncingHit(MortarShell shell, int damage, int explosionTileRadius, int bounceCount,
        float deltaTime)
    {
        if (bounceCount <= 0) return;

        shell.Destroyed += previousVelocity =>
        {
            // TODO: Make shells bounce backwards from walls
            float xVelocity = -1;
            var yVelocity = previousVelocity.Y > 0 ? -1 : 1;
            var newVelocityDirection = new Vector2(xVelocity, yVelocity);
            newVelocityDirection.Normalize();
            var newVelocity = newVelocityDirection * previousVelocity.Length();
            var newShell = new MortarShell(Game);

            newShell.SetPosition(shell.Position);
            newShell.physics.LocalGravity = projectileGravity;
            newShell.physics.DragFactor = 0f;
            newShell.physics.AddForce(newVelocity);
            newShell.Sprite = shell.Sprite;
            newShell.Size = shell.Size;
            newShell.RotationOffset = shell.RotationOffset;

            HandleBasicProjectileHit(shell, damage, explosionTileRadius, deltaTime);
            HandleBouncingHit(newShell, damage, explosionTileRadius, bounceCount - 1, deltaTime);
        };
    }

    private Vector2 CalculateProjectileVelocity(Vector2 targetStrikePosition, float deltaTime)
    {
        // This was made with Claude AI :)
        float dx = MathF.Abs(targetStrikePosition.X - Position.X);
        float dy = targetStrikePosition.Y - Position.Y;

        // Range formula for a projectile:
        // Range = (v₀² × sin(2 × θ)) / g
        // When θ = 45°, Range = v₀² / g
        // Knowing the range, the velocity can be solved by rearranging the formula:
        // v₀ = √(Range × g / sin(2 × θ))
        var velocityMagnitude = MathF.Sqrt(projectileGravity * dx / MathF.Sin(2 * FiringAngle));

        // Adjust for height difference
        float heightFactor = 1.0f - (dy / (3 * dx));
        float adjustedVelocity = velocityMagnitude * MathHelper.Clamp(heightFactor, 0.1f, 5.0f);

        float vx = -adjustedVelocity * MathF.Cos(FiringAngle);
        float vy = -adjustedVelocity * MathF.Sin(FiringAngle);

        return new Vector2(vx, vy);
    }

    private void OnRightClickTower()
    {
        if (Mortar.IsMortarTargeting) return;
        if (!towerCore.detailsClosed) return;
        if (BuildingSystem.IsPlacingTower) return;

        if (!isTargeting)
        {
            StartTargeting?.Invoke(this);
        }

        isTargeting = true;
        IsMortarTargeting = true;
    }

    public override void Destroy()
    {
        towerCore.CloseDetailsView();
        Game.Components.Remove(towerCore);

        base.Destroy();
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        return TowerCore.DefaultCanPlaceTower(GetDefaultGridSize(), targetWorldPosition);
    }

    public static Entity CreateNewInstance(Game game, Vector2 worldPosition)
    {
        return new Mortar(game, worldPosition);
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(2, 2);
    }

    public static AnimationSystem.AnimationData GetUnupgradedBaseAnimationData()
    {
        var sprite = AssetManager.GetTexture("mortar_base_idle");

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

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Mortar;
    }

    public void UpgradeTower(TowerUpgradeNode newUpgrade)
    {
        Texture2D newIdleTexture;
        Texture2D newFireTexture;
        int newIdleFrameCount;
        int newFireFrameCount;

        if (newUpgrade.Name == Upgrade.BouncingBomb.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("mortar_bouncingbomb_idle");
            newFireTexture = AssetManager.GetTexture("mortar_bouncingbomb_fire");
            newIdleFrameCount = 1;
            newFireFrameCount = 3;
            projectileSprite = AssetManager.GetTexture("mortar_bouncingbomb_shell");

            var newExplosionSprite = AssetManager.GetTexture("mortar_bouncingbomb_explosion");
            var newExplosionAnimation = new AnimationSystem.AnimationData(
                texture: newExplosionSprite,
                frameCount: 7,
                frameSize: new Vector2(newExplosionSprite.Width / 7, newExplosionSprite.Height),
                delaySeconds: 0.075f);

            explosionAnimation = newExplosionAnimation;
        }
        else if (newUpgrade.Name == Upgrade.EfficientReload.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("mortar_efficientreload_idle");
            newFireTexture = AssetManager.GetTexture("mortar_efficientreload_fire");
            newIdleFrameCount = 1;
            newFireFrameCount = 3;
        }
        else if (newUpgrade.Name == Upgrade.BigBomb.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("mortar_heavyshells_idle");
            newFireTexture = AssetManager.GetTexture("mortar_heavyshells_fire");
            newIdleFrameCount = 1;
            newFireFrameCount = 5;
            projectileSprite = AssetManager.GetTexture("mortar_heavyshells_shell");
        }
        else if (newUpgrade.Name == Upgrade.Hellrain.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("mortar_hellrain_idle");
            newFireTexture = AssetManager.GetTexture("mortar_hellrain_fire");
            newIdleFrameCount = 1;
            newFireFrameCount = 7;
            projectileSprite = AssetManager.GetTexture("mortar_hellrain_shell");

            var newExplosionSprite = AssetManager.GetTexture("mortar_hellrain_explosion");
            var newExplosionAnimation = new AnimationSystem.AnimationData(
                texture: newExplosionSprite,
                frameCount: 4,
                frameSize: new Vector2(newExplosionSprite.Width / 4, newExplosionSprite.Height),
                delaySeconds: 0.075f);

            explosionAnimation = newExplosionAnimation;
        }
        else if (newUpgrade.Name == Upgrade.MissileSilo.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("mortar_missilesilo_idle");
            newFireTexture = AssetManager.GetTexture("mortar_missilesilo_fire");
            newIdleFrameCount = 1;
            newFireFrameCount = 4;
            projectileSprite = AssetManager.GetTexture("mortar_missilesilo_shell");
        }
        else
        {
            newIdleTexture = AssetManager.GetTexture("mortar_nuke_idle");
            newFireTexture = AssetManager.GetTexture("mortar_nuke_fire");
            newIdleFrameCount = 1;
            newFireFrameCount = 7;
            projectileSprite = AssetManager.GetTexture("mortar_nuke_shell");

            var newExplosionSprite = AssetManager.GetTexture("mortar_nuke_explosion");
            var newExplosionAnimation = new AnimationSystem.AnimationData(
                texture: newExplosionSprite,
                frameCount: 11,
                frameSize: new Vector2(newExplosionSprite.Width / 11, newExplosionSprite.Height),
                delaySeconds: 0.1f);

            explosionAnimation = newExplosionAnimation;
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

        AnimationSystem.ChangeAnimationState(null, newIdleAnimation);
        AnimationSystem.ChangeAnimationState("fire", newFireAnimation);
    }

    public static float GetBaseRange() => 0f;

    public float GetRange() => 0f;
}
