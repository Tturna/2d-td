using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public class Crane : Entity, ITower
{
    private TowerCore towerCore;
    private Entity? ballThing;

    private const float TriggerMargin = 3f;

    private float activeBallTime = 3f;
    private float cooldownTime = 1f;
    private float actionTimer, cooldownTimer;
    private Vector2 defaultBallOffset = new Vector2(-8, 0);

    public Crane(Game game) : base(game, GetTowerBaseSprite())
    {
        towerCore = new TowerCore(this);
    }

    public override void Initialize()
    {
        ballThing = new Entity(Game, GetBallSprite(Game.SpriteBatch));
        ballThing.Position = Position + defaultBallOffset;
        Game.Components.Add(ballThing);
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (cooldownTimer > 0)
        {
            cooldownTimer -= deltaTime;

            if (cooldownTimer <= 0)
            {
                cooldownTimer = 0;
            }
            else
            {
                return;
            }
        }

        if (actionTimer > 0)
        {
            actionTimer -= deltaTime;

            if (actionTimer <= 0)
            {
                actionTimer = 0;
                ballThing!.Position = Position + defaultBallOffset;
                cooldownTimer = cooldownTime;
            }

            return;
        }

        if (IsEnemyBelow())
        {
            Trigger();
        }
    }

    private bool IsEnemyBelow()
    {
        var towerTestPosition = ballThing!.Position + ballThing.Size / 2;
        foreach (var enemy in EnemySystem.Enemies)
        {
            var enemyTestPosition = enemy.Position + enemy.Size / 2;

            if (enemyTestPosition.X < towerTestPosition.X + TriggerMargin &&
                enemyTestPosition.X > towerTestPosition.X - TriggerMargin)
            {
                return true;
            }
        }

        return false;
    }

    private void Trigger()
    {
        actionTimer = activeBallTime;
        var groundCheckPosition = ballThing!.Position;
        
        while (ScrapSystem.GetScrapFromPosition(groundCheckPosition) is null &&
            !Collision.IsPointInTerrain(groundCheckPosition, Game.Terrain))
        {
            groundCheckPosition += Vector2.UnitY * Grid.TileLength;
        }

        groundCheckPosition -= Vector2.UnitY * Grid.TileLength;
        ballThing.Position = groundCheckPosition;

        for (int i = EnemySystem.Enemies.Count - 1; i >= 0; i--)
        {
            if (i >= EnemySystem.Enemies.Count) continue;

            var enemy = EnemySystem.Enemies[i];
            var diff = (ballThing.Position + ballThing.Size / 2) - (enemy.Position + enemy.Size / 2);
            var distance = diff.Length();

            if (distance > Grid.TileLength) continue;

            enemy.HealthSystem.TakeDamage(30);
        }
    }

    private static Texture2D GetBallSprite(SpriteBatch spriteBatch)
    {
        var texture = new Texture2D(spriteBatch.GraphicsDevice, width: Grid.TileLength, height: Grid.TileLength,
        mipmap: false, SurfaceFormat.Color);

        var colorData = new Color[Grid.TileLength * Grid.TileLength];

        for (var i = 0; i < colorData.Length; i++)
        {
            colorData[i] = Color.White;
        }

        texture.SetData(colorData);

        return texture;
    }

    public override void Destroy()
    {
        Game.Components.Remove(ballThing);
        base.Destroy();
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        return TowerCore.DefaultCanPlaceTower(GetDefaultGridSize(), targetWorldPosition);
    }

    public static Entity CreateNewInstance(Game game)
    {
        return new Crane(game);
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(2, 2);
    }

    public static Texture2D GetTowerBaseSprite()
    {
        return AssetManager.GetTexture("turretTwo");
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Crane;
    }
}
