using System;
using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
class Hovership : Entity, ITower
{
    private TowerCore towerCore;
    private Entity turretHovership;
    private Vector2 bombSpawnOffset = new Vector2(0, 8);
    private int baseTargetHoverTileHeight = 10;
    private int realTargetHoverTileHeight;
    private static int baseTileRange = 20;
    private int realTileRange;
    private int baseDamage = 15;
    private int baseProjectileAmount = 3;
    private int realProjectileAmount;
    private int spawnedBombsDuringBarrage;
    private float baseHovershipSpeed = 50f;
    private float realHovershipSpeed;
    private float actionsPerSecond = 1f;
    private float actionTimer;
    private float bombSpawnInterval = 0.3f;
    private float bombSpawnTimer;
    private Texture2D bombSprite;
    private bool isOverEnemy;
    private bool shouldReturnToBase;

    private float orbitalLaserBuildupTimer;
    private float orbitalLaserFireTime = 4f;
    private float orbitalLaserFireTimer;
    private float orbitalLaserDamageInterval = 1f / 12f;
    private float orbitalLaserDamageTimer;
    private AnimationSystem.AnimationData currentOrbitalLaserAnimationData;
    private Entity? orbitalLaserBeam;
    private Entity? orbitalLaserImpact;

    private Entity? ufoTractorBeam;
    // stores pairs of enemy and its random relative position in the tractor beam while flying
    private Dictionary<Enemy, Vector2> ufoCarriedEnemies = new();
    private float ufoSeekTime = 3f;
    private float ufoSeekTimer;

    private Random random = new();

    public enum Upgrade
    {
        NoUpgrade,
        BombierBay,
        OrbitalLaser,
        CarpetofFire,
        EfficientEngines,
        EMPShip,
        UFO
    }

    public Hovership(Game game, Vector2 position) : base(game, position, GetUnupgradedPlatformAnimation())
    {
        towerCore = new TowerCore(this);

        var orbitalLaserIcon = AssetManager.GetTexture("hovership_orbitallaser_icon");
        var carpetOfFireIcon = AssetManager.GetTexture("hovership_carpetoffire_icon");
        var bombierBayIcon = AssetManager.GetTexture("hovership_bombierbay_icon");
        var empShipIcon = AssetManager.GetTexture("hovership_emp_icon");
        var ufoIcon = AssetManager.GetTexture("hovership_ufo_icon");
        var efficientEnginesIcon = AssetManager.GetTexture("hovership_efficientengines_icon");

        var OrbitalLaser = new TowerUpgradeNode(Upgrade.OrbitalLaser.ToString(), orbitalLaserIcon, price: 160);
        var CarpetofFire = new TowerUpgradeNode(Upgrade.CarpetofFire.ToString(), carpetOfFireIcon, price: 120);
        var BombierBay = new TowerUpgradeNode(Upgrade.BombierBay.ToString(), bombierBayIcon, price: 40, leftChild: OrbitalLaser, rightChild: CarpetofFire);

        var EMPShip = new TowerUpgradeNode(Upgrade.EMPShip.ToString(), empShipIcon, price: 110);
        var UFO = new TowerUpgradeNode(Upgrade.UFO.ToString(), ufoIcon, price: 200);
        var EfficientEngines = new TowerUpgradeNode(Upgrade.EfficientEngines.ToString(), efficientEnginesIcon, price: 15, leftChild: EMPShip, rightChild: UFO);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0,
            leftChild: BombierBay, rightChild: EfficientEngines);

        BombierBay.Description = "+2 projectiles";
        EfficientEngines.Description = "+10 tile range";
        OrbitalLaser.Description = "-0.85 shots/s\nInstead of bombs,\nfires a massive orbital laser\nthat deals 300 damage\nover 4s.\nUnlimited pierce";
        CarpetofFire.Description = "+3 projectiles.\nProjectiles inflict 1 burn\nstack and leave fire tiles\non the ground that\ndeal 10 DPS for 5s.";
        EMPShip.Description = "-2 projectiles.\n+10 tile hover height.\n+5 tile area of effect\nNow Shocks enemies for 5s.";
        UFO.Description = "Sucks up to 5 bots up toward it and\ndrops them back at the entrance.\nWhile held, they take 10 DPS.";

