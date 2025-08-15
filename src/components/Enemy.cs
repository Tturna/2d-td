using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class Enemy : Entity
{
    public HealthSystem HealthSystem;
    public PhysicsSystem PhysicsSystem;
    public MovementSystem MovementSystem;
    public AnimationSystem AnimationSystem;

    public Enemy(Game game, Vector2 position, Vector2 size, MovementSystem.MovementData movementData,
        AnimationSystem.AnimationData animationData) : base(game, position, size: size)
    {
        HealthSystem = new HealthSystem(owner: this, initialHealth: 100);
        HealthSystem.Died += OnDeath;

        PhysicsSystem = new PhysicsSystem(Game);
        MovementSystem = new MovementSystem(Game, movementData);
        AnimationSystem = new AnimationSystem(animationData);
    }

    public override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
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
        AnimationSystem.UpdateAnimation(deltaTime);
    }

    public override void Draw(GameTime gameTime)
    {
        AnimationSystem.Draw(Game.SpriteBatch, Position, RotationRadians, DrawOrigin, DrawLayerDepth);
    }

    private void OnDeath(Entity diedEntity)
    {
        Game.Components.Remove(this);
        EnemySystem.Enemies.Remove(this);
    }
}
