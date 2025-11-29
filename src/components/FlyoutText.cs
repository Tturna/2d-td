using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class FlyoutText : UIEntity
{
    private float lifetimeLeft;
    private float originalLifetime;
    private bool shouldSlowdown;
    private Vector2 velocity;

    public FlyoutText(Game1 game, List<UIEntity> uiElements, string text, Vector2 startPosition,
        Vector2 flyoutVelocity, float lifetime, bool slowdown = true) : base(game, uiElements,
        AssetManager.GetFont("pixelsix"), text)
    {
        lifetimeLeft = lifetime;
        originalLifetime = lifetime;
        shouldSlowdown = slowdown;
        velocity = flyoutVelocity;
        SetPosition(Camera.WorldToScreenPosition(startPosition));
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

        UpdatePosition(velocity * deltaTime);

        if (shouldSlowdown)
        {
            velocity = Vector2.Lerp(velocity, Vector2.Zero, deltaTime);
        }

        var normalLifetime = lifetimeLeft / originalLifetime;
        TextColor = Color.FromNonPremultiplied(new Vector4(1f, 1f, 1f, normalLifetime));

        base.Update(gameTime);
    }
}
