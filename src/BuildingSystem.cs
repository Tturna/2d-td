using System;
using System.Collections.Generic;
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
    public static List<Entity> Towers { get; private set; } = new();

    public static void Initialize(Game game)
    {
        BuildingSystem.game = (Game1)game;
        Towers = new();
    }

    public static void Update(GameTime gameTime)
    {
        lastGameTime = gameTime.TotalGameTime;
        if (game is null) return;
        if (selectedTowerType == TowerType.None) return;

        var gridMousePosition = Grid.SnapPositionToGrid(InputSystem.GetMouseWorldPosition());

        var towerAllowsPlacement = true;

        if (canPlaceTowerCallback is not null)
        {
            towerAllowsPlacement = canPlaceTowerCallback(gridMousePosition);
        }

        CanPlaceTower = towerAllowsPlacement &&
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
        Towers.Add(spawnedTower);

        var costText = $"-{CurrencyManager.GetTowerPrice(selectedTowerType)}";
        var costTextPosition = spawnedTower.Position - Vector2.UnitY * 6;
        var textVelocity = -Vector2.UnitY * 25f;
        UIComponent.SpawnFlyoutText(costText, costTextPosition, textVelocity, lifetime: 1f,
            color: Color.White);

        var particlesPosition = spawnedTower.Position + spawnedTower.Size / 2 +
            Vector2.UnitY * spawnedTower.Size.Y / 2;
        ParticleSystem.PlayTowerPlacementEffect(particlesPosition);
        SoundSystem.PlaySound("placeDown");

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
