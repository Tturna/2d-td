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

    public delegate void DamagedHandler(Entity source, Entity damagedEntity, int amount);
    public event DamagedHandler Damaged;

    public delegate void HealedHandler(Entity healedEntity, int amount);
    public event HealedHandler Healed;

    private Texture2D pixelSprite;
    private Color normalBackgroundBarColor = Color.FromNonPremultiplied(new Vector4(48f/255f, 8f/255f, 35f/255, 1f));
    private Color normalForegroundBarColor = Color.FromNonPremultiplied(new Vector4(249f/255f, 72f/255f, 88f/255, 1f));
    private Color repairBackgroundBarColor = Color.FromNonPremultiplied(new Vector4(22f/255f, 35f/255f, 31f/255f, 1f));
    private Color repairForegroundBarColor = Color.FromNonPremultiplied(new Vector4(162f/255f, 1f, 63f/255f, 1f));
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

        var bgColor = CurrentHealth <= 0 ? repairBackgroundBarColor : normalBackgroundBarColor;
        var fgColor = CurrentHealth <= 0 ? repairForegroundBarColor : normalForegroundBarColor;

        float fillAmount = 0f;

        if (CurrentHealth > 0)
        {
            fillAmount = (float)CurrentHealth / (float)MaxHealth;
        }
        else
        {
            // Assume tower repair requires healing half of max health.
            fillAmount = 1f + ((float)CurrentHealth / ((float)MaxHealth / 2));
        }

        var foregroundBarWidth = fillAmount * (float)healthBarWidth;

        // background bar
        Game1.Instance.SpriteBatch.Draw(pixelSprite,
            position: worldPosition - Vector2.UnitX * (healthBarWidth / 2),
            sourceRectangle: null,
            color: bgColor,
            rotation: 0,
            origin: Vector2.Zero,
            scale: new Vector2(healthBarWidth, 1),
            effects: SpriteEffects.None,
            layerDepth: 0.4f);

        fgColor = healthBarFlashTimer > 0 ? Color.White : fgColor;

        // actual health bar
        Game1.Instance.SpriteBatch.Draw(pixelSprite,
            position: worldPosition - Vector2.UnitX * (healthBarWidth / 2),
            sourceRectangle: null,
            color: fgColor,
            rotation: 0,
            origin: Vector2.Zero,
            scale: new Vector2(foregroundBarWidth, 1),
            effects: SpriteEffects.None,
            layerDepth: 0.3f);
    }

    public void TakeDamage(Entity source, int amount)
    {
        if (CurrentHealth == 0) return;

        CurrentHealth -= amount;
        OnDamaged(source, Owner, amount);
        healthBarFlashTimer = healthBarFlashTime;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;

            OnDied(Owner);
        }
    }

    public void Heal(int amount)
    {
        if (CurrentHealth >= MaxHealth) return;

        CurrentHealth += amount;
        OnHealed(Owner, amount);
        healthBarFlashTimer = healthBarFlashTime;

        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }

    public bool SetHealth(int health, bool force = false)
    {
        if (force)
        {
            CurrentHealth = health;
            return true;
        }

        var newHealth = MathHelper.Clamp(health, 0, MaxHealth);
        CurrentHealth = newHealth;
        return newHealth == health;
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }

    public void SetMaxHealth(int newMax)
    {
        var diff = newMax - MaxHealth;
        MaxHealth = newMax;
        CurrentHealth += diff;
    }

    public void SetHealthBarBackgroundColor(Color color)
    {
        normalBackgroundBarColor = color;
    }

    public void SetHealthBarForegroundColor(Color color)
    {
        normalForegroundBarColor = color;
    }

    private void OnDied(Entity diedEntity)
    {
        Died?.Invoke(diedEntity);
    }

    private void OnDamaged(Entity source, Entity damagedEntity, int amount)
    {
        Damaged?.Invoke(source, damagedEntity, amount);
    }

    private void OnHealed(Entity healedEntity, int amount)
    {
        Healed?.Invoke(healedEntity, amount);
    }
}
