using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class CameraManager : GameComponent
{
    private Game1 game;
    private float currentScale = 1;

    private float cameraShakeDuration;
    private float cameraShakeDurationLeft;
    private float cameraShakeStrength;
    private Vector2 preShakePosition;
    private Random rng = new();

    public static CameraManager Instance;

    public CameraManager(Game game) : base(game)
    {
        this.game = (Game1)game;
        Instance = this;
    }

    public override void Initialize()
    {
        var initCamPos = new Vector2(400, 400);
        var failsafe = 0;

        while (!Collision.IsPointInTerrain(initCamPos, game.Terrain))
        {
            initCamPos += Vector2.UnitY * Grid.TileLength;
            failsafe++;

            if (failsafe > 300)
            {
                initCamPos = new Vector2(400, 400);
                break;
            }
        }

        initCamPos -= Vector2.UnitY * Grid.TileLength * 8;
        Camera.Position = initCamPos;
        preShakePosition = Camera.Position;

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState currentState = Keyboard.GetState();

        Vector2 posChange = Vector2.Zero;

        var Speed = 500;
        if (currentState.IsKeyDown(Keys.A))
            posChange.X -= deltaTime * Speed;
        if (currentState.IsKeyDown(Keys.D))
            posChange.X += deltaTime * Speed;
        if (currentState.IsKeyDown(Keys.W))
            posChange.Y -= deltaTime * Speed;
        if (currentState.IsKeyDown(Keys.S))
            posChange.Y += deltaTime * Speed;

        // scroll down is negative
        // var scroll = -(float)InputSystem.mouseJustScrolledAmount() / 1000f;
        // currentScale = Math.Max(1, currentScale + scroll);
        // eg. 1/8 will make it zoom out 8x
        // var totalCameraScale = 1 / currentScale;
        // Camera.Scale = totalCameraScale;

        preShakePosition += posChange;

        var (minBound, maxBound) = game.Terrain.GetPlayableTerrainBounds();

        // Prevent camera from seeing into enemy spawn area to the left
        minBound += Vector2.UnitX * game.NativeScreenWidth / 2;

        preShakePosition = Vector2.Clamp(preShakePosition, minBound, maxBound);
        Camera.Position = preShakePosition;
        
        if (cameraShakeDurationLeft > 0)
        {
            var randomAngleRadians = (float)rng.NextDouble() * MathHelper.Tau;
            var rx = MathF.Cos(randomAngleRadians);
            var ry = MathF.Sin(randomAngleRadians);
            var randomUnitVector = new Vector2(rx, ry);

            Camera.Position += randomUnitVector * cameraShakeStrength;
            cameraShakeDurationLeft -= deltaTime;
        }

        base.Update(gameTime);
    }

    public void ShakeCamera(float strength, float duration)
    {
        cameraShakeStrength = strength;
        cameraShakeDuration = duration;
        cameraShakeDurationLeft = duration;
    }
}
