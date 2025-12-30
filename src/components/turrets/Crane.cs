using System.Collections.Generic;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public class Crane : Entity, ITower
{
    private TowerCore towerCore;
    private Entity? ballThing;
    private PhysicsSystem? crusherPhysics;

    private const float TriggerMargin = 3f;
    private float ballSpeed;
    private float ballFallAcceleration = 10f;
    private Vector2 defaultBallOffset = new Vector2(0, 6);
    private Vector2 ballOffset;
    private float cooldownTime = 1.5f;
    private float reelDelayTime = 1.5f;
    private float actionTime = 1f;
    private int damage = 50;
    private float reelSpeedFactor = 1.5f;
    private int pierce = 3;

    private float actionTimer, cooldownTimer, reelDelayTimer;
    private Vector2 targetBallPosition;
    private HashSet<Enemy> hitEnemies = new();

    private float hitDelay = 0.1f;
    private float hitDelayTimer;

    public enum Upgrade
    {
        NoUpgrade,
        BigBox,
        ExplosivePayload,
        Crusher,
        ChargedLifts,
        Sawblade,
        LaserGate
    }

    public Crane(Game game, Vector2 position) : base(game, position, GetUnupgradedBaseAnimationData())
    {
        var attackSprite = AssetManager.GetTexture("crane_base_attack");

        var attackAnimation = new AnimationSystem.AnimationData
        (
            texture: attackSprite,
            frameCount: 3,
            frameSize: new Vector2(attackSprite.Width / 3, attackSprite.Height),
            delaySeconds: 0.1f
        );

        // base constructor defines animation system
        AnimationSystem!.AddAnimationState("attack", attackAnimation);

        towerCore = new TowerCore(this);

        var explosivePayloadIcon = AssetManager.GetTexture("crane_explosivepayload_icon");
        var crusherIcon = AssetManager.GetTexture("crane_crusher_icon");
        var bigBoxIcon = AssetManager.GetTexture("crane_biggerbox_icon");
        var sawbladeIcon = AssetManager.GetTexture("crane_sawblade_icon");
        var chargedLiftsIcon = AssetManager.GetTexture("crane_chargedlifts_icon");
        var laserGateIcon = AssetManager.GetTexture("crane_lasergate_icon");

        var explosivePayload = new TowerUpgradeNode(Upgrade.ExplosivePayload.ToString(), explosivePayloadIcon, price: 185);
        var crusher = new TowerUpgradeNode(Upgrade.Crusher.ToString(), crusherIcon, price: 190);
        var bigBox = new TowerUpgradeNode(Upgrade.BigBox.ToString(), bigBoxIcon, price: 25,
            leftChild: explosivePayload, rightChild: crusher);

        var sawblade = new TowerUpgradeNode(Upgrade.Sawblade.ToString(), sawbladeIcon, price: 170);
        var lasergate = new TowerUpgradeNode(Upgrade.LaserGate.ToString(), laserGateIcon, price: 170);
        var chargedLifts = new TowerUpgradeNode(Upgrade.ChargedLifts.ToString(), chargedLiftsIcon, price: 15,
            leftChild: sawblade, rightChild: lasergate);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0,
            leftChild: bigBox, rightChild: chargedLifts);

        bigBox.Description = "+30 damage\nIncreased ball size ";
        chargedLifts.Description = "-1s lift time";
        explosivePayload.Description = "Ball explodes instantly\non contact with an enemy,\ndealing 120 damage in a\n6 tile radius.";
        crusher.Description = "+180 damage\n+10 pierce.\nBall is no longer attached\nby a tether, instead\nrolls downhill until it stops.";
        sawblade.Description = "Ball passes through enemies,\ndealing 120 DPS to all\ntouching the blade.";

        towerCore.CurrentUpgrade = defaultNode;
    }

    public override void Initialize()
    {
        var baseBall = AssetManager.GetTexture("crane_base_ball");
        var baseBallAnimation = new AnimationSystem.AnimationData(
            texture: baseBall,
            frameCount: 1,
            frameSize: new Vector2(baseBall.Width, baseBall.Height),
            delaySeconds: float.PositiveInfinity);

        ballOffset = defaultBallOffset;
        ballThing = new Entity(Game, position: Position + ballOffset, baseBallAnimation);
    }

    public override void Update(GameTime gameTime)
    {
        if (towerCore.Health.CurrentHealth <= 0) return;

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleDefaultCrane(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.BigBox.ToString())
        {
            HandleDefaultCrane(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.ChargedLifts.ToString())
        {
            HandleDefaultCrane(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.ExplosivePayload.ToString())
        {
            HandleExplosivePayload(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Sawblade.ToString())
        {
            var totalGameSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
            HandleSawblade(deltaTime, totalGameSeconds);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.LaserGate.ToString())
        {
            HandleLaserGate(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Crusher.ToString())
        {
            var totalGameSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
            HandleCrusher(deltaTime, totalGameSeconds);
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        towerCore.Health.DrawHealthBar(Position + new Vector2(Size.X / 2, -4));

        base.Draw(gameTime);
    }

    private List<Enemy> GetEnemiesInRange(float extraRange = 0f, bool getOnlyFirst = false, bool useHashSet = false)
    {
        List<Enemy> enemies = new();
        var ballThingCenter = ballThing!.Position + ballThing.Size / 2;
        var ballThingSize = MathHelper.Max(ballThing.Size.X, ballThing.Size.Y);
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesFromBinsInRange(ballThingCenter, ballThingSize + extraRange);

        foreach (var enemy in enemyCandidates)
        {
            if (useHashSet)
            {
                if (hitEnemies.Contains(enemy)) continue;
            }

            if (!Collision.AreEntitiesColliding(enemy, ballThing)) continue;

            if (useHashSet)
            {
                hitEnemies.Add(enemy);
            }

            enemies.Add(enemy);

            if (getOnlyFirst) return enemies;

            if (enemies.Count >= pierce) return enemies;
        }

        return enemies;
    }

    private void DamageHitEnemies(float deltaTime)
    {
        var enemies = GetEnemiesInRange(useHashSet: true);

        foreach (var enemy in enemies)
        {
            enemy.HealthSystem.TakeDamage(this, damage);
        }
    }

    private void DamageEnemiesUnconditionally(float deltaTime, int damage, float extraRange = 0f)
    {
        var enemies = GetEnemiesInRange(extraRange: extraRange);

        foreach (var enemy in enemies)
        {
            enemy.HealthSystem.TakeDamage(this, damage);
        }
    }

    /// <summary>
    /// Drops the ball and return a boolean indicating whether it has reached the target.
    /// </summary>
    private bool HandleBallDescent(float deltaTime, bool damageEnemies = true)
    {
        if (ballThing!.Position == targetBallPosition)
        {
            SoundSystem.PlaySound("mortarfire");
            return true;
        }

        ballSpeed += ballFallAcceleration;
        ballThing.UpdatePosition(Vector2.UnitY * ballSpeed * deltaTime);

        if (damageEnemies)
        {
            DamageHitEnemies(deltaTime);
        }

        var corpseCandidates = ScrapSystem.Corpses!.GetBinAndNeighborValues(ballThing.Position + ballThing.Size / 2);

        foreach (var corpse in corpseCandidates)
        {
            if (Collision.AreEntitiesColliding(corpse, ballThing))
            {
                SoundSystem.PlaySound("mortarfire");
                return true;
            }
        }

        if (ballThing.Position.Y >= targetBallPosition.Y)
        {
            ballThing.SetPosition(targetBallPosition);
        }

        return false;
    }

    private void HandleReelBack(float deltaTime)
    {
        if (ballThing!.Position == Position + ballOffset) return;

        ballThing.SetPosition(Vector2.Lerp(targetBallPosition, Position + ballOffset,
            (1f - (cooldownTimer / cooldownTime)) * reelSpeedFactor));

        if (ballThing.Position.Y <= Position.Y + ballOffset.Y)
        {
            ballThing.SetPosition(Position + ballOffset);
        }
    }

    private void HandleDefaultCrane(float deltaTime)
    {
        var reelDelayTimerRunning = UpdateTimer(deltaTime, ref reelDelayTimer);

        if (reelDelayTimerRunning) return;

        var cooldownTimerRunning = UpdateTimer(deltaTime, ref cooldownTimer);

        if (cooldownTimerRunning)
        {
            HandleReelBack(deltaTime);
            return;
        }

        // nothing counts down action timer here. It's just set to 0 when the ball hits the ground
        if (actionTimer > 0)
        {
            var targetReached = HandleBallDescent(deltaTime);

            if (targetReached)
            {
                actionTimer = 0;
                cooldownTimer = cooldownTime;
                reelDelayTimer = reelDelayTime;
                // set target pos again in case ball hit corpse and stops before hitting the
                // target position on terrain
                targetBallPosition = ballThing!.Position;
            }

            return;
        }

        if (IsEnemyBelow())
        {
            Trigger();
            AnimationSystem!.OneShotAnimationState("attack");
        }
    }

    private void HandleExplosivePayload(float deltaTime)
    {
        if (UpdateTimer(deltaTime, ref cooldownTimer)) return;

        if (actionTimer > 0)
        {
            ballSpeed += ballFallAcceleration;
            ballThing!.UpdatePosition(Vector2.UnitY * ballSpeed * deltaTime);
            var enemyHit = GetEnemiesInRange(getOnlyFirst: true).Count > 0;

            // Explode if enemy or ground hit
            if (enemyHit || ballThing.Position.Y >= targetBallPosition.Y)
            {
                ballThing.SetPosition(targetBallPosition);
                EffectUtility.Explode(this, ballThing.Position + ballThing.Size / 2,
                    radius: 6 * Grid.TileLength, magnitude: 10, damage);

                actionTimer = 0;
                cooldownTimer = cooldownTime + reelDelayTime;
                ballThing.SetPosition(Position + ballOffset);
            }

            return;
        }

        if (IsEnemyBelow())
        {
            Trigger();
            AnimationSystem!.OneShotAnimationState("attack");
        }
    }

    private void HandleSawblade(float deltaTime, float totalGameSeconds)
    {
        if (reelDelayTimer > 0 || cooldownTimer > 0 || actionTimer > 0)
        {
            hitDelayTimer -= deltaTime;

            if (hitDelayTimer <= 0)
            {
                DamageEnemiesUnconditionally(deltaTime, damage / 10);
                hitDelayTimer = hitDelay;
            }
        }

        var reelDelayTimerRunning = UpdateTimer(deltaTime, ref reelDelayTimer);

        if (reelDelayTimerRunning) return;

        var cooldownTimerRunning = UpdateTimer(deltaTime, ref cooldownTimer);

        if (cooldownTimerRunning)
        {
            HandleReelBack(deltaTime);
            return;
        }

        if (actionTimer > 0)
        {
            var targetReached = HandleBallDescent(deltaTime, damageEnemies: false);

            if (targetReached)
            {
                actionTimer = 0;
                reelDelayTimer = reelDelayTime;
                cooldownTimer = cooldownTime;
                targetBallPosition = ballThing!.Position;
            }

            return;
        }

        if (IsEnemyBelow())
        {
            Trigger();
            AnimationSystem!.OneShotAnimationState("attack");
        }
    }

    private void HandleLaserGate(float deltaTime)
    {
        if (IsEnemyBelow())
        {
            if (ballThing!.Scale.X > 0)
            {
                hitDelayTimer -= deltaTime;

                if (hitDelayTimer <= 0)
                {
                    hitDelayTimer = hitDelay;
                    var enemyCandidates = EnemySystem.EnemyBins.GetValuesInBinLine(ballThing.Position, BinGrid<Enemy>.LineDirection.Down);

                    foreach (var enemy in enemyCandidates)
                    {
                        if (!Collision.IsLineInEntity(ballThing!.Position, targetBallPosition, enemy,
                            out var _, out var _))
                        {
                            continue;
                        }

                        enemy.HealthSystem.TakeDamage(this, damage / 10);
                    }
                }
            }
            else
            {
                if (Collision.IsLineInTerrain(ballThing!.Position, ballThing.Position + Vector2.UnitY * 100,
                    out var entryPoint, out var _))
                {
                    targetBallPosition = entryPoint;
                    var diff = ballThing!.Position - entryPoint;
                    var distance = diff.Length();
                    ballThing.Scale = new Vector2(1, distance);
                    AnimationSystem!.ToggleAnimationState("attack");
                }
            }
        }
        else if (ballThing!.Scale.X > 0)
        {
            ballThing.Scale = Vector2.Zero;
            AnimationSystem!.ToggleAnimationState(null);
        }
    }

    private void HandleCrusher(float deltaTime, float totalGameSeconds)
    {
        if (crusherPhysics is null)
        {
            crusherPhysics = new PhysicsSystem(Game);
            crusherPhysics.IgnoreEnemyCollision = true;
        }

        var reelDelayTimerRunning = UpdateTimer(deltaTime, ref reelDelayTimer);

        if (reelDelayTimerRunning)
        {
            crusherPhysics.AddForce(-Vector2.UnitX * 10f * deltaTime);
            crusherPhysics.UpdatePhysics(ballThing, deltaTime);

            DamageHitEnemies(deltaTime);

            return;
        }

        var cooldownTimerRunning = UpdateTimer(deltaTime, ref cooldownTimer);

        if (cooldownTimerRunning)
        {
            ballThing!.SetPosition(Position + ballOffset);

            return;
        }

        if (actionTimer > 0)
        {
            var targetReached = HandleBallDescent(deltaTime);

            if (targetReached)
            {
                cooldownTimer = cooldownTime;
                reelDelayTimer = reelDelayTime;
                actionTimer = 0;
                targetBallPosition = ballThing!.Position;
            }

            return;
        }

        if (IsEnemyBelow())
        {
            Trigger();
            AnimationSystem!.OneShotAnimationState("attack");
        }
    }

    private bool UpdateTimer(float deltaTime, ref float timer)
    {
        if (timer > 0)
        {
            timer -= deltaTime;

            if (timer <= 0)
            {
                timer = 0;
            }

            return true;
        }

        return false;
    }

    private bool IsEnemyBelow()
    {
        var towerTestPosition = ballThing!.Position + ballThing.Size / 2;
        var enemyCandidates = EnemySystem.EnemyBins.GetValuesInBinLine(towerTestPosition,
            BinGrid<Enemy>.LineDirection.Down, lineWidthAdditionInCells: 2);

        foreach (var enemy in enemyCandidates)
        {
            var enemyTestPosition = enemy.Position + enemy.Size / 2;

            if (enemyTestPosition.X < towerTestPosition.X + TriggerMargin &&
                enemyTestPosition.X > towerTestPosition.X - TriggerMargin)
            {
                return true;
            }
        }

        return false;
    }

    private void Trigger()
    {
        actionTimer = actionTime;
        var groundCheckPosition = ballThing!.Position;
        
        while (!Collision.IsPointInTerrain(groundCheckPosition, Game.Terrain))
        {
            groundCheckPosition += Vector2.UnitY * Grid.TileLength;
        }

        var adjustedPosition = Grid.SnapPositionToGrid(groundCheckPosition - Vector2.UnitY * Grid.TileLength);
        adjustedPosition += Vector2.One * (Grid.TileLength / 2);
        adjustedPosition -= ballThing.Size / 2;
        targetBallPosition = adjustedPosition;
        hitEnemies = new();
    }

    public override void Destroy()
    {
        towerCore.CloseDetailsView();
        Game.Components.Remove(towerCore);
        ballThing?.Destroy();
        base.Destroy();
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        var targetGridPosition = Grid.SnapPositionToGrid(targetWorldPosition) + Vector2.UnitX * Grid.TileLength;
        var gridSizeX = 2;
        var gridSizeY = 2;

        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                var position = targetGridPosition + new Vector2(x, y) * Grid.TileLength;

                if (Collision.IsPointInTerrain(position, Game1.Instance.Terrain))
                {
                    return false;
                }

                foreach (var tower in BuildingSystem.Towers)
                {
                    if (Collision.IsPointInEntity(position, tower))
                    {
                        return false;
                    }
                }
            }
        }

        var belowTilePosition = targetGridPosition + Vector2.UnitY * gridSizeY * Grid.TileLength;
        var aboveTilePosition = targetGridPosition - Vector2.UnitY * Grid.TileLength;

        if (!Collision.IsPointInTerrain(belowTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        if (Collision.IsPointInTerrain(aboveTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        return true;
    }

    public static Entity CreateNewInstance(Game game, Vector2 worldPosition)
    {
        return new Crane(game, worldPosition);
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(3, 2);
    }

    public static AnimationSystem.AnimationData GetUnupgradedBaseAnimationData()
    {
        var sprite = AssetManager.GetTexture("crane_base_idle");

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
        return BuildingSystem.TowerType.Crane;
    }

    public void UpgradeTower(TowerUpgradeNode newUpgrade)
    {
        Texture2D newIdleTexture;
        Texture2D newFireTexture;
        var newIdleFrameCount = 1;
        var newFireFrameCount = 1;

        if (newUpgrade.Name == Upgrade.BigBox.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("crane_biggerbox_idle");
            newFireTexture = AssetManager.GetTexture("crane_biggerbox_attack");
            newIdleFrameCount = 1;
            newFireFrameCount = 3;

            var newBallTexture = AssetManager.GetTexture("crane_biggerbox_box");
            var newBallAnimation = new AnimationSystem.AnimationData(
                texture: newBallTexture,
                frameCount: 1,
                frameSize: new Vector2(newBallTexture.Width, newBallTexture.Height),
                delaySeconds: float.PositiveInfinity);

            ballThing!.AnimationSystem!.ChangeAnimationState(null, newBallAnimation);
            ballThing.Size = newBallAnimation.FrameSize;

            damage += 30;
            var ballOffsetAdjustment = new Vector2(0, 2);
            var towerOffset = new Vector2(-2, -2);

            if (ballThing.Position == Position + ballOffset)
            {
                ballThing.UpdatePosition(ballOffsetAdjustment + towerOffset);
            }

            UpdatePosition(towerOffset);
            ballOffset += ballOffsetAdjustment;
        }
        else if (newUpgrade.Name == Upgrade.ExplosivePayload.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("crane_explosivepayload_idle");
            newFireTexture = AssetManager.GetTexture("crane_explosivepayload_attack");
            newIdleFrameCount = 2;
            newFireFrameCount = 3;

            var newBallTexture = AssetManager.GetTexture("crane_explosivepayload_box");
            var newBallAnimation = new AnimationSystem.AnimationData(
                texture: newBallTexture,
                frameCount: 1,
                frameSize: new Vector2(newBallTexture.Width, newBallTexture.Height),
                delaySeconds: float.PositiveInfinity);

            ballThing!.AnimationSystem!.ChangeAnimationState(null, newBallAnimation);
            ballThing.Size = newBallAnimation.FrameSize;

            damage = 120;
            UpdatePosition(-Vector2.UnitY * 3);

            ballOffset += Vector2.UnitY * 2;
        }
        else if (newUpgrade.Name == Upgrade.Crusher.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("crane_crusher_idle");
            newFireTexture = AssetManager.GetTexture("crane_crusher_attack");
            newIdleFrameCount = 1;
            newFireFrameCount = 3;

            var newBallTexture = AssetManager.GetTexture("crane_crusher_ball");
            var newBallAnimation = new AnimationSystem.AnimationData(
                texture: newBallTexture,
                frameCount: 1,
                frameSize: new Vector2(newBallTexture.Width, newBallTexture.Height),
                delaySeconds: float.PositiveInfinity);

            ballThing!.AnimationSystem!.ChangeAnimationState(null, newBallAnimation);
            ballThing.Size = newBallAnimation.FrameSize;

            damage += 180;
            pierce += 10;

            var ballOffsetAdjustment = new Vector2(-8 - newBallTexture.Width / 2, -newBallTexture.Height / 2);
            var towerOffset = new Vector2(8, -4);

            if (ballThing.Position == Position + ballOffset)
            {
                ballThing.UpdatePosition(ballOffsetAdjustment + towerOffset);
            }

            UpdatePosition(towerOffset);
            ballOffset += ballOffsetAdjustment;
        }
        else if (newUpgrade.Name == Upgrade.ChargedLifts.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("crane_chargedlifts_idle");
            newFireTexture = AssetManager.GetTexture("crane_chargedlifts_attack");
            newIdleFrameCount = 1;
            newFireFrameCount = 3;

            cooldownTime -= 1;
        }
        else if (newUpgrade.Name == Upgrade.LaserGate.ToString())
        {
            newIdleTexture = AssetManager.GetTexture("crane_lasergate_idle");
            newFireTexture = AssetManager.GetTexture("crane_lasergate_attack");
            newIdleFrameCount = 6;
            newFireFrameCount = 2;

            var newBallTexture = AssetManager.GetTexture("crane_lasergate_laser");
            var newBallAnimation = new AnimationSystem.AnimationData(
                texture: newBallTexture,
                frameCount: 2,
                frameSize: new Vector2(newBallTexture.Width / 2, newBallTexture.Height),
                delaySeconds: 0.1f);

            ballThing!.AnimationSystem!.ChangeAnimationState(null, newBallAnimation);
            ballThing.Size = newBallAnimation.FrameSize;

            damage = 100;
            var ballOffsetAdjustment = new Vector2(2, 3);
            var towerOffset = new Vector2(0, -2);

            if (ballThing.Position == Position + ballOffset)
            {
                ballThing.UpdatePosition(ballOffsetAdjustment + towerOffset);
            }

            UpdatePosition(towerOffset);
            ballOffset += ballOffsetAdjustment;
        }
        else
        {
            newIdleTexture = AssetManager.GetTexture("crane_sawblade_idle");
            newFireTexture = AssetManager.GetTexture("crane_sawblade_attack");
            newIdleFrameCount = 1;
            newFireFrameCount = 3;

            var newBallTexture = AssetManager.GetTexture("crane_sawblade_blade");
            var newBallAnimation = new AnimationSystem.AnimationData(
                texture: newBallTexture,
                frameCount: 2,
                frameSize: new Vector2(newBallTexture.Width / 2, newBallTexture.Height),
                delaySeconds: 0.1f);

            ballThing!.AnimationSystem!.ChangeAnimationState(null, newBallAnimation);
            ballThing.Size = newBallAnimation.FrameSize;

            damage = 120;
            var ballOffsetAdjustment = new Vector2(-3, 5);
            var towerOffset = new Vector2(-3, -5);

            if (ballThing.Position == Position + ballOffset)
            {
                ballThing.UpdatePosition(ballOffsetAdjustment + towerOffset);
            }

            UpdatePosition(towerOffset);
            ballOffset += ballOffsetAdjustment;
            reelDelayTime = 3f;
            cooldownTime = 2f;
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
            delaySeconds: 0.1f
        );

        AnimationSystem!.ChangeAnimationState(null, newIdleAnimation);
        AnimationSystem.ChangeAnimationState("attack", newFireAnimation);
    }

    public static float GetBaseRange() => 0f;

    public float GetRange() => 0f;

    public TowerCore GetTowerCore() => towerCore;

    public static void DrawBaseRangeIndicator(Vector2 worldPosition) { }
    public void DrawRangeIndicator() { }
}
