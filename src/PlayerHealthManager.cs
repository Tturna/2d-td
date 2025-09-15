namespace _2d_td;

public class PlayerHealthManager
{
    private int health = 100;

    public PlayerHealthManager(int health)
    {
        this.health = health;
    }

    public takeDamage(int amount)
    {
        if (health > 0)
        {
            health -= amount;
        }
        else
        {
            //call a fail lvl function, prob in Game1
        }
    }
}