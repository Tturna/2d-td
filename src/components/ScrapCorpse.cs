using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class ScrapCorpse : Entity, IKnockable
{
    public PhysicsSystem PhysicsSystem;
    public int ScrapValue;

    public ScrapCorpse(Game game, Vector2 position, AnimationSystem.AnimationData animationData,
        int scrapValue) :
        base(game, position, animationData)
    {
        ScrapValue = scrapValue;
        PhysicsSystem = new PhysicsSystem(Game);
        PhysicsSystem.StopMovement();
        PhysicsSystem.LocalGravity = 0.25f;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void FixedUpdate(float deltaTime)
    {
        if (IsDestroyed) return;
        if (ScrapSystem.Corpses.TotalValueCount == 0) return;

        PhysicsSystem.UpdatePhysics(this, deltaTime);
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
                // throw new InvalidOperationException($"Couldn't remove corpse ({this}) from bin grid. Either it doesn't exist or its state in the grid is wrong.");
                Console.WriteLine($"Couldn't remove corpse ({this}) from bin grid. Either it doesn't exist or its state in the grid is wrong.");
                base.Destroy();
                return;
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
