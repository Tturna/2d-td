using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
// using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class Enemy : Entity
{
    private Texture2D hurtTexture;
    private float hurtTimeSeconds = 0.1f;

    public HealthSystem HealthSystem;
    public PhysicsSystem PhysicsSystem;
    public MovementSystem MovementSystem;
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

        this.hurtTexture = hurtTexture;
    }

    public override void Update(GameTime gameTime)
    {
        // Used to test enemy physics
        // if (Keyboard.GetState().IsKeyDown(Keys.H))
        // {
        //     PhysicsSystem.AddForce(-Vector2.UnitX * 0.5f);
        // }
        //
        // if (Keyboard.GetState().IsKeyDown(Keys.L))
        // {
        //     PhysicsSystem.AddForce(Vector2.UnitX * 0.5f);
        // }
        //
        // if (Keyboard.GetState().IsKeyDown(Keys.J))
        // {
        //     PhysicsSystem.AddForce(Vector2.UnitY);
        // }
        //
        // if (Keyboard.GetState().IsKeyDown(Keys.K))
        // {
        //     PhysicsSystem.AddForce(-Vector2.UnitY);
        // }

        MovementSystem.UpdateMovement(this, gameTime);
        PhysicsSystem.UpdatePhysics(this, gameTime);

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }

    private void OnDamaged(Entity damagedEntity)
    {
        AnimationSystem.OverrideTexture(hurtTexture, hurtTimeSeconds);
    }

    private void OnDeath(Entity diedEntity)
    {
        EnemySystem.Enemies.Remove(this);
        CurrencyManager.AddBalance(ScrapValue);
        Destroy();
    }
}
