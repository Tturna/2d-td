using System;
using System.Collections.Generic;

namespace _2d_td;

public static class CurrencyManager
{
    private static Dictionary<BuildingSystem.TurretType, int> towerPriceMap = new()
    {
        { BuildingSystem.TurretType.GunTurret, 20 },
        { BuildingSystem.TurretType.Railgun, 25 }
    };

    private static Dictionary<string, int> towerUpgradePriceMap = new()
    {
        { GunTurret.Upgrade.DoubleGun.ToString(), 20 },
        { GunTurret.Upgrade.ImprovedBarrel.ToString(), 15 },
        { GunTurret.Upgrade.PhotonCannon.ToString(), 75 },
        { GunTurret.Upgrade.BotShot.ToString(), 50 },
        { GunTurret.Upgrade.RocketShots.ToString(), 70 },
    };

    public static int Balance { get; private set; }

    public static void Initialize()
    {
        Balance = 200;
    }

    public static int GetTowerPrice(BuildingSystem.TurretType towerType)
    {
        if (towerPriceMap.TryGetValue(towerType, out int price))
        {
            return price;
        }

        throw new ArgumentOutOfRangeException(nameof(towerType), $"Given type {towerType} does not have a price.");
    }

    public static int GetUpgradePrice(string upgradeName)
    {
        if (towerUpgradePriceMap.TryGetValue(upgradeName, out int price))
        {
            return price;
        }

        throw new ArgumentOutOfRangeException(nameof(upgradeName), $"{upgradeName} is not a valid upgrade.");
    }

    public static bool TryBuyTower(BuildingSystem.TurretType towerType)
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

    public static void SellTower(BuildingSystem.TurretType towerType)
    {
        Balance += (int)Math.Ceiling((double)GetTowerPrice(towerType) / 2.0);
    }

    public static bool TryBuyUpgrade(string upgradeName)
    {
        if (towerUpgradePriceMap.TryGetValue(upgradeName, out var price))
        {
            if (Balance < price)
            {
                return false;
            }

            Balance -= price;
            return true;
        }

        throw new ArgumentOutOfRangeException(nameof(upgradeName), $"{upgradeName} is not a valid upgrade.");
    }

    public static void AddBalance(int amount)
    {
        Balance += amount;
    }
}
