using System;

namespace _2d_td;

public class HealthSystem
{
    public Entity Owner;
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    public class DiedEventArgs(Entity diedEntity) : EventArgs { }
    public event EventHandler<DiedEventArgs> Died;

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

            var diedEventArgs = new DiedEventArgs(Owner);
            OnDied(diedEventArgs);
        }
    }

    private void OnDied(DiedEventArgs args)
    {
        Died?.Invoke(this, args);
    }
}
