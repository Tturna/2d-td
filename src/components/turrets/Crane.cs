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
    private float cooldownTime = 1.5f;
    private float reelDelayTime = 1.5f;
    private float actionTime = 1f;
    private int damage = 50;
    private float reelSpeedFactor = 1.5f;
    private int pierce = 3;

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

    public Crane(Game game, Vector2 position) : base(game, position, GetUnupgradedBaseAnimationData())
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

        var explosivePayload = new TowerUpgradeNode(Upgrade.ExplosivePayload.ToString(), tempIcon, price: 185);
        var crusher = new TowerUpgradeNode(Upgrade.Crusher.ToString(), tempIcon, price: 190);
        var bigBall = new TowerUpgradeNode(Upgrade.BigBall.ToString(), tempIcon, price: 25,
            leftChild: explosivePayload, rightChild: crusher);

        var razorball = new TowerUpgradeNode(Upgrade.Razorball.ToString(), tempIcon, price: 170);
        var chargedLifts = new TowerUpgradeNode(Upgrade.ChargedLifts.ToString(), tempIcon, price: 15,
            leftChild: razorball);

        var defaultNode = new TowerUpgradeNode(Upgrade.NoUpgrade.ToString(), upgradeIcon: null, price: 0,
            leftChild: bigBall, rightChild: chargedLifts);

        bigBall.Description = "+30 damage\nIncreased ball size ";
        chargedLifts.Description = "-1s lift time";
        explosivePayload.Description = "Ball explodes instantly\non contact with an enemy,\ndealing 120 damage in a\n6 tile radius.";
        crusher.Description = "+180 damage\n+10 pierce.\nBall is no longer attached\nby a tether, instead\nrolls downhill until it stops.";
        razorball.Description = "Ball passes through enemies,\ndealing 120 DPS to all\ntouching the blade.";

        towerCore.CurrentUpgrade = defaultNode;
    }

    public override void Initialize()
    {
        UpdatePosition(-Vector2.UnitY * 3);
        ballThing = new Entity(Game, position: Position + defaultBallOffset, GetBallSprite(Game.SpriteBatch));
    }

    public override void Update(GameTime gameTime)
    {
        if (towerCore.Health.CurrentHealth <= 0) return;

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (towerCore.CurrentUpgrade.Name == Upgrade.NoUpgrade.ToString())
        {
            HandleDefaultCrane(deltaTime);
        }
        else if (towerCore.CurrentUpgrade.Name == Upgrade.BigBall.ToString())
        {
            // TODO: Increased ball size?
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
        else if (towerCore.CurrentUpgrade.Name == Upgrade.Razorball.ToString())
        {
            var totalGameSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
            HandleRazorball(deltaTime, totalGameSeconds);
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
    private bool HandleBallDescent(float deltaTime)
    {
        if (ballThing!.Position == targetBallPosition) return true;

        ballSpeed += ballFallAcceleration;
        ballThing.UpdatePosition(Vector2.UnitY * ballSpeed * deltaTime);

        DamageHitEnemies(deltaTime);

        var corpseCandidates = ScrapSystem.Corpses!.GetBinAndNeighborValues(ballThing.Position + ballThing.Size / 2);

        foreach (var corpse in corpseCandidates)
        {
            if (Collision.AreEntitiesColliding(corpse, ballThing))
            {
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
        if (ballThing!.Position == Position + defaultBallOffset) return;

        ballThing.SetPosition(Vector2.Lerp(targetBallPosition, Position + defaultBallOffset,
            (1f - (cooldownTimer / cooldownTime)) * reelSpeedFactor));

        if (ballThing.Position.Y <= Position.Y + defaultBallOffset.Y)
        {
            ballThing.SetPosition(Position + defaultBallOffset);
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
                ballThing.SetPosition(Position + defaultBallOffset);
            }

            return;
        }

        if (IsEnemyBelow())
        {
            Trigger();
        }
    }

    private void HandleRazorball(float deltaTime, float totalGameSeconds)
    {
        if (reelDelayTimer > 0 || cooldownTimer > 0 || actionTimer > 0)
        {
            var comparedTime = Math.Floor(totalGameSeconds * 100f);

            if (comparedTime % 25 == 0)
            {
                DamageEnemiesUnconditionally(deltaTime, damage / 4);
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
            var targetReached = HandleBallDescent(deltaTime);

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

            var comparedTime = Math.Floor(totalGameSeconds * 100f);

            if (comparedTime % 10 == 0)
            {
                DamageEnemiesUnconditionally(deltaTime, damage);
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
        // Texture2D newIdleTexture;
        // Texture2D newFireTexture;
        // var newIdleFrameCount = 1;
        // var newFireFrameCount = 1;

        if (newUpgrade.Name == Upgrade.BigBall.ToString())
        {
            // newIdleTexture = AssetManager.GetTexture("railgun_antimatterlaser_idle");
            // newFireTexture = AssetManager.GetTexture("railgun_antimatterlaser_fire");
            // newIdleFrameCount = 4;
            // newFireFrameCount = 6;
            damage += 30;
        }
        else if (newUpgrade.Name == Upgrade.ExplosivePayload.ToString())
        {
            damage = 120;
        }
        else if (newUpgrade.Name == Upgrade.Crusher.ToString())
        {
            damage += 180;
            pierce += 10;
        }
        else if (newUpgrade.Name == Upgrade.ChargedLifts.ToString())
        {
            cooldownTime -= 1;
        }
        else
        {
            // razorball
            damage = 120;
        }

        // var newIdleAnimation = new AnimationSystem.AnimationData
        // (
        //     texture: newIdleTexture,
        //     frameCount: newIdleFrameCount,
        //     frameSize: new Vector2(newIdleTexture.Width / newIdleFrameCount, newIdleTexture.Height),
        //     delaySeconds: 0.1f
        // );

        // var newFireAnimation = new AnimationSystem.AnimationData
        // (
        //     texture: newFireTexture,
        //     frameCount: newFireFrameCount,
        //     frameSize: new Vector2(newFireTexture.Width / newFireFrameCount, newFireTexture.Height),
        //     delaySeconds: 0.05f
        // );

        // AnimationSystem!.ChangeAnimationState(null, newIdleAnimation);
        // AnimationSystem.ChangeAnimationState("fire", newFireAnimation);

    }

    public static float GetBaseRange() => 0f;

    public float GetRange() => 0f;

    public TowerCore GetTowerCore() => towerCore;

    public static void DrawBaseRangeIndicator(Vector2 worldPosition) { }
    public void DrawRangeIndicator() { }
}
