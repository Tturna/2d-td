using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class MortarShell : Entity
{
    public delegate void DestroyedHandler(Vector2 previousVelocity);
    public event DestroyedHandler Destroyed;

    public PhysicsSystem physics;
    public bool Homing;

    private Enemy closestEnemy;
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
        shellCenter = Position + Size / 2;
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

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
                closestEnemy = null;
                differenceToClosestEnemy = null;

                if (!EnemySystem.EnemyTree.TryGetSmallestQuad(shellCenter, out var smallestQuad))
                {
                    // shell is outside of enemy quad tree. Don't trigger real shell impact
                    Destroy();
                }

                var enemyCandidates = smallestQuad.Values;

                foreach (var enemy in enemyCandidates)
                {
                    // Target the bottom part of enemies to hit the ground
                    var targetEnemyPosition = enemy.Position + new Vector2(enemy.Size.X / 2, enemy.Size.Y);
                    var diff = targetEnemyPosition - shellCenter;
                    var distance = diff.Length();

                    if (differenceToClosestEnemy is null || distance < ((Vector2)differenceToClosestEnemy).Length())
                    {
                        closestEnemy = enemy;
                        differenceToClosestEnemy = diff;
                    }
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

        if (!collided)
        {
            var collisionDistance = Math.Max(Size.X, Size.Y);
            var enemyCandidates = EnemySystem.EnemyTree.GetValuesInOverlappingQuads(shellCenter, (int)collisionDistance);

            foreach (var enemy in enemyCandidates)
            {
                var diff = enemy.Position + enemy.Size / 2 - Position + Size / 2;
                var distance = diff.Length();

                if (distance > collisionDistance) continue;

                collided = true;
                break;
            }
        }

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
