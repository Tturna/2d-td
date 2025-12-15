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

        DrawOrigin = Size / 2;
        DrawOffset = Size / 2;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var velX = PhysicsSystem.Velocity.X;
        Rotate(velX * 10 * deltaTime);

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
            SetPosition(newPosition, force: true);

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

        SetPosition(newPosition, force: true);
    }

    public override void SetPosition(Vector2 newPosition, bool force = false)
    {
        if (!force)
        {
            var oldBinGridPosition = ScrapSystem.Corpses.WorldToGridPosition(Position);
            var newBinGridPosition = ScrapSystem.Corpses.WorldToGridPosition(newPosition);

            if (oldBinGridPosition != newBinGridPosition)
            {
                var canRemove = ScrapSystem.Corpses.Remove(this);
                SetPosition(newPosition, force: true);

                if (!canRemove)
                {
                    throw new InvalidOperationException($"Couldn't remove enemy ({this}) from bin grid. Either it doesn't exist or its state in the grid is wrong.");
                }

                ScrapSystem.Corpses.Add(this);
                return;
            }
        }

        base.SetPosition(newPosition, force);
    }

    public void ClimbUp(Vector2 climbVelocity)
    {
        UpdatePosition(climbVelocity);

        var enemyCandidates = EnemySystem.EnemyBins.GetBinAndNeighborValues(Position + Size / 2);

        foreach (var enemy in enemyCandidates)
        {
            if (!Collision.AreEntitiesColliding(this, enemy)) continue;

            enemy.UpdatePosition(climbVelocity);
        }

        var corpseCandidates = ScrapSystem.Corpses.GetBinAndNeighborValues(Position + Size / 2);

        foreach (var corpse in corpseCandidates)
        {
            if (this == corpse) continue;
            if (!Collision.AreEntitiesColliding(this, corpse)) continue;

            corpse.UpdatePosition(climbVelocity);
        }
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
