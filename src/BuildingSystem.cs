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
        Mortar,
        Hovership,
        PunchTrap
    }

    private static Game1 game;
    private static TimeSpan lastGameTime;
    private static TowerType selectedTowerType;
    private static TimeSpan allowedTowerPlacementTime;
    private static Func<Vector2, bool> canPlaceTowerCallback;
    private static Func<Game, Vector2, Entity> createTowerInstanceCallback;

    public static bool CanPlaceTower { get; private set; }
    public static bool IsPlacingTower { get; private set; }

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

        var towerAllowsPlacement = true;

        if (canPlaceTowerCallback is not null)
        {
            towerAllowsPlacement = canPlaceTowerCallback(gridMousePosition);
        }

        CanPlaceTower = !isColliding &&
            towerAllowsPlacement &&
            gameTime.TotalGameTime > allowedTowerPlacementTime &&
            selectedTowerType != TowerType.None;

        if (InputSystem.IsLeftMouseButtonClicked() && CanPlaceTower)
        {
            TrySpawnTower(gridMousePosition);
        }
    }

    private static bool TrySpawnTower(Vector2 position)
    {
        if (!CurrencyManager.TryBuyTower(selectedTowerType)) return false;

        var spawnedTower = createTowerInstanceCallback(game, position);

        var costText = $"-{CurrencyManager.GetTowerPrice(selectedTowerType)}";
        var costTextPosition = spawnedTower.Position - Vector2.UnitY * 6;
        var textVelocity = -Vector2.UnitY * 25f;
        UIComponent.SpawnFlyoutText(costText, costTextPosition, textVelocity, lifetime: 1f);

        var particlesPosition = spawnedTower.Position + spawnedTower.Size / 2 +
            Vector2.UnitY * spawnedTower.Size.Y / 2;
        ParticleSystem.PlayTowerPlacementEffect(particlesPosition);

        return true;
    }

    public static void SelectTower<T>() where T : ITower
    {
        allowedTowerPlacementTime = lastGameTime.Add(TimeSpan.FromMilliseconds(200));
        selectedTowerType = T.GetTowerType();
        canPlaceTowerCallback = T.CanPlaceTower;
        createTowerInstanceCallback = T.CreateNewInstance;
        IsPlacingTower = true;
    }

    public static void DeselectTower()
    {
        selectedTowerType = TowerType.None;
        canPlaceTowerCallback = null;
        createTowerInstanceCallback = null;
        IsPlacingTower = false;
    }

    // TODO: Consider taking this out and adding the ability to get the type of a tower
    // in ITower.
    public static TowerType GetTowerTypeFromEntity(Entity towerEntity)
    {
        return towerEntity switch
        {
            GunTurret => TowerType.GunTurret,
            Railgun => TowerType.Railgun,
            Drone => TowerType.Drone,
            Crane => TowerType.Crane,
            Mortar => TowerType.Mortar,
            Hovership => TowerType.Hovership,
            PunchTrap => TowerType.PunchTrap,
            _ => throw new ArgumentOutOfRangeException(nameof(towerEntity), $"Entity {towerEntity.ToString()} is not a valid turret.")
        };
    }
}