        towerCore.CurrentUpgrade = defaultNode;

        turretHovership = new Entity(Game, position, GetUnupgradedBaseAnimationData());
        turretHovership.DrawLayerDepth = 0.8f;

        bombSprite = AssetManager.GetTexture("hovership_base_bomb");

        UpdatePosition(Vector2.UnitY * Grid.TileLength);

        realTileRange = baseTileRange;
        realProjectileAmount = baseProjectileAmount;
        realTargetHoverTileHeight = baseTargetHoverTileHeight;
        realHovershipSpeed = baseHovershipSpeed;

        WaveSystem.WaveEnded += () => shouldReturnToBase = true;
    }

    // TODO: Handle upgraded stats sensibly by using variables and updating them in
    // UpgradeTower().
    public override void Update(GameTime gameTime)
    {
        if (towerCore.Health.CurrentHealth <= 0) return;

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleBasicShots(deltaTime);
            HandleHovershipPosition(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.BombierBay.ToString())
        {
            HandleBasicShots(deltaTime);
            HandleHovershipPosition(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.OrbitalLaser.ToString())
        {
            HandleOrbitalLaser(deltaTime);
            HandleHovershipPosition(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.CarpetofFire.ToString())
        {
            HandleBasicShots(deltaTime);
            HandleHovershipPosition(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.EfficientEngines.ToString())
        {
            HandleBasicShots(deltaTime);
            HandleHovershipPosition(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.EMPShip.ToString())
        {
            HandleBasicShots(deltaTime);
            HandleHovershipPosition(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.UFO.ToString())
        {
            HandleUFO(deltaTime);
            HandleHovershipPosition(deltaTime);
        } 

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }

    private void HandleHovershipPosition(float deltaTime)
    {
        var centeredTarget = Vector2.Zero;
        var isUfoSeekDone = ufoCarriedEnemies.Count >= 5 || (ufoSeekTimer > ufoSeekTime && ufoCarriedEnemies.Count > 0);

        if (isUfoSeekDone)
        {
            centeredTarget = Game.Terrain.GetFirstTilePosition();
        }
        else if (shouldReturnToBase)
        {
            centeredTarget = Position + Vector2.UnitX * (Size.X / 2) - turretHovership.Size + Vector2.UnitX * (turretHovership.Size.X / 2);
        }
        else
        {
            var rightmostEnemy = GetRightmostEnemy();

            if (rightmostEnemy is null)
            {
                isOverEnemy = false;
                turretHovership.RotationRadians = MathHelper.Lerp(turretHovership.RotationRadians, 0f, deltaTime * 10f);
                return;
            }

            var targetOffset = ufoTractorBeam is null && orbitalLaserBeam is null ? Vector2.UnitX * 2 * Grid.TileLength : Vector2.Zero;
            centeredTarget = rightmostEnemy.Position + targetOffset + rightmostEnemy.Size / 2 - turretHovership.Size / 2;
        }

        var finalTarget = centeredTarget;

        if (!shouldReturnToBase)
        {
            var rawHeight = isUfoSeekDone ? realTargetHoverTileHeight * 5 : realTargetHoverTileHeight;
            finalTarget -= Vector2.UnitY * (rawHeight * Grid.TileLength);
        }

        var difference = finalTarget - turretHovership.Position;
        var distance = difference.Length();
        var direction = difference;
        direction.Normalize();

        isOverEnemy = distance < Grid.TileLength;

        if (distance > 1f)
        {
            difference.Normalize();
            turretHovership.UpdatePosition(difference * realHovershipSpeed * deltaTime);

            // don't tilt when in ufo or orbital laser mode because the beams looks trash
            if (ufoTractorBeam is null && orbitalLaserBeam is null)
            {
                var targetRotation = direction.X * MathHelper.PiOver4 * MathHelper.Min(distance * 0.1f, 0.5f);
                turretHovership.RotationRadians = MathHelper.Lerp(turretHovership.RotationRadians, targetRotation, deltaTime * 10f);
            }
        }
        else if (shouldReturnToBase)
        {
            shouldReturnToBase = false;
        }
        else if (isUfoSeekDone)
        {
            ufoCarriedEnemies.Clear();
            actionTimer = 0f;
            ufoTractorBeam!.Scale = Vector2.Zero;
            shouldReturnToBase = true;
            ufoSeekTimer = 0f;
        }
    }

    private void HandleBasicShots(float deltaTime)
    {
        var actionInterval = 1f / actionsPerSecond;
        actionTimer += deltaTime;

        if (actionTimer >= actionInterval)
        {
            bombSpawnTimer += deltaTime;
        }

        if (isOverEnemy && actionTimer >= actionInterval && bombSpawnTimer >= bombSpawnInterval)
        {
            spawnedBombsDuringBarrage++;
            var randomXPosition = random.Next(-8, 8);
            var spawnPoint = turretHovership.Position + Size / 2 + bombSpawnOffset + Vector2.UnitX * randomXPosition;
            Shoot(Vector2.UnitY, spawnPoint);
            bombSpawnTimer = 0f;

            if (spawnedBombsDuringBarrage >= realProjectileAmount)
            {
                actionTimer = 0f;
                spawnedBombsDuringBarrage = 0;
            }
        }
    }

    private void HandleOrbitalLaser(float deltaTime)
    {
        var actionInterval = 1f / actionsPerSecond;
        actionTimer += deltaTime;

        if (actionTimer < actionInterval) return;

        if (orbitalLaserBuildupTimer <= 0)
        {
            currentOrbitalLaserAnimationData = turretHovership.AnimationSystem!.ToggleAnimationState("buildup");
        }

        if (orbitalLaserBuildupTimer < currentOrbitalLaserAnimationData.DelaySeconds * currentOrbitalLaserAnimationData.FrameCount)
        {
            orbitalLaserBuildupTimer += deltaTime;
            return;
        }

        if (orbitalLaserFireTimer <= 0)
        {
            turretHovership.AnimationSystem!.ToggleAnimationState("firing");
        }

        if (orbitalLaserFireTimer < orbitalLaserFireTime)
        {
            orbitalLaserFireTimer += deltaTime;

            var shipCenterBottom = turretHovership.Position + new Vector2(turretHovership.Size.X / 2, turretHovership.Size.Y);
            var beamPos = shipCenterBottom - Vector2.UnitX * (orbitalLaserBeam!.Size.X / 2);
            var maxBeamLength = realTargetHoverTileHeight * 2 * Grid.TileLength;

            orbitalLaserBeam!.SetPosition(beamPos);
            orbitalLaserBeam.Scale = new Vector2(1, maxBeamLength);

            var beamEndPoint = shipCenterBottom + Vector2.UnitY * maxBeamLength;

            if (Collision.IsLineInTerrain(shipCenterBottom, beamEndPoint, out var entryPoint, out var _))
            {
                orbitalLaserImpact!.SetPosition(entryPoint - orbitalLaserImpact.Size + Vector2.UnitX * orbitalLaserImpact.Size.X / 2);
                orbitalLaserImpact.Scale = Vector2.One;
                beamEndPoint = entryPoint;
            }
            else
            {
                orbitalLaserImpact!.Scale = Vector2.Zero;
            }

            if (orbitalLaserDamageTimer < orbitalLaserDamageInterval)
            {
                orbitalLaserDamageTimer += deltaTime;
                return;
            }

            var beamLength = Vector2.Distance(shipCenterBottom, beamEndPoint);
            var enemyCandidates = EnemySystem.EnemyBins.GetValuesInBinLine(shipCenterBottom,
                BinGrid<Enemy>.LineDirection.Down, lineWidthAdditionInCells: 2);

            foreach (var enemy in enemyCandidates)
            {
                if (Collision.AABB(enemy.Position.X, enemy.Position.Y, enemy.Size.X, enemy.Size.Y,
                    beamPos.X, beamPos.Y, orbitalLaserBeam.Size.X, beamLength))
                {
                    // 900 damage over 4s
                    var damage = (int)(900f / (orbitalLaserFireTime / orbitalLaserDamageInterval));
                    enemy.HealthSystem.TakeDamage(damage);
                }
            }

            orbitalLaserDamageTimer = 0f;
            return;
        }

        turretHovership.AnimationSystem!.ToggleAnimationState(null);
        orbitalLaserBeam!.Scale = Vector2.Zero;
        orbitalLaserImpact!.Scale = Vector2.Zero;
        actionTimer = 0f;
        orbitalLaserBuildupTimer = 0f;
        orbitalLaserFireTimer = 0f;
        orbitalLaserDamageTimer = 0f;
    }

    private void HandleUFO(float deltaTime)
    {
        var actionInterval = 1f / actionsPerSecond;
        actionTimer += deltaTime;

        if (shouldReturnToBase || actionTimer < actionInterval) return;

        var shipCenterBottom = turretHovership.Position + new Vector2(turretHovership.Size.X / 2, turretHovership.Size.Y);
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesInBinLine(shipCenterBottom,
            BinGrid<Enemy>.LineDirection.Down, lineWidthAdditionInCells: 2);

        ufoTractorBeam!.Scale = Vector2.One;
        ufoTractorBeam.SetPosition(shipCenterBottom - Vector2.UnitX * (ufoTractorBeam.Size.X / 2));
        var beamVisible = ufoCarriedEnemies.Count > 0;

        if (ufoSeekTimer < ufoSeekTime && ufoCarriedEnemies.Count < 5)
        {
            ufoSeekTimer += deltaTime;

            foreach (var enemy in enemyCandidates)
            {
                if (ufoCarriedEnemies.ContainsKey(enemy)) continue;

                if (Collision.AreEntitiesColliding(enemy, ufoTractorBeam))
                {
                    beamVisible = true;

                    var rx = ((float)random.NextDouble() - 0.5f) * 2f * (ufoTractorBeam.Size.X / 2);
                    var ry = ((float)random.NextDouble() - 0.5f) * 2f * (ufoTractorBeam.Size.Y / 2);
                    var randomRelativePosition = new Vector2(rx, ry);
                    ufoCarriedEnemies.Add(enemy, randomRelativePosition);

                    if (ufoCarriedEnemies.Count >= 5) break;
                }
            }
        }

        if (!beamVisible)
        {
            ufoTractorBeam.Scale = Vector2.Zero;
        }
        else
        {
            List<Enemy> toRemove = new();

            foreach (var (enemy, relativePosition) in ufoCarriedEnemies)
            {
                enemy.PhysicsSystem.StopMovement();
                enemy.SetPosition(Vector2.Lerp(enemy.Position, ufoTractorBeam.Position + relativePosition, deltaTime * 5f));

                if (!Collision.AreEntitiesColliding(enemy, ufoTractorBeam))
                {
                    toRemove.Add(enemy);

                    if (ufoCarriedEnemies.Count == toRemove.Count)
                    {
                        ufoSeekTimer = 0f;
                        break;
                    }
                }
            }

            foreach (var enemy in toRemove)
            {
                ufoCarriedEnemies.Remove(enemy);
            }
        }
    }

    private void Shoot(Vector2 direction, Vector2 position)
    {
        var bomb = new MortarShell(Game);
        bomb.Sprite = bombSprite;
        bomb.SetPosition(position);
        bomb.physics.DragFactor = 0f;
        bomb.physics.LocalGravity = 0.125f;
        bomb.Destroyed += _ => EffectUtility.Explode(bomb.Position, radius: 4 * Grid.TileLength,
            magnitude: 10f, damage: baseDamage);
    }

    private Enemy? GetRightmostEnemy()
    {
        Enemy? rightmostEnemy = null;

        var range = realTileRange * Grid.TileLength;
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(Position + Size / 2, range);

        foreach (Enemy enemy in enemyCandidates)
        {
            if (ufoCarriedEnemies.ContainsKey(enemy)) continue;

            var distanceToEnemy = Vector2.Distance(Position + Size / 2, enemy.Position + enemy.Size / 2);

            if (distanceToEnemy > range)
                continue;

            if (rightmostEnemy is null || enemy.Position.X > rightmostEnemy.Position.X)
            {
                rightmostEnemy = enemy;
            }
        }

        return rightmostEnemy;
    }

    public override void Destroy()
    {
        towerCore.CloseDetailsView();
        turretHovership?.Destroy();
        Game.Components.Remove(towerCore);

        base.Destroy();
    }

    public static AnimationSystem.AnimationData GetUnupgradedBaseAnimationData()
    {
        var sprite = AssetManager.GetTexture("hovership_base_idle");

        return new AnimationSystem.AnimationData
        (
            texture: sprite,
            frameCount: 3,
            frameSize: new Vector2(sprite.Width / 3, sprite.Height),
            delaySeconds: 0.1f
        );
    }

    public static List<KeyValuePair<UIEntity, Vector2>> GetUnupgradedPartIcons(List<UIEntity> uiElements)
    {
        var baseSprite = AssetManager.GetTexture("hovership_base_idle");

        var baseData = new AnimationSystem.AnimationData
        (
            texture: baseSprite,
            frameCount: 1,
            frameSize: new Vector2(baseSprite.Width / 3, baseSprite.Height),
            delaySeconds: float.PositiveInfinity
        );

        var baseEntity = new UIEntity(Game1.Instance, uiElements, Vector2.Zero, baseData);

        var list = new List<KeyValuePair<UIEntity, Vector2>>();
        list.Add(KeyValuePair.Create(baseEntity, Vector2.Zero));

        return list;
    }

    private static AnimationSystem.AnimationData GetUnupgradedPlatformAnimation()
    {
        var sprite = AssetManager.GetTexture("hovership_base_platform");

        return new AnimationSystem.AnimationData(
            texture: sprite,
            frameCount: 2,
            frameSize: new Vector2(sprite.Width / 2, sprite.Height),
            delaySeconds: 0.5f);
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(4, 2);
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Hovership;
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        return TowerCore.DefaultCanPlaceTower(GetDefaultGridSize(), targetWorldPosition);
    }

    public static Entity CreateNewInstance(Game game, Vector2 worldPosition)
    {
        return new Hovership(game, worldPosition);
    }

    public void UpgradeTower(TowerUpgradeNode newUpgrade)
    {
        Texture2D newIdleTexture;
        Texture2D newPlatformTexture;
        var newIdleFrameCount = 1;
        var newPlatformFrameCount = 1;

        if (newUpgrade.Name == Upgrade.BombierBay.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("hovership_bombierbay_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_bombierbay_platform");
            newIdleFrameCount = 3;
            newPlatformFrameCount = 2;
            // offset platform because its sprite changes size
            UpdatePosition(-Vector2.UnitY * 2);

            realProjectileAmount += 2;
        }
        else if (newUpgrade.Name == Upgrade.EfficientEngines.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("hovership_efficientengines_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_efficientengines_platform");
            newIdleFrameCount = 3;
            newPlatformFrameCount = 2;
            UpdatePosition(-Vector2.UnitY * 2);

            realTileRange += 20;
            realHovershipSpeed = baseHovershipSpeed * 2;
        }
        else if (newUpgrade.Name == Upgrade.OrbitalLaser.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("hovership_orbitallaser_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_orbitallaser_platform");
            newIdleFrameCount = 8;
            newPlatformFrameCount = 2;
            UpdatePosition(-Vector2.UnitY * 8);
            actionsPerSecond = 1f / 8f;

            var attackBuildupTexture = AssetManager.GetTexture("hovership_orbitallaser_attack");
            var attackBuildupAnimation = new AnimationSystem.AnimationData(
                texture: attackBuildupTexture,
                frameCount: 5,
                frameSize: new Vector2(attackBuildupTexture.Width / 5, attackBuildupTexture.Height),
                delaySeconds: 0.1f);

            var firingTexture = AssetManager.GetTexture("hovership_orbitallaser_firing");
            var firingAnimation = new AnimationSystem.AnimationData(
                texture: firingTexture,
                frameCount: 2,
                frameSize: new Vector2(firingTexture.Width / 2, firingTexture.Height),
                delaySeconds: 0.1f);

            turretHovership.AnimationSystem!.AddAnimationState("buildup", attackBuildupAnimation);
            turretHovership.AnimationSystem!.AddAnimationState("firing", firingAnimation);

            var beamTexture = AssetManager.GetTexture("hovership_orbitallaser_beam");
            var beamData = new AnimationSystem.AnimationData(
                texture: beamTexture,
                frameCount: 2,
                frameSize: new Vector2(beamTexture.Width / 2, beamTexture.Height),
                delaySeconds: 0.1f);

            var impactTexture = AssetManager.GetTexture("hovership_orbitallaser_impact");
            var impactData = new AnimationSystem.AnimationData(
                texture: impactTexture,
                frameCount: 2,
                frameSize: new Vector2(impactTexture.Width / 2, impactTexture.Height),
                delaySeconds: 0.1f);

            orbitalLaserBeam = new Entity(Game, Vector2.Zero, beamData);
            orbitalLaserImpact = new Entity(Game, Vector2.Zero, impactData);
            orbitalLaserImpact.DrawLayerDepth = 0.7f;
        }
        else if (newUpgrade.Name == Upgrade.CarpetofFire.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("hovership_carpetoffire_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_carpetoffire_platform");
            newIdleFrameCount = 4;
            newPlatformFrameCount = 2;
            UpdatePosition(-Vector2.UnitY);
            bombSprite = AssetManager.GetTexture("hovership_carpetoffire_bomb");

            realProjectileAmount += 3;
        }
        else if (newUpgrade.Name == Upgrade.EMPShip.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("hovership_emp_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_emp_platform");
            newIdleFrameCount = 4;
            newPlatformFrameCount = 2;
            UpdatePosition(-Vector2.UnitY * 4);
            bombSprite = AssetManager.GetTexture("hovership_emp_bomb");

            realProjectileAmount -= 2;
            realTargetHoverTileHeight += 10;
        }
        else
        {
            newIdleTexture = AssetManager.GetTexture("hovership_ufo_idle");
            newPlatformTexture = AssetManager.GetTexture("hovership_ufo_platform");
            newIdleFrameCount = 3;
            newPlatformFrameCount = 2;
            UpdatePosition(Vector2.UnitY * 5);

            var beamTexture = AssetManager.GetTexture("hovership_ufo_tractorbeam");
            var beamData = new AnimationSystem.AnimationData(
                texture: beamTexture,
                frameCount: 4,
                frameSize: new Vector2(beamTexture.Width / 4, beamTexture.Height),
                delaySeconds: 0.1f);

            ufoTractorBeam = new Entity(Game, Vector2.Zero, beamData);
            realTargetHoverTileHeight = (int)MathF.Ceiling(ufoTractorBeam.Size.Y / Grid.TileLength);
            turretHovership.RotationRadians = 0f;
        }

        var newIdleAnimation = new AnimationSystem.AnimationData
        (
            texture: newIdleTexture,
            frameCount: newIdleFrameCount,
            frameSize: new Vector2(newIdleTexture.Width / newIdleFrameCount, newIdleTexture.Height),
            delaySeconds: 0.1f
        );

        var newPlatformAnimation = new AnimationSystem.AnimationData
        (
            texture: newPlatformTexture,
            frameCount: newPlatformFrameCount,
            frameSize: new Vector2(newPlatformTexture.Width / newPlatformFrameCount, newPlatformTexture.Height),
            delaySeconds: 0.5f
        );

        turretHovership!.AnimationSystem!.ChangeAnimationState(null, newIdleAnimation);
        AnimationSystem!.ChangeAnimationState(null, newPlatformAnimation);

        var newSize = new Vector2(newIdleAnimation.FrameSize.X, newIdleAnimation.FrameSize.Y);
        turretHovership.Size = newSize;
    }

    public static float GetBaseRange() => baseTileRange;
    public float GetRange() => realTileRange;
}
