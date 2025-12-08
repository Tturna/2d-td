using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class FlyoutText : UIEntity
{
    private float lifetimeLeft;
    private float originalLifetime;
    private bool shouldSlowdown;
    private Vector2 velocity;
    private Vector2 targetWorldPosition;
    private Vector2 addedPosition = Vector2.Zero;
    private Color color;

    public FlyoutText(Game1 game, List<UIEntity> uiElements, string text, Vector2 startWorldPosition,
        Vector2 flyoutVelocity, float lifetime, Color color, bool slowdown = true) : base(game, uiElements,
        AssetManager.GetFont("pixelsix"), text)
    {
        lifetimeLeft = lifetime;
        originalLifetime = lifetime;
        shouldSlowdown = slowdown;
        velocity = flyoutVelocity;
        targetWorldPosition = startWorldPosition;
        this.color = color;
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        lifetimeLeft -= deltaTime;

        if (lifetimeLeft <= 0)
        {
            Destroy();
            return;
        }

        addedPosition += velocity * deltaTime;
        SetPosition(Camera.WorldToScreenPosition(targetWorldPosition + addedPosition));

        if (shouldSlowdown)
        {
            velocity = Vector2.Lerp(velocity, Vector2.Zero, deltaTime);
        }

        var normalLifetime = lifetimeLeft / originalLifetime;
        var reverseLifetime = 1f - normalLifetime;
        TextColor = Color.Lerp(color, Color.Transparent, reverseLifetime);

        base.Update(gameTime);
    }
}
