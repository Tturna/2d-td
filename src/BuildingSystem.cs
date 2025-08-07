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
    private static TurretType selectedTurretType;

    public static bool CanPlaceTurret { get; private set; }

    public static void Initialize(Game game)
    {
        BuildingSystem.game = (Game1)game;
    }

    public static void Update()
    {
        var gridMousePosition = Grid.SnapPositionToGrid(InputSystem.GetMousePosition());

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

        CanPlaceTurret = !isColliding;

        if (CanPlaceTurret && InputSystem.IsLeftMouseButtonClicked() && selectedTurretType != TurretType.None)
        {
            var turret = SpawnTurret(selectedTurretType, gridMousePosition);
            game.Components.Add(turret);
        }
    }

    private static Entity SpawnTurret(TurretType turretType, Vector2 position)
    {
        return turretType switch {
            TurretType.GunTurret => new GunTurret(game, position),
            TurretType.Railgun => new Railgun(game, position),
            _ => throw new ArgumentOutOfRangeException(nameof(selectedTurretType), $"Unhandled entity type: {selectedTurretType}")
        };
    }

    public static Texture2D SelectTurret(TurretType turretType)
    {
        selectedTurretType = turretType;

        return turretType switch {
            TurretType.GunTurret => AssetManager.GetTexture("turret"),
            TurretType.Railgun => AssetManager.GetTexture("turretTwo"),
            _ => null
        };
    }
}
