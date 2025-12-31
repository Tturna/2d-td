using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class CurrencyManager
{
    public delegate void CurrencyAddedHandler(int amount);
    public static event CurrencyAddedHandler CurrencyAdded;

    private static Dictionary<BuildingSystem.TowerType, int> towerPriceMap = new()
    {
        { BuildingSystem.TowerType.GunTurret, 20 },
        { BuildingSystem.TowerType.Railgun, 25 },
        { BuildingSystem.TowerType.Drone, 25 },
        { BuildingSystem.TowerType.Crane, 15 },
        { BuildingSystem.TowerType.Mortar, 30 },
        { BuildingSystem.TowerType.Hovership, 25 },
        { BuildingSystem.TowerType.PunchTrap, 5 }
    };

    private static Dictionary<Tileset, int> tilePriceMap;

    public static int Balance { get; private set; }

    public static void Initialize()
    {
        Balance = 80;

        tilePriceMap = new()
        {
            { Game1.Instance.Terrain.GetPlayerLightTileset(), 1 },
            { Game1.Instance.Terrain.GetPlayerHeavyTileset(), 5 }
        };
        WaveSystem.WaveEnded += () => 
        {
            // TODO: Give money here instead of WaveSystem, right now it doesnt work because it doesnt get reset when leaving the level
        };
    }

    public static int GetTowerPrice(BuildingSystem.TowerType towerType)
    {
        if (towerPriceMap.TryGetValue(towerType, out int price))
        {
            return price;
        }

        throw new ArgumentOutOfRangeException(nameof(towerType), $"Given type {towerType} does not have a price.");
    }
    
    public static bool TryBuyTile(Tileset tileset)
    {
        var price = tilePriceMap[tileset];
        if (Balance < price) return false;

        Balance -= price;
        return true;
    }
    public static bool TryBuyTower(BuildingSystem.TowerType towerType)
    {
        var price = GetTowerPrice(towerType);

        if (Balance < price) return false;

        // TODO: Track bought towers?
        Balance -= price;
        return true;
    }

    public static bool TryRepairTower(BuildingSystem.TowerType towerType)
    {
        var price = GetTowerPrice(towerType) / 2;

        if (Balance < price) return false;

        Balance -= price;
        return true;
    }

    public static int SellTower(int towerValue, bool isBroken)
    {
        var priceDivisor = isBroken ? 4 : 2;
        var returnScrap = (int)Math.Ceiling((double)towerValue / priceDivisor);
        AddBalance(returnScrap);
        SoundSystem.PlaySound("sell");

        return returnScrap;
    }

    public static bool TryBuyUpgrade(int price)
    {
        if (Balance < price)
        {
            return false;
        }

        Balance -= price;
        return true;
    }

    public static void AddBalance(int amount)
    {
        Balance += amount;
        CurrencyAdded?.Invoke(amount);
        SoundSystem.PlaySound("coin");
    }
}
