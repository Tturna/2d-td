using System;
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
        if (IsDestroyed) return;
        if (EnemySystem.EnemyBins.TotalValueCount == 0) return;

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        PhysicsSystem.UpdatePhysics(this, deltaTime);
        base.Update(gameTime);
    }

    public override void UpdatePosition(Vector2 positionChange)
    {
        if (IsDestroyed) return;

        var newPosition = Position + positionChange;
        var oldBinGridPosition = ScrapSystem.Corpses.WorldToGridPosition(Position);
        var newBinGridPosition = ScrapSystem.Corpses.WorldToGridPosition(newPosition);

        if (oldBinGridPosition != newBinGridPosition)
        {
            var canRemove = ScrapSystem.Corpses.Remove(this);
            SetPosition(newPosition);

            if (!canRemove)
            {
                Console.WriteLine($"Components contains this scrap: {Game.Components.Contains(this)}");
                throw new InvalidOperationException($"Couldn't remove corpse ({this}) from bin grid. Either it doesn't exist or its state in the grid is wrong.");
            }

            ScrapSystem.Corpses.Add(this);
            return;
        }

        SetPosition(newPosition);
    }

    public override void Destroy()
    {
        ScrapSystem.Corpses.Remove(this);
        base.Destroy();
    }

    public void ApplyKnockback(Vector2 knockback)
    {
        PhysicsSystem.StopMovement();
        PhysicsSystem.AddForce(knockback);
    }
}
