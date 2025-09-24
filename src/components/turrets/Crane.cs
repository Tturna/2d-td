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

    public Crane(Game game, Vector2 position) : base(game, position, GetTowerBaseSprite())
    {
        towerCore = new TowerCore(this);

        var explosivePayload = new TowerUpgradeNode(Upgrade.ExplosivePayload.ToString(), price: 85);
        var crusher = new TowerUpgradeNode(Upgrade.Crusher.ToString(), price: 70);
        var bigBall = new TowerUpgradeNode(Upgrade.BigBall.ToString(), price: 25,
            leftChild: explosivePayload, rightChild: crusher);

        var razorball = new TowerUpgradeNode(Upgrade.Razorball.ToString(), price: 50);
        var chargedLifts = new TowerUpgradeNode(Upgrade.ChargedLifts.ToString(), price: 15,
            leftChild: razorball);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), price: 0,
            leftChild: bigBall, rightChild: chargedLifts);

        towerCore.CurrentUpgrade = defaultNode;
    }

    public override void Initialize()
    {
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
    }

    private List<Enemy> GetEnemiesInRange(float extraRange = 0f, bool getOnlyFirst = false, bool useHashSet = false)
    {
        List<Enemy> enemies = new();

        for (int i = EnemySystem.Enemies.Count - 1; i >= 0; i--)
        {
            var enemy = EnemySystem.Enemies[i];

            if (useHashSet)
            {
                if (hitEnemies.Contains(enemy)) continue;
            }

            var diff = (ballThing!.Position + ballThing.Size / 2) - (enemy.Position + enemy.Size / 2);
            var distance = diff.Length();

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
        ballThing.Position += Vector2.UnitY * ballSpeed * deltaTime;

        DamageHitEnemies(damage);

        if (ballThing.Position.Y >= targetBallPosition.Y)
        {
            ballThing.Position = targetBallPosition;
        }

        return false;
    }

    private void HandleReelBack(float deltaTime, float reelSpeedFactor, float cooldownTime)
    {
        if (ballThing!.Position == Position + defaultBallOffset) return;

        ballThing.Position = Vector2.Lerp(targetBallPosition, Position + defaultBallOffset,
            (1f - (cooldownTimer / cooldownTime)) * reelSpeedFactor);

        if (ballThing.Position.Y <= Position.Y + defaultBallOffset.Y)
        {
            ballThing.Position = Position + defaultBallOffset;
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
        }
    }

    private void HandleExplosivePayload(float deltaTime, float actionTime, float cooldownTime,
        float reelDelayTime)
    {
        if (UpdateTimer(deltaTime, ref cooldownTimer)) return;

        if (actionTimer > 0)
        {
            ballSpeed += ballFallAcceleration;
            ballThing!.Position += Vector2.UnitY * ballSpeed * deltaTime;
            var enemyHit = GetEnemiesInRange(getOnlyFirst: true).Count > 0;

            // Explode if enemy or ground hit
            if (enemyHit || ballThing.Position.Y >= targetBallPosition.Y)
            {
                ballThing.Position = targetBallPosition;
                DamageEnemiesUnconditionally(damage: 120, extraRange: 6 * Grid.TileLength);

                actionTimer = 0;
                cooldownTimer = cooldownTime + reelDelayTime;
                ballThing.Position = Position + defaultBallOffset;
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
            ballThing!.Position = Position + defaultBallOffset;

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
        foreach (var enemy in EnemySystem.Enemies)
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
        
        while (ScrapSystem.GetScrapFromPosition(groundCheckPosition) is null &&
            !Collision.IsPointInTerrain(groundCheckPosition, Game.Terrain))
        {
            groundCheckPosition += Vector2.UnitY * Grid.TileLength;
        }

        groundCheckPosition -= Vector2.UnitY * Grid.TileLength;
        targetBallPosition = groundCheckPosition;
        hitEnemies = new();
    }

    private static Texture2D GetBallSprite(SpriteBatch spriteBatch)
    {
        var texture = new Texture2D(spriteBatch.GraphicsDevice, width: Grid.TileLength, height: Grid.TileLength,
        mipmap: false, SurfaceFormat.Color);

        var colorData = new Color[Grid.TileLength * Grid.TileLength];

        for (var i = 0; i < colorData.Length; i++)
        {
            colorData[i] = Color.White;
        }

        texture.SetData(colorData);

        return texture;
    }

    public override void Destroy()
    {
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
        return new Vector2(2, 2);
    }

    public static Texture2D GetTowerBaseSprite()
    {
        return AssetManager.GetTexture("turretTwo");
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Crane;
    }
}
