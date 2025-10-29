using System;
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
    private Vector2 defaultBallOffset = new Vector2(-8, 0);

    private float actionTimer, cooldownTimer, reelDelayTimer;
    private Vector2 targetBallPosition;
    private HashSet<Enemy> hitEnemies = new();

    public enum Upgrade
    {
        NoUpgrade,
        BigBall,
        ExplosivePayload,
        Crusher,
        ChargedLifts,
        Razorball
    }

    public Crane(Game game, Vector2 position) : base(game, position, GetTowerBaseAnimationData())
    {
        var attackSprite = AssetManager.GetTexture("crane_base_attack");

        var attackAnimation = new AnimationSystem.AnimationData
        (
            texture: attackSprite,
            frameCount: 5,
            frameSize: new Vector2(attackSprite.Width / 5, attackSprite.Height),
            delaySeconds: 0.1f
        );

        // base constructor defines animation system
        AnimationSystem!.AddAnimationState("attack", attackAnimation);

        towerCore = new TowerCore(this);

        var tempIcon = AssetManager.GetTexture("gunTurret_botshot_icon");

        var explosivePayload = new TowerUpgradeNode(Upgrade.ExplosivePayload.ToString(), tempIcon, price: 85);
        var crusher = new TowerUpgradeNode(Upgrade.Crusher.ToString(), tempIcon, price: 70);
        var bigBall = new TowerUpgradeNode(Upgrade.BigBall.ToString(), tempIcon, price: 25,
            leftChild: explosivePayload, rightChild: crusher);

        var razorball = new TowerUpgradeNode(Upgrade.Razorball.ToString(), tempIcon, price: 50);
        var chargedLifts = new TowerUpgradeNode(Upgrade.ChargedLifts.ToString(), tempIcon, price: 15,
            leftChild: razorball);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0,
            leftChild: bigBall, rightChild: chargedLifts);

        bigBall.Description = "+20 damage\nIncreased ball size ";
        chargedLifts.Description = "-1s lift time";
        explosivePayload.Description = "Ball explodes instantly\non contact with an enemy,\ndealing 120 damage in a\n6 tile radius.";
        crusher.Description = "0.2 shots/sec\n+50 damage\n+10 pierce.\nBall is no longer attached\nby a tether, instead\nrolls downhill until it stops.";
        razorball.Description = "Ball passes through enemies,\ndealing 50 DPS to all\ntouching the blade.\nReeled up after 1.5s.";

        towerCore.CurrentUpgrade = defaultNode;
    }

    public override void Initialize()
    {
        UpdatePosition(-Vector2.UnitY * 3);
        ballThing = new Entity(Game, position: Position + defaultBallOffset, GetBallSprite(Game.SpriteBatch));
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleDefaultCrane(deltaTime, damage: 30, reelSpeedFactor: 1f, actionTime: 1f,
                cooldownTime: 1f, reelDelayTime: 0.5f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.BigBall.ToString())
        {
            // TODO: Increased ball size?
            HandleDefaultCrane(deltaTime, damage: 50, reelSpeedFactor: 1f, actionTime: 1f,
                cooldownTime: 1f, reelDelayTime: 0.5f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.ChargedLifts.ToString())
        {
            HandleDefaultCrane(deltaTime, damage: 30, reelSpeedFactor: 1.5f, actionTime: 1f,
                cooldownTime: 1f, reelDelayTime: 0.5f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.ExplosivePayload.ToString())
        {
            HandleExplosivePayload(deltaTime, actionTime: 1f, cooldownTime: 1f, reelDelayTime: 0.5f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Razorball.ToString())
        {
            HandleRazorball(deltaTime, (float)gameTime.TotalGameTime.TotalSeconds, damage: 30,
                reelSpeedFactor: 1.5f, actionTime: 1f, cooldownTime: 1f, reelDelayTime: 1.5f);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Crusher.ToString())
        {
            HandleCrusher(deltaTime, (float)gameTime.TotalGameTime.TotalSeconds, damage: 30,
                reelSpeedFactor: 1f, actionTime: 1f, cooldownTime: 1f, reelDelayTime: 2f);
        }

        base.Update(gameTime);
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

            var diff = (ballThingCenter) - (enemy.Position + enemy.Size / 2);
            var distance = diff.Length();

            // TODO: ensure this condition is correct
            if (distance > ballThing.Size.X / 2 + enemy.Size.X / 2 + extraRange) continue;

            if (useHashSet)
            {
                hitEnemies.Add(enemy);
            }

            enemies.Add(enemy);

            if (getOnlyFirst) return enemies;
        }

        return enemies;
    }

    private void DamageHitEnemies(int damage)
    {
        var enemies = GetEnemiesInRange(useHashSet: true);

        foreach (var enemy in enemies)
        {
            enemy.HealthSystem.TakeDamage(damage);
        }
    }

    private void DamageEnemiesUnconditionally(int damage, float extraRange = 0f)
    {
        var enemies = GetEnemiesInRange(extraRange: extraRange);

        foreach (var enemy in enemies)
        {
            enemy.HealthSystem.TakeDamage(damage);
        }
    }

    /// <summary>
    /// Drops the ball and return a boolean indicating whether it has reached the target.
    /// </summary>
    private bool HandleBallDescent(float deltaTime, int damage)
    {
        if (ballThing!.Position == targetBallPosition) return true;

        ballSpeed += ballFallAcceleration;
        ballThing.UpdatePosition(Vector2.UnitY * ballSpeed * deltaTime);

        DamageHitEnemies(damage);

        if (ballThing.Position.Y >= targetBallPosition.Y)
        {
            ballThing.SetPosition(targetBallPosition);
        }

        return false;
    }

    private void HandleReelBack(float deltaTime, float reelSpeedFactor, float cooldownTime)
    {
        if (ballThing!.Position == Position + defaultBallOffset) return;

        ballThing.SetPosition(Vector2.Lerp(targetBallPosition, Position + defaultBallOffset,
            (1f - (cooldownTimer / cooldownTime)) * reelSpeedFactor));

        if (ballThing.Position.Y <= Position.Y + defaultBallOffset.Y)
        {
            ballThing.SetPosition(Position + defaultBallOffset);
        }
    }

    private void HandleDefaultCrane(float deltaTime, int damage, float reelSpeedFactor,
        float actionTime, float cooldownTime, float reelDelayTime)
    {
        var reelDelayTimerRunning = UpdateTimer(deltaTime, ref reelDelayTimer);

        if (reelDelayTimerRunning) return;

        var cooldownTimerRunning = UpdateTimer(deltaTime, ref cooldownTimer);

        if (cooldownTimerRunning)
        {
            HandleReelBack(deltaTime, reelSpeedFactor: reelSpeedFactor, cooldownTime);
            return;
        }

        if (actionTimer > 0)
        {
            var targetReached = HandleBallDescent(deltaTime, damage: damage);

            if (targetReached)
            {
                actionTimer = 0;
                cooldownTimer = cooldownTime;
                reelDelayTimer = reelDelayTime;
                return;
            }
        }

        if (IsEnemyBelow())
        {
            Trigger(actionTime);
            AnimationSystem!.OneShotAnimationState("attack");
        }
    }

    private void HandleExplosivePayload(float deltaTime, float actionTime, float cooldownTime,
        float reelDelayTime)
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
                DamageEnemiesUnconditionally(damage: 120, extraRange: 6 * Grid.TileLength);

                actionTimer = 0;
                cooldownTimer = cooldownTime + reelDelayTime;
                ballThing.SetPosition(Position + defaultBallOffset);
            }
        }

        if (IsEnemyBelow())
        {
            Trigger(actionTime);
        }
    }

    private void HandleRazorball(float deltaTime, float totalGameSeconds, int damage,
        float reelSpeedFactor, float actionTime, float cooldownTime, float reelDelayTime)
    {
        if (reelDelayTimer > 0 || cooldownTimer > 0 || actionTimer > 0)
        {
            var comparedTime = Math.Floor(totalGameSeconds * 100f);

            if (comparedTime % 25 == 0)
            {
                DamageEnemiesUnconditionally(damage: 15);
            }
        }

        var reelDelayTimerRunning = UpdateTimer(deltaTime, ref reelDelayTimer);

        if (reelDelayTimerRunning) return;

        var cooldownTimerRunning = UpdateTimer(deltaTime, ref cooldownTimer);

        if (cooldownTimerRunning)
        {
            HandleReelBack(deltaTime, reelSpeedFactor, cooldownTime);
            return;
        }

        if (actionTimer > 0)
        {
            var targetReached = HandleBallDescent(deltaTime, damage: damage);

            if (targetReached)
            {
                actionTimer = 0;
                reelDelayTimer = reelDelayTime;
                cooldownTimer = cooldownTime;
            }

            return;
        }

        if (IsEnemyBelow())
        {
            Trigger(actionTime);
        }
    }

    private void HandleCrusher(float deltaTime, float totalGameSeconds, int damage, float reelSpeedFactor,
        float actionTime, float cooldownTime, float reelDelayTime)
    {
        if (crusherPhysics is null)
        {
            crusherPhysics = new PhysicsSystem(Game);
        }

        var reelDelayTimerRunning = UpdateTimer(deltaTime, ref reelDelayTimer);

        if (reelDelayTimerRunning)
        {
            crusherPhysics.AddForce(-Vector2.UnitX * 10f * deltaTime);
            crusherPhysics.UpdatePhysics(ballThing, deltaTime);

            var comparedTime = Math.Floor(totalGameSeconds * 100f);

            if (comparedTime % 10 == 0)
            {
                DamageEnemiesUnconditionally(damage: 8);
            }

            return;
        }

        var cooldownTimerRunning = UpdateTimer(deltaTime, ref cooldownTimer);

        if (cooldownTimerRunning)
        {
            ballThing!.SetPosition(Position + defaultBallOffset);

            return;
        }

        if (actionTimer > 0)
        {
            var targetReached = HandleBallDescent(deltaTime, damage: damage);

            if (targetReached)
            {
                cooldownTimer = cooldownTime;
                reelDelayTimer = reelDelayTime;
                actionTimer = 0;

                return;
            }
        }

        if (IsEnemyBelow())
        {
            Trigger(actionTime);
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

    private void Trigger(float actionTime)
    {
        actionTimer = actionTime;
        var groundCheckPosition = ballThing!.Position;
        
        while (!Collision.IsPointInTerrain(groundCheckPosition, Game.Terrain))
        {
            groundCheckPosition += Vector2.UnitY * Grid.TileLength;
        }

        groundCheckPosition -= Vector2.UnitY * Grid.TileLength;
        targetBallPosition = groundCheckPosition;
        hitEnemies = new();
    }

    private static Texture2D GetBallSprite(SpriteBatch spriteBatch)
    {
        var texture = TextureUtility.GetBlankTexture(spriteBatch, Grid.TileLength, Grid.TileLength, Color.White);
        return texture;
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
        return TowerCore.DefaultCanPlaceTower(GetDefaultGridSize(), targetWorldPosition);
    }

    public static Entity CreateNewInstance(Game game, Vector2 worldPosition)
    {
        return new Crane(game, worldPosition);
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(3, 2);
    }

    public static AnimationSystem.AnimationData GetTowerBaseAnimationData()
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

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Crane;
    }

    public void UpgradeTower(TowerUpgradeNode newUpgrade)
    {
    }
}
