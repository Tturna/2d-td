using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Enemy : Entity
{
    private Texture2D hurtTexture;
    private float hurtTimeSeconds = 0.1f;

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

    public Enemy(Game game, Vector2 position, Vector2 size, MovementSystem.MovementData movementData,
        AnimationSystem.AnimationData animationData, Texture2D hurtTexture, int health,
        int scrapValue) : base(game, position, animationData)
    {
        HealthSystem = new HealthSystem(owner: this, initialHealth: health);
        HealthSystem.Died += OnDeath;
        HealthSystem.Damaged += OnDamaged;

        PhysicsSystem = new PhysicsSystem(Game);
        MovementSystem = new MovementSystem(Game, movementData);
        ScrapValue = scrapValue;
        hurtAnimThreshold = .33*HealthSystem.MaxHealth;

        this.hurtTexture = hurtTexture;

        selfDestructTimer = selfDestructTime;
    }

    public override void Update(GameTime gameTime)
    {
        if (Collision.AreEntitiesColliding(this, HQ.Instance))
        {
            HQ.Instance.HealthSystem.TakeDamage(attackDamage);
            EnemySystem.Enemies.Remove(this);
            Destroy();
        }

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        MovementSystem.UpdateMovement(this, gameTime);
        PhysicsSystem.UpdatePhysics(this, deltaTime);

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

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
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
            AnimationSystem.OverrideTexture(hurtTexture, hurtTimeSeconds);
            hurtProgress = 0;
        }
    }

    public void Knockback(Vector2 direction,float force)
    {
        PhysicsSystem.AddForce(direction*force);
    }

    private void OnDeath(Entity diedEntity)
    {
        EnemySystem.Enemies.Remove(this);
        CurrencyManager.AddBalance(ScrapValue);
        ScrapSystem.AddScrap(Game, Position);
        Destroy();
    }
}
