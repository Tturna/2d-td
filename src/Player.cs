/*namespace _2d_td;

public class Player : Entity
{
    HealthSystem healthSystem;
    private int startingHealth = 100;

    public Player()
    {
        healthSystem = new HealthSystem(owner: this, initialHealth: startingHealth);

        healthSystem.Died += (Entity player) =>
        {
            Console.WriteLine("Player lost");
        };
    }
}*/