using System;
using System.Collections.Generic;

namespace _2d_td;

public static class CurrencyManager
{
    private static Dictionary<BuildingSystem.TowerType, int> towerPriceMap = new()
    {
        { BuildingSystem.TowerType.GunTurret, 20 },
        { BuildingSystem.TowerType.Railgun, 25 }
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

        if (Balance < price)
        {
            return false;
        }

        // TODO: Track bought towers?
        Balance -= price;
        return true;
    }

    public static void SellTower(BuildingSystem.TowerType towerType)
    {
        Balance += (int)Math.Ceiling((double)GetTowerPrice(towerType) / 2.0);
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
    }
}
