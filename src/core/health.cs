namespace TDgame;
// this uses Depedency Injection design pattern
public interface IHealthManager
{
    void ChangeHealth(int delta);
    void SetHealth(int amount);
}

public class BasicHealthManager : IHealthManager
{
    public int Health { get; private set; }

    public BasicHealthManager(int originalHealth = 100)
    {
        Health = originalHealth;
    }

    public void ChangeHealth(int delta)
    {
        Health -= delta;
    }

    public void SetHealth(int amount)
    {
        Health = amount;
    }
}