using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public static class BuildingSystem
{
    public enum TurretType
    {
        None,
        GunTurret,
        Railgun
    }

    private static Game1 game;
    private static TimeSpan lastGameTime;
    private static TurretType selectedTurretType;
    private static TimeSpan allowedTurretPlacementTime;

    public static bool CanPlaceTurret { get; private set; }

    public static void Initialize(Game game)
    {
        BuildingSystem.game = (Game1)game;
    }

    public static void Update(GameTime gameTime)
    {
        lastGameTime = gameTime.TotalGameTime;
        if (game is null) return;

        var gridMousePosition = Grid.SnapPositionToGrid(InputSystem.GetMouseWorldPosition());

        // TODO: Make a system that doesn't require collision checks against every entity.
        // This could be done by connecting tiles or tile coordinates to entities and checking
        // if the tile under the mouse has a connected entity.
        var isColliding = false;

        foreach (var component in game.Components)
        {
            if (component is not Entity) continue;

            var entity = (Entity)component;

            if (entity.Position == gridMousePosition)
            {
                isColliding = true;
                break;
            }
        }

        CanPlaceTurret = !isColliding &&
            gameTime.TotalGameTime > allowedTurretPlacementTime &&
            selectedTurretType != TurretType.None;

        if (InputSystem.IsLeftMouseButtonClicked() &&
            CanPlaceTurret &&
            TrySpawnTurret(selectedTurretType, gridMousePosition, out var turret))
        {
            game.Components.Add(turret);
        }
    }

    private static bool TrySpawnTurret(TurretType turretType, Vector2 position, out Entity spawnedTurret)
    {
        spawnedTurret = null;
        if (!CurrencyManager.TryBuyTower(turretType)) return false;

        spawnedTurret = turretType switch {
            TurretType.GunTurret => new GunTurret(game),
            TurretType.Railgun => new Railgun(game),
            _ => throw new ArgumentOutOfRangeException(nameof(selectedTurretType), $"Unhandled entity type: {selectedTurretType}")
        };

        spawnedTurret.Position = position;
        return true;
    }

    public static Texture2D SelectTurret(TurretType turretType)
    {
        selectedTurretType = turretType;
        allowedTurretPlacementTime = lastGameTime.Add(TimeSpan.FromMilliseconds(200));

        return turretType switch {
            TurretType.GunTurret => AssetManager.GetTexture("gunTurretBase"),
            TurretType.Railgun => AssetManager.GetTexture("turretTwo"),
            _ => null
        };
    }

    public static TurretType GetTurretTypeFromEntity(Entity turretEntity)
    {
        return turretEntity switch
        {
            GunTurret => TurretType.GunTurret,
            Railgun => TurretType.Railgun,
            _ => throw new ArgumentOutOfRangeException(nameof(turretEntity), $"Entity {turretEntity.ToString()} is not a valid turret.")
        };
    }
}
