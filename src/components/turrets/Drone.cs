using System;
using System.ComponentModel;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
class Drone : Entity, ITower
{
    private TowerCore towerCore;
    private Vector2 spawnOffset = new (0, 11);
    int baseRange = 10;
    int damage = 10;
    float bulletSpeed = 400f;
    float actionsPerSecond = 2f;
    float actionTimer;

    public enum Upgrade
    {
        NoUpgrade,
        AdvancedWeaponry,
        FlyingArsenal,
        ImprovedRadar,
        AssasinDrone,
        UAV,
    }

    public Drone(Game game) : base(game, GetTowerBaseSprite())
    {
        towerCore = new TowerCore(this);

        // towerCore.CurrentUpgrade = defaultNode;
    }

    public Drone(Game game, Vector2 position) : this(game)
    {
        Position = position;
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        HandleBasicShots(deltaTime, actionsPerSecond, damage, baseRange, 15f);

        base.Update(gameTime);
    }

    private void HandleBasicShots(float deltaTime, float actionsPerSecond, int damage, int range, float attackAngle)
    {
        var actionInterval = 1f / actionsPerSecond;

        actionTimer += deltaTime;

        var degree = (float)(attackAngle * Math.PI / 180);

        var closestEnemy = GetValidEnemy(range, degree);

        if (closestEnemy is null) return;

        if (actionTimer >= actionInterval)
        {
            var enemyCenter = closestEnemy.Position + closestEnemy.Size / 2;
            var direction = enemyCenter - Position;
            direction.Normalize();
            Shoot(damage, direction);
            actionTimer = 0f;
        }
    }

    private void Shoot(int damage, Vector2 direction)
    {
        var bullet = new Projectile(Game, Position + spawnOffset);
        bullet.Direction = direction;
        bullet.BulletPixelsPerSecond = bulletSpeed;
        bullet.Damage = damage;
        bullet.Lifetime = 1f;
        bullet.BulletLength = 20f;
        bullet.BulletWidth = 8f;
        bullet.Sprite = AssetManager.GetTexture("tempprojectile");
        Game.Components.Add(bullet);
    }

    private Enemy? GetValidEnemy(int tileRange, float attackAngle)
    {
        Enemy? closestEnemy = null;
        float closestDistance = float.PositiveInfinity;

        // TODO: Don't loop over all enemies. Just the ones in range.
        foreach (Enemy enemy in EnemySystem.Enemies)
        {
            var distanceToEnemy = Vector2.Distance(Position, enemy.Position);

            if (distanceToEnemy > tileRange * Grid.TileLength)
                continue;

            var enemyCenter = enemy.Position + enemy.Size / 2;
            var direction = Math.Atan2(enemyCenter.Y - Position.Y, enemyCenter.X - Position.X);

            if (Math.Abs(direction) < (Math.PI)-(attackAngle/2))
            {
                continue;
            }

            if (distanceToEnemy < closestDistance)
            {
                var towerCenter = Position + Size / 2;
                if (Collision.IsLineInTerrain(towerCenter, enemyCenter)) continue;

                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    public override void Destroy()
    {
        towerCore.CloseDetailsView();
        // Game.Components.Remove(turretHead);
        Game.Components.Remove(towerCore);

        base.Destroy();
    }

    public static Texture2D GetTowerBaseSprite()
    {
        return AssetManager.GetTexture("railgun");
    }

    public static Vector2 GetDefaultGridSize()
    {
        return new Vector2(3, 2);
    }

    public static BuildingSystem.TowerType GetTowerType()
    {
        return BuildingSystem.TowerType.Drone;
    }

    public static bool CanPlaceTower(Vector2 targetWorldPosition)
    {
        // todo: improve
        var towerGridSize = GetDefaultGridSize();
        var targetGridPosition = Grid.SnapPositionToGrid(targetWorldPosition);

        for (int y = 0; y < towerGridSize.Y; y++)
        {
            for (int x = 0; x < towerGridSize.X; x++)
            {
                var position = targetGridPosition + new Vector2(x, y) * Grid.TileLength;

                if (Collision.IsPointInTerrain(position, Game1.Instance.Terrain))
                {
                    return false;
                }
            }
        }

        var turretGridHeight = towerGridSize.Y;

        var belowTilePosition = targetGridPosition + Vector2.UnitY * turretGridHeight * Grid.TileLength;
        var aboveTilePosition = targetGridPosition - Vector2.UnitY * Grid.TileLength;
        var leftTilePosition = targetGridPosition - Vector2.UnitX * Grid.TileLength;
        var rightTilePosition = targetGridPosition + Vector2.UnitX * Grid.TileLength;

        if (Collision.IsPointInTerrain(belowTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        if (Collision.IsPointInTerrain(aboveTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        if (Collision.IsPointInTerrain(leftTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        if (Collision.IsPointInTerrain(rightTilePosition, Game1.Instance.Terrain))
        {
            return false;
        }

        return true;
    }

    public static Entity CreateNewInstance(Game game)
    {
        return new Drone(game);
    }
}
