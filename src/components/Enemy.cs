using Microsoft.Xna.Framework;

namespace _2d_td;

public class Enemy : Entity
{
    public HealthSystem HealthSystem;

    public Enemy(Game game, Vector2 position) : base(game, position, AssetManager.GetTexture("enemy"))
    {
        HealthSystem = new HealthSystem(this, 100);
        HealthSystem.Died += OnDeath;
    }

    private void OnDeath(Entity diedEntity)
    {
        Game.Components.Remove(this);
        Game.Enemies.Remove(this);
    }
}
