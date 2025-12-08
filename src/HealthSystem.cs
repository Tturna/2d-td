using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class HealthSystem
{
    public Entity Owner;
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    public delegate void DiedHandler(Entity diedEntity);
    public event DiedHandler Died;

    public delegate void DamagedHandler(Entity damagedEntity, int amount);
    public event DamagedHandler Damaged;

    private Texture2D pixelSprite;
    private Color backgroundBarColor = Color.FromNonPremultiplied(new Vector4(48f/255f, 8f/255f, 35f/255, 1f));
    private Color foregroundBarColor = Color.FromNonPremultiplied(new Vector4(249f/255f, 72f/255f, 88f/255, 1f));
    private const int healthBarWidth = 8;
    private float healthBarFlashTime = 0.075f;
    private float healthBarFlashTimer;

    public HealthSystem(Entity owner, int initialHealth) : this(owner, initialHealth, initialHealth) { }

    public HealthSystem(Entity owner, int initialHealth, int maxHealth)
    {
        Owner = owner;
        MaxHealth = maxHealth;
        CurrentHealth = initialHealth;

        pixelSprite = TextureUtility.GetBlankTexture(Game1.Instance.SpriteBatch, 1, 1, Color.White);
    }

    public void UpdateHealthBarGraphics(float deltaTime)
    {
        if (healthBarFlashTimer > 0)
        {
            healthBarFlashTimer -= deltaTime;
        }
    }

    public void DrawHealthBar(Vector2 worldPosition)
    {
        if (CurrentHealth >= MaxHealth) return;

        // background bar
        Game1.Instance.SpriteBatch.Draw(pixelSprite,
            position: worldPosition - Vector2.UnitX * (healthBarWidth / 2),
            sourceRectangle: null,
            color: backgroundBarColor,
            rotation: 0,
            origin: Vector2.Zero,
            scale: new Vector2(healthBarWidth, 1),
            effects: SpriteEffects.None,
            layerDepth: 0.7f);

        var fgColor = healthBarFlashTimer > 0 ? Color.White : foregroundBarColor;

        // actual health bar
        Game1.Instance.SpriteBatch.Draw(pixelSprite,
            position: worldPosition - Vector2.UnitX * (healthBarWidth / 2),
            sourceRectangle: null,
            color: fgColor,
            rotation: 0,
            origin: Vector2.Zero,
            scale: new Vector2((float)CurrentHealth / (float)MaxHealth * (float)healthBarWidth, 1),
            effects: SpriteEffects.None,
            layerDepth: 0.6f);
    }

    public void TakeDamage(int amount)
    {
        if (CurrentHealth == 0) return;

        CurrentHealth -= amount;
        OnDamaged(Owner, amount);
        healthBarFlashTimer = healthBarFlashTime;

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

    private void OnDamaged(Entity damagedEntity, int amount)
    {
        Damaged?.Invoke(damagedEntity, amount);
    }
}
