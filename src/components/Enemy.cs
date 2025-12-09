using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Enemy : Entity, IKnockable
{
    public HealthSystem HealthSystem;
    public PhysicsSystem PhysicsSystem;
    public MovementSystem MovementSystem;
    public int ScrapValue;

    private double hurtProgress;
    private double hurtAnimThreshold;
    private int attackDamage = 10;
    private float selfDestructTime = 8;
    private float selfDestructTimer;
    private Vector2 lastPosition;
    private readonly int yKillThreshold = 100 * Grid.TileLength;

    private static Texture2D explosionSprite = AssetManager.GetTexture("death_explosion_small");
    private AnimationSystem.AnimationData deathExplosionAnimation = new AnimationSystem.AnimationData(
        texture: explosionSprite,
        frameCount: 6,
        frameSize: new Vector2(explosionSprite.Width / 6, explosionSprite.Height),
        delaySeconds: 0.05f);

    public Enemy(Game game, Vector2 position, Vector2 size, MovementSystem.MovementData movementData,
        AnimationSystem.AnimationData animationData, int health,
        int scrapValue) : base(game, position, animationData)
    {
        HealthSystem = new HealthSystem(owner: this, initialHealth: health);
        HealthSystem.Died += OnDeath;
        HealthSystem.Damaged += OnDamaged;

        PhysicsSystem = new PhysicsSystem(Game);
        MovementSystem = new MovementSystem(Game, movementData);
        ScrapValue = scrapValue;
        hurtAnimThreshold = 0.33 * HealthSystem.MaxHealth;
        selfDestructTimer = selfDestructTime;
    }

    public override void Update(GameTime gameTime)
    {
        if (IsDestroyed) return;

        if (!Game.Components.Contains(this))
        {
            // Apparently MonoGame will call Update on a component that has already been
            // removed from Components. I guess this can happen if something removes a
            // component from Components in the same frame it would be updated.
            return;
        }

        if (Collision.AreEntitiesColliding(this, HQ.Instance))
        {
            HQ.Instance.HealthSystem.TakeDamage(attackDamage);
            Destroy();
            return;
        }

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var posDiff = Position - lastPosition;
        lastPosition = Position;
        var rawXVelocity = MathF.Abs(posDiff.X);

        if (rawXVelocity > 0.1f)
        {
            selfDestructTimer = selfDestructTime;
        }
        else
        {
            selfDestructTimer -= deltaTime;

            if (selfDestructTimer <= 0)
            {
                EffectUtility.Explode(Position, radius: 3 * Grid.TileLength, magnitude: 20f,
                    damage: 10);
                OnDeath(this);
            }
        }

        HealthSystem.UpdateHealthBarGraphics(deltaTime);

        base.Update(gameTime);
    }

    public override void FixedUpdate(float deltaTime)
    {
        MovementSystem.UpdateMovement(this, deltaTime);
        PhysicsSystem.UpdatePhysics(this, deltaTime);
    }

    public override void Draw(GameTime gameTime)
    {
        HealthSystem.DrawHealthBar(Position + new Vector2(Size.X / 2, -4));
        base.Draw(gameTime);
    }

    public override void UpdatePosition(Vector2 positionChange)
    {
        var newPosition = Position + positionChange;

        if (newPosition.Y >= yKillThreshold)
        {
            Destroy();
            return;
        }

        var oldBinGridPosition = EnemySystem.EnemyBins.WorldToGridPosition(Position);
        var newBinGridPosition = EnemySystem.EnemyBins.WorldToGridPosition(newPosition);

        if (oldBinGridPosition != newBinGridPosition)
        {
            var canRemove = EnemySystem.EnemyBins.Remove(this);
            SetPosition(newPosition, force: true);

            if (!canRemove)
            {
                throw new InvalidOperationException($"Couldn't remove enemy ({this}) from bin grid. Either it doesn't exist or its state in the grid is wrong.");
            }

            EnemySystem.EnemyBins.Add(this);
            return;
        }

        SetPosition(newPosition, force: true);
    }

    public override void SetPosition(Vector2 newPosition, bool force = false)
    {
        if (!force)
        {
            var oldBinGridPosition = EnemySystem.EnemyBins.WorldToGridPosition(Position);
            var newBinGridPosition = EnemySystem.EnemyBins.WorldToGridPosition(newPosition);

            if (oldBinGridPosition != newBinGridPosition)
            {
                var canRemove = EnemySystem.EnemyBins.Remove(this);
                SetPosition(newPosition, force: true);

                if (!canRemove)
                {
                    throw new InvalidOperationException($"Couldn't remove enemy ({this}) from bin grid. Either it doesn't exist or its state in the grid is wrong.");
                }

                EnemySystem.EnemyBins.Add(this);
                return;
            }
        }

        base.SetPosition(newPosition, force);
    }

    public void ApplyKnockback(Vector2 knockback)
    {
        PhysicsSystem.StopMovement();
        PhysicsSystem.AddForce(knockback);
        selfDestructTimer = selfDestructTime;
    }

    private void OnDamaged(Entity damagedEntity, int amount)
    {
        hurtProgress += amount;
        if (hurtProgress >= hurtAnimThreshold)
        {
            hurtProgress = 0;
        }

        StretchImpact(new Vector2(1.8f, 0.4f), 0.1f);
        ParticleSystem.PlayBotchunkExplosion(Position + Size / 2);
        UIComponent.SpawnFlyoutText(amount.ToString(), Position - Vector2.UnitY * (Size.Y + 2), -Vector2.UnitY * 12, lifetime: 1f,
            color: Color.FromNonPremultiplied(new Vector4(249f/255f, 72f/255f, 88f/255f, 1f)));
    }

    public override void Destroy()
    {
        EnemySystem.EnemyBins.Remove(this);
        base.Destroy();
    }

    public void Knockback(Vector2 direction,float force)
    {
        PhysicsSystem.AddForce(direction*force);
    }

    private void OnDeath(Entity diedEntity)
    {
        EffectUtility.Explode(Position + Size / 2, Size.X * 2f, magnitude: 10f, damage: 0,
            animation: deathExplosionAnimation);

        var corpseSprite = AssetManager.GetTexture("node_corpse");
        var anim = new AnimationSystem.AnimationData(
            texture: corpseSprite,
            frameCount: 1,
            frameSize: new Vector2(corpseSprite.Width, corpseSprite.Height),
            delaySeconds: float.PositiveInfinity);

        anim.DelaySeconds = float.PositiveInfinity;
        ScrapSystem.AddCorpse(Game, Position, anim, ScrapValue, knockback: -Vector2.UnitX);
        Destroy();
    }
}
