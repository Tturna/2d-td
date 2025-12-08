using Microsoft.Xna.Framework;

namespace _2d_td;

public class Explosion : Entity
{
    private float lifetimeLeft;

    public Explosion(Game1 game, Vector2 worldPosition) : base(game, worldPosition, GetExplosionAnimation())
    {
        lifetimeLeft = AnimationSystem.BaseAnimationData.DelaySeconds * AnimationSystem.BaseAnimationData.FrameCount;
        DrawOrigin = AnimationSystem.BaseAnimationData.FrameSize / 2;
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

        base.Update(gameTime);
    }

    private static AnimationSystem.AnimationData GetExplosionAnimation()
    {
        var sprite = AssetManager.GetTexture("explosion_small");

        var animation = new AnimationSystem.AnimationData(
            texture: sprite,
            frameCount: 4,
            frameSize: new Vector2(sprite.Width / 4, sprite.Height),
            delaySeconds: 0.075f);

        return animation;
    }
}
