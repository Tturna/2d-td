using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Mortar : Entity, ITower
{
    private TowerCore towerCore;
    private Entity turretHead;
    private Vector2 turretHeadAxisCenter;
    private float actionsPerSecond = 0.5f;
    private float actionTimer;
    private bool isTargeting, canSetTarget;
    private Vector2 projectileVelocity;
    private float projectileGravity = 10f;

    private const float FiringAngle = MathHelper.PiOver4;
    private Random random = new();

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

    public Mortar(Game game) : base(game, GetTowerBaseSprite())
    {
        towerCore = new TowerCore(this);
        towerCore.Clicked += OnClickTower;

        var bouncingBomb = new TowerUpgradeNode(Upgrade.BouncingBomb.ToString(), price: 80);
        var nuke = new TowerUpgradeNode(Upgrade.Nuke.ToString(), price: 200);

        var bigBomb = new TowerUpgradeNode(Upgrade.BigBomb.ToString(), price: 35,
            leftChild: bouncingBomb, rightChild: nuke);

        var missileSilo = new TowerUpgradeNode(Upgrade.MissileSilo.ToString(), price: 90);
        var hellrain = new TowerUpgradeNode(Upgrade.Hellrain.ToString(), price: 110);
        var efficientReload = new TowerUpgradeNode(Upgrade.EfficientReload.ToString(), price: 20,
            leftChild: missileSilo, rightChild: hellrain);

        var defaultUpgrade = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), price: 0,
            leftChild: bigBomb, rightChild: efficientReload);

        towerCore.CurrentUpgrade = defaultUpgrade;
    }

    public override void Initialize()
    {
        // Position turret head to match where turret base expects it.
        float TurretHeadXOffset = Sprite!.Width * 0.7f;
        float TurretHeadYOffset = 9f;
        turretHeadAxisCenter = Position + new Vector2(TurretHeadXOffset, TurretHeadYOffset);

        // Offset turret base pos by 2 pixels;
        Position += Vector2.UnitX * 2;

        turretHead = new Entity(Game, turretHeadAxisCenter, AssetManager.GetTexture("gunTurretHead"));

        // Draw turret head with the origin in its axis of rotation
        const float TurretHeadDrawXOffset = 0.85f;
        var drawOrigin = new Vector2(turretHead!.Sprite!.Width * TurretHeadDrawXOffset, turretHead.Sprite.Height / 2);

        turretHead.DrawOrigin = drawOrigin;
        turretHead.DrawLayerDepth = 0.8f;
        turretHead.RotationRadians = MathHelper.PiOver2 - MathHelper.PiOver4;

        Game.Components.Add(turretHead);
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (InputSystem.IsRightMouseButtonClicked())
        {
            isTargeting = false;
        }

        if (canSetTarget && InputSystem.IsLeftMouseButtonClicked())
        {
            projectileVelocity = CalculateProjectileVelocity(InputSystem.GetMouseWorldPosition(), deltaTime);
            isTargeting = false;
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

            return;
        }

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShot(explosionTileRadius: 4, damage: 25, actionsPerSecond);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.BigBomb.ToString())
        {
            HandleBasicShot(explosionTileRadius: 6, damage: 35, actionsPerSecond);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.BouncingBomb.ToString())
        {
            HandleBouncingBomb(explosionTileRadius: 6, damage: 50, actionsPerSecond);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Nuke.ToString())
        {
            HandleBouncingBomb(explosionTileRadius: 16, damage: 350, actionsPerSecond - 0.3f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.EfficientReload.ToString())
        {
            HandleBasicShot(explosionTileRadius: 4, damage: 25, actionsPerSecond + 0.3f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.MissileSilo.ToString())
        {
            HandleMissileSilo(explosionTileRadius: 4, damage: 30, actionsPerSecond + 0.3f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Hellrain.ToString())
        {
            HandleHellrain(explosionTileRadius: 3, damage: 25, actionsPerSecond - 0.3f);
        }

        base.Update(gameTime);
    }

    private void HandleBasicShot(int explosionTileRadius, int damage, float shotsPerSecond)
    {
        if (projectileVelocity == default) return;

        var shell = new MortarShell(Game);
        shell.Position = Position;
        shell.physics.LocalGravity = projectileGravity;
        shell.physics.DragFactor = 0f;
        shell.physics.AddForce(projectileVelocity);

        shell.Destroyed += _ => HandleBasicProjectileHit(shell, damage, explosionTileRadius);

        actionTimer = 1f / actionsPerSecond;
    }

    private void HandleBouncingBomb(int explosionTileRadius, int damage, float shotsPerSecond)
    {
        if (projectileVelocity == default) return;

        var shell = new MortarShell(Game);
        shell.Position = Position;
        shell.physics.LocalGravity = projectileGravity;
        shell.physics.DragFactor = 0f;
        shell.physics.AddForce(projectileVelocity);

        HandleBouncingHit(shell, damage, explosionTileRadius, bounceCount: 3);

        actionTimer = 1f / actionsPerSecond;
    }

    private void HandleMissileSilo(int explosionTileRadius, int damage, float shotsPerSecond)
    {
        if (projectileVelocity == default) return;

        for (int i = 0; i < 3; i++)
        {
            var shell = new MortarShell(Game);
            var xOffset = i * Grid.TileLength;
            shell.Position = Position + Vector2.UnitX * xOffset;
            shell.physics.LocalGravity = 0f;
            shell.Homing = true;

            var randomX = (float)random.NextDouble() * 2f - 1f;
            shell.physics.AddForce(-Vector2.UnitY * 4f + Vector2.UnitX * randomX);

            shell.Destroyed += _ => HandleBasicProjectileHit(shell, damage, explosionTileRadius);
        }

        actionTimer = 1f / actionsPerSecond;
    }

    private void HandleHellrain(int explosionTileRadius, int damage, float shotsPerSecond)
    {
        if (projectileVelocity == default) return;

        for (int i = 0; i < 6; i++)
        {
            var shell = new MortarShell(Game);
            shell.Position = Position;
            shell.physics.LocalGravity = projectileGravity;
            shell.physics.DragFactor = 0f;

            var randomX = (float)random.NextDouble() * 2f - 1f;
            var randomY = (float)random.NextDouble() * 1f - 0.5f;
            var randomAddition = new Vector2(randomX, randomY);
            shell.physics.AddForce(projectileVelocity + randomAddition);

            shell.Destroyed += _ => HandleBasicProjectileHit(shell, damage, explosionTileRadius);
        }

        actionTimer = 1f / actionsPerSecond;
    }

    private void HandleBasicProjectileHit(MortarShell shell, int damage, int explosionTileRadius)
    {
        for (int i = EnemySystem.Enemies.Count - 1; i >= 0; i--)
        {
            if (i >= EnemySystem.Enemies.Count) continue;

            var enemy = EnemySystem.Enemies[i];

            var diff = shell.Position + shell.Size / 2 - enemy.Position + enemy.Size / 2;
            var distance = diff.Length();

            if (distance > explosionTileRadius * Grid.TileLength) continue;

            enemy.HealthSystem.TakeDamage(damage);
        }
    }

    private void HandleBouncingHit(MortarShell shell, int damage, int explosionTileRadius, int bounceCount)
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

            newShell.Position = shell.Position;
            newShell.physics.LocalGravity = projectileGravity;
            newShell.physics.DragFactor = 0f;
            newShell.physics.AddForce(newVelocity);

            HandleBasicProjectileHit(shell, damage, explosionTileRadius);
            HandleBouncingHit(newShell, damage, explosionTileRadius, bounceCount - 1);
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
        var velocityMagnitude = MathF.Sqrt(projectileGravity * dx * deltaTime / MathF.Sin(2 * FiringAngle));

        // Adjust for height difference
        float heightFactor = 1.0f - (dy / (3 * dx));
        float adjustedVelocity = velocityMagnitude * MathHelper.Clamp(heightFactor, 0.1f, 5.0f);

        float vx = -adjustedVelocity * MathF.Cos(FiringAngle);
        float vy = -adjustedVelocity * MathF.Sin(FiringAngle);

        return new Vector2(vx, vy);
    }

    private void OnClickTower()
    {
        isTargeting = true;
    }

    public override void Destroy()
    {
        Game.Components.Remove(turretHead);

        base.Destroy();
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        return TowerCore.DefaultCanPlaceTower(GetDefaultGridSize(), targetWorldPosition);
    }

    public static Entity CreateNewInstance(Game game)
    {
        return new Mortar(game);
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(2, 2);
    }

    public static Texture2D GetTowerBaseSprite()
    {
        return AssetManager.GetTexture("gunTurretBase");
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Mortar;
    }
}
