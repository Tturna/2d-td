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
    double hurtProgress;
    double hurtAnimThreshold;
    private int attackDamage = 10;
    public int ScrapValue;

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

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
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

    private void OnDeath(Entity diedEntity)
    {
        EnemySystem.Enemies.Remove(this);
        CurrencyManager.AddBalance(ScrapValue);
        ScrapSystem.AddScrap(Game, Position);
        Destroy();
    }


}
