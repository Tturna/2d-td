using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public class MortarShell : Entity
{
    public delegate void DestroyedHandler(Vector2 previousVelocity);
    public event DestroyedHandler? Destroyed;

    public PhysicsSystem physics;
    public bool Homing;

    private Enemy? closestEnemy;
    private Vector2? differenceToClosestEnemy;
    private float homingDelayTimer;
    private float lifeTime = 5f;
    private float homingSpeed = 7f;
    private float homingDragFactor = 0.02f;
    private float directionCorrectionThreshold = 0.6f;
    private float directionCorrectionSpeed = 1.5f;
    private Vector2 shellCenter;

    public MortarShell(Game1 game) : base(game, position: null, GetShellTexture(game.SpriteBatch))
    {
        physics = new PhysicsSystem(Game);
        var rng = new Random();
        homingDelayTimer = 0.5f + (float)rng.NextDouble() * 0.2f;
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        shellCenter = Position + Size / 2;

        lifeTime -= deltaTime;

        if (Homing)
        {
            if (homingDelayTimer > 0)
            {
                homingDelayTimer -= deltaTime;
            }
            else
            {
                physics.DragFactor = homingDragFactor;
                closestEnemy = EnemySystem.EnemyBins.GetClosestValue(shellCenter);
                differenceToClosestEnemy = null;

                if (closestEnemy is not null)
                {
                    differenceToClosestEnemy = closestEnemy.Position + closestEnemy.Size / 2 - shellCenter;
                }

                if (differenceToClosestEnemy is not null)
                {
                    var targetDirection = (Vector2)differenceToClosestEnemy;
                    targetDirection.Normalize();

                    var velocityDirection = physics.Velocity;
                    velocityDirection.Normalize();

                    var dot = Vector2.Dot(targetDirection, velocityDirection);

                    if (dot < directionCorrectionThreshold)
                    {
                        physics.AddForce(-physics.Velocity * deltaTime * directionCorrectionSpeed);
                    }

                    physics.AddForce(targetDirection * homingSpeed * deltaTime);
                }
                else
                {
                    // If there are no enemies, just drift until something is hit.
                    physics.DragFactor = 0f;
                }
            }
        }

        var oldVelocity = physics.Velocity;
        var collided = physics.UpdatePhysics(this, deltaTime);

        if (lifeTime <= 0 || collided) DestroyShell(oldVelocity);

        base.Update(gameTime);
    }

    public void DestroyShell(Vector2 previousVelocity)
    {
        Destroyed?.Invoke(previousVelocity);
        Destroy();
    }

    private static Texture2D GetShellTexture(SpriteBatch spriteBatch)
    {
        var tex = TextureUtility.GetBlankTexture(spriteBatch, 4, 4, Color.White);
        return tex;
    }
}
