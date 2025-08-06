using System;

namespace _2d_td;

public class HealthSystem
{
    public Entity Owner;
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    public delegate void DiedHandler(Entity diedEntity);
    public event DiedHandler Died;

    public HealthSystem(Entity owner, int initialHealth)
    {
        Owner = owner;
        MaxHealth = initialHealth;
        CurrentHealth = initialHealth;
    }

    public HealthSystem(Entity owner, int initialHealth, int maxHealth)
    {
        Owner = owner;
        MaxHealth = maxHealth;
        CurrentHealth = initialHealth;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;

        Console.WriteLine($"{Owner.ToString()} took {amount} damage!");

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;

            OnDied(Owner);
        }
    }

    private void OnDied(Entity diedEntity)
    {
        Died?.Invoke(diedEntity);
    }
}
