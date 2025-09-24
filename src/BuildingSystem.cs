using System;
using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class BuildingSystem
{
    public enum TowerType
    {
        None,
        GunTurret,
        Railgun,
        Drone,
        Crane,
        Mortar
    }

    private static Game1 game;
    private static TimeSpan lastGameTime;
    private static TowerType selectedTowerType;
    private static TimeSpan allowedTurretPlacementTime;
    private static Func<Vector2, bool> canPlaceTowerCallback;
    private static Func<Game, Vector2, Entity> createTowerInstanceCallback;

    public static bool CanPlaceTurret { get; private set; }

    public static void Initialize(Game game)
    {
        BuildingSystem.game = (Game1)game;
    }

    public static void Update(GameTime gameTime)
    {
        lastGameTime = gameTime.TotalGameTime;
        if (game is null) return;
        if (selectedTowerType == TowerType.None) return;

        var gridMousePosition = Grid.SnapPositionToGrid(InputSystem.GetMouseWorldPosition());

        // TODO: Make a system that doesn't require collision checks against every entity.
        // This could be done by connecting tiles or tile coordinates to entities and checking
        // if the tile under the mouse has a connected entity.
        var isColliding = false;

        foreach (var component in game.Components)
        {
            if (isColliding) break;

            if (component is not Entity) continue;

            var entity = (Entity)component;

            if (entity.Position == gridMousePosition)
            {
                isColliding = true;
            }
        }

        var turretAllowsPlacement = true;

        if (canPlaceTowerCallback is not null)
        {
            turretAllowsPlacement = canPlaceTowerCallback(gridMousePosition);
        }

        CanPlaceTurret = !isColliding &&
            turretAllowsPlacement &&
            gameTime.TotalGameTime > allowedTurretPlacementTime &&
            selectedTowerType != TowerType.None;

        if (InputSystem.IsLeftMouseButtonClicked() && CanPlaceTurret)
        {
            TrySpawnTurret(gridMousePosition);
        }
    }

    private static bool TrySpawnTurret(Vector2 position)
    {
        if (!CurrencyManager.TryBuyTower(selectedTowerType)) return false;

        var spawnedTurret = createTowerInstanceCallback(game, position);
        return true;
    }

    public static void SelectTurret<T>() where T : ITower
    {
        allowedTurretPlacementTime = lastGameTime.Add(TimeSpan.FromMilliseconds(200));
        selectedTowerType = T.GetTowerType();
        canPlaceTowerCallback = T.CanPlaceTower;
        createTowerInstanceCallback = T.CreateNewInstance;
    }

    public static void DeselectTower()
    {
        selectedTowerType = TowerType.None;
        canPlaceTowerCallback = null;
        createTowerInstanceCallback = null;
    }

    // TODO: Consider taking this out and adding the ability to get the type of a tower
    // in ITower.
    public static TowerType GetTurretTypeFromEntity(Entity turretEntity)
    {
        return turretEntity switch
        {
            GunTurret => TowerType.GunTurret,
            Railgun => TowerType.Railgun,
            Drone => TowerType.Drone,
            Crane => TowerType.Crane,
            Mortar => TowerType.Mortar,
            _ => throw new ArgumentOutOfRangeException(nameof(turretEntity), $"Entity {turretEntity.ToString()} is not a valid turret.")
        };
    }
}
