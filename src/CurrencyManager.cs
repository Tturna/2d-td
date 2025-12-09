using System;
using System.Collections.Generic;

namespace _2d_td;

public static class CurrencyManager
{
    public delegate void CurrencyAddedHandler(int amount);
    public static event CurrencyAddedHandler CurrencyAdded;

    private static Dictionary<BuildingSystem.TowerType, int> towerPriceMap = new()
    {
        { BuildingSystem.TowerType.GunTurret, 20 },
        { BuildingSystem.TowerType.Railgun, 25 },
        { BuildingSystem.TowerType.Drone, 20 },
        { BuildingSystem.TowerType.Crane, 30 },
        { BuildingSystem.TowerType.Mortar, 30 },
        { BuildingSystem.TowerType.Hovership, 50 },
        { BuildingSystem.TowerType.PunchTrap, 15 }
    };

    public static int Balance { get; private set; }

    public static void Initialize()
    {
        Balance = 200;
    }

    public static int GetTowerPrice(BuildingSystem.TowerType towerType)
    {
        if (towerPriceMap.TryGetValue(towerType, out int price))
        {
            return price;
        }

        throw new ArgumentOutOfRangeException(nameof(towerType), $"Given type {towerType} does not have a price.");
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

    public static int SellTower(BuildingSystem.TowerType towerType)
    {
        var returnScrap = (int)Math.Ceiling((double)GetTowerPrice(towerType) / 2.0);
        AddBalance(returnScrap);

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
    }
}
