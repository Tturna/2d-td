namespace _2d_td;

public class Player
{
    HealthSystem healthSystem;
    private int health = 100;

    public Player()
    {
        HealthSystem = new HealthSystem(owner: this, initialHealth: health);
    }
}