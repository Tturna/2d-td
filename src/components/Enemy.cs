using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

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

        base.Update(gameTime);
    }

    public override void FixedUpdate(float deltaTime)
    {
        MovementSystem.UpdateMovement(this, deltaTime);
        PhysicsSystem.UpdatePhysics(this, deltaTime);
    }

    public override void Draw(GameTime gameTime)
    {
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
            SetPosition(newPosition);

            if (!canRemove)
            {
                throw new InvalidOperationException($"Couldn't remove enemy ({this}) from bin grid. Either it doesn't exist or its state in the grid is wrong.");
            }

            EnemySystem.EnemyBins.Add(this);
            return;
        }

        SetPosition(newPosition);
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

        StretchImpact(new Vector2(1.3f, 0.7f), 0.1f);
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
        EffectUtility.Explode(Position + Size / 2, Size.X * 2f, magnitude: 10f, damage: 0);

        var anim = AnimationSystem.BaseAnimationData;
        anim.DelaySeconds = float.PositiveInfinity;
        ScrapSystem.AddCorpse(Game, Position, anim, ScrapValue, knockback: Vector2.UnitX * 0.7f);
        Destroy();
    }
}
