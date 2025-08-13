using Microsoft.Xna.Framework;

namespace _2d_td;

public class MovementSystem
{
    public enum MovementPattern
    {
        Charge
        // Add more if an enemy should do something other than charge to the right side of
        // the screen
    }

    public struct MovementData
    {
        public MovementPattern Pattern;
        public bool CanWalk;
        public bool CanFly;
        public float WalkSpeed;
        public float FlySpeed;
        public float JumpForce;
    }

    private Game1 game;
    private Vector2 defaultChargeDirection = Vector2.UnitX;
    private float jumpTimer;
    private float jumpInterval = 0.5f;
    private float jumpCheckDistanceFactor = 1.5f;

    public MovementData CurrentData { get; private set; }

    public MovementSystem(Game1 game, MovementData data)
    {
        this.game = game;
        CurrentData = data;
    }

    public void UpdateMovement(Entity entity, GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (jumpTimer > 0f)
        {
            jumpTimer -= deltaTime;

            if (jumpTimer <= 0f)
            {
                jumpTimer = 0f;
            }
        }

        switch (CurrentData.Pattern)
        {
            case MovementPattern.Charge:
                HandleCharge(entity, deltaTime);
                break;
        }
    }

    private void HandleCharge(Entity entity, float deltaTime)
    {
        if (CurrentData.CanWalk)
        {
            var jumpCheckPoint = entity.Position + defaultChargeDirection * jumpCheckDistanceFactor * Grid.TileLength;
            var groundCheckPoint = entity.Position + Vector2.UnitY * Grid.TileLength;

            if (jumpTimer == 0f &&
                Collision.IsPointInTerrain(jumpCheckPoint, game.Terrain) &&
                Collision.IsPointInTerrain(groundCheckPoint, game.Terrain) &&
                entity is Enemy)
            {
                var enemy = (Enemy)entity;
                enemy.PhysicsSystem.AddForce(-Vector2.UnitY * CurrentData.JumpForce);
                enemy.PhysicsSystem.AddForce(defaultChargeDirection * CurrentData.WalkSpeed * deltaTime);
                jumpTimer = jumpInterval;
            }

            entity.Position += defaultChargeDirection * CurrentData.WalkSpeed * deltaTime;
        }
        // TODO: Implement flying enemy logic and shi
    }
}
