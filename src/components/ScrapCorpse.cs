using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class ScrapCorpse : Entity, IKnockable
{
    public PhysicsSystem PhysicsSystem;

    public ScrapCorpse(Game game, Vector2 position, AnimationSystem.AnimationData animationData) :
        base(game, position, animationData) { }

    public override void Initialize()
    {
        PhysicsSystem = new PhysicsSystem(Game);
        PhysicsSystem.StopMovement();
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        PhysicsSystem.UpdatePhysics(this, deltaTime);
        base.Update(gameTime);
    }

    public void ApplyKnockback(Vector2 knockback)
    {
        PhysicsSystem.StopMovement();
        PhysicsSystem.AddForce(knockback);
    }
}
