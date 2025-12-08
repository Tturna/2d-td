using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public static class ParticleSystem
{
    private record class ColorFade
    {
        public KeyValuePair<Color, float>[] ColorTimings;

        public ColorFade(KeyValuePair<Color, float>[] sortedColorTimings)
        {
            ColorTimings = sortedColorTimings;
        }

        public Color Evaluate(float v)
        {
            if (ColorTimings[ColorTimings.Length - 1].Value <= v)
            {
                return ColorTimings[ColorTimings.Length - 1].Key;
            }

            // TODO: implement binary search (using Array.BinarySearch()?) if this becomes
            // a bottleneck.
            for (int i = ColorTimings.Length - 2; i >= 0; i--)
            {
                var fromThreshold = ColorTimings[i].Value;

                if (v < fromThreshold) continue;

                var toThreshold = ColorTimings[i + 1].Value;
                // inverse lerp
                var relativeV = (v - fromThreshold) / (toThreshold - fromThreshold);

                return Color.Lerp(ColorTimings[i].Key, ColorTimings[i + 1].Key, relativeV);
            }

            return ColorTimings[0].Key;
        }
    }

    private record class Particle
    {
        public Texture2D? Sprite;
        public AnimationSystem? AnimationSystem;
        public Vector2 Position;
        public Vector2 Velocity;
        public float MaxLifetime;
        public float LifetimeLeft;
        public bool ShouldFadeOut;
        public bool ShouldFadeColor;
        public bool ShouldSlowDown;
        public bool HasGravity;
        public Color Color;
        public ColorFade? ColorFade;
        public float Depth;
        public float SlowdownSpeed;
        public float RotationSpeed;
        public float RotationRadians;

        public Particle(Vector2 position, Vector2 velocity, float lifetime, Color color,
            Texture2D? sprite = null, AnimationSystem.AnimationData? animation = null,
            bool shouldFadeOut = true, bool shouldSlowDown = false, bool hasGravity = false,
            float depth = 1f, float slowdownSpeed = 4f, float rotationSpeed = 0)
        {
            Sprite = sprite;
            Position = position;
            Velocity = velocity;
            MaxLifetime = lifetime;
            LifetimeLeft = lifetime;
            ShouldFadeOut = shouldFadeOut;
            ShouldSlowDown = shouldSlowDown;
            HasGravity = hasGravity;
            Color = color;
            Depth = depth;
            SlowdownSpeed = slowdownSpeed;
            RotationSpeed = rotationSpeed;

            if (animation is not null)
            {
                AnimationSystem = new AnimationSystem((AnimationSystem.AnimationData)animation);
            }
        }

        public Particle(Vector2 position, Vector2 velocity, float lifetime, ColorFade fade,
            Texture2D? sprite = null, AnimationSystem.AnimationData? animation = null,
            bool shouldFadeOut = true, bool shouldSlowDown = false, bool hasGravity = false,
            float depth = 1f, float slowdownSpeed = 4f, float rotationSpeed = 0f)
        {
            Sprite = sprite;
            Position = position;
            Velocity = velocity;
            MaxLifetime = lifetime;
            LifetimeLeft = lifetime;
            ShouldFadeOut = shouldFadeOut;
            ShouldSlowDown = shouldSlowDown;
            HasGravity = hasGravity;
            SlowdownSpeed = slowdownSpeed;
            Depth = depth;
            ColorFade = fade;
            ShouldFadeColor = true;
            RotationSpeed = rotationSpeed;

            if (animation is not null)
            {
                AnimationSystem = new AnimationSystem((AnimationSystem.AnimationData)animation);
            }
        }
    }

    private static Game1 game = null!;
    private static Texture2D pixelSprite = null!;
    private static Random rng = new();
    private static Particle?[] particles = null!;
    private static Stack<int> deathIndexStack = new();
    private static int nextMaxIndex;
    private const int MaxParticles = 10000;
    private static Texture2D[] botchunks =
    {
        AssetManager.GetTexture("botchunk1"),
        AssetManager.GetTexture("botchunk2"),
        AssetManager.GetTexture("botchunk3"),
        AssetManager.GetTexture("botchunk4"),
        AssetManager.GetTexture("botchunk5"),
        AssetManager.GetTexture("botchunk6")
    };

    public static void Initialize(Game1 game)
    {
        ParticleSystem.game = game;
        pixelSprite = TextureUtility.GetBlankTexture(game.SpriteBatch, 1, 1, Color.White);
        particles = new Particle[MaxParticles];
    }

    public static void FixedUpdate()
    {
        var deltaTime = Game1.FixedDeltaTime;

        for (int i = 0; i < MaxParticles; i++)
        {
            var particle = particles![i];

            if (particle is null) continue;

            particle.LifetimeLeft -= deltaTime;

            if (particle.LifetimeLeft <= 0)
            {
                particles[i] = null;
                deathIndexStack.Push(i);
                continue;
            }

            if (particle.HasGravity)
            {
                var gravityMagnitude = 0.125f;
                particle.Velocity += Vector2.UnitY * gravityMagnitude;
            }

            var slowdownThreshold = 0.8f;
            var normalLifetime = particle.LifetimeLeft / particle.MaxLifetime;
            var slowdownFactor = normalLifetime < slowdownThreshold ? deltaTime * particle.SlowdownSpeed : 0f;
            particle.Velocity = Vector2.Lerp(particle.Velocity, Vector2.Zero, slowdownFactor);
            particle.Position += particle.Velocity;
            particle.RotationRadians += particle.RotationSpeed;

            if (particle.AnimationSystem is not null)
            {
                particle.AnimationSystem.UpdateAnimation(deltaTime);
            }
        }
    }

    public static void DrawParticles(SpriteBatch spriteBatch, Matrix cameraTranslation)
    {
        spriteBatch.Begin(transformMatrix: cameraTranslation, sortMode: SpriteSortMode.BackToFront,
            samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default);

        for (int i = 0; i < MaxParticles; i++)
        {
            var particle = particles![i];

            if (particle is null) continue;

            var color = particle.Color;

            if (particle.ShouldFadeOut || particle.ShouldFadeColor)
            {
                var x = particle.LifetimeLeft / particle.MaxLifetime;

                if (particle.ShouldFadeColor)
                {
                    color = particle.ColorFade!.Evaluate(1f - x);
                }

                if (particle.ShouldFadeOut)
                {
                    var fadeoutThreshold = 0.5f;
                    var fadeoutX = x / (1f - fadeoutThreshold);
                    fadeoutX = 1 - MathF.Cos((fadeoutX * MathF.PI) / 2);
                    color = Color.Lerp(color, Color.Transparent, 1f - fadeoutX);
                }
            }

            var sprite = particle.Sprite ?? pixelSprite;

            if (particle.AnimationSystem is not null)
            {
                var frameSize = particle.AnimationSystem.BaseAnimationData.FrameSize;
                var drawOrigin = frameSize / 2;
                particle.AnimationSystem.Draw(spriteBatch, particle.Position, color, drawOrigin: drawOrigin,
                    drawLayerDepth: particle.Depth, rotationRadians: particle.RotationRadians);
            }
            else
            {
                var drawOrigin = new Vector2(sprite!.Width / 2, sprite.Height / 2);

                spriteBatch.Draw(sprite,
                    particle.Position,
                    sourceRectangle: null,
                    color,
                    rotation: particle.RotationRadians,
                    origin: drawOrigin,
                    scale: Vector2.One,
                    effects: SpriteEffects.None,
                    layerDepth: particle.Depth);
            }
        }

        spriteBatch.End();
    }

    private static void AddParticle(Particle particle)
    {
        if (deathIndexStack.Count > 0)
        {
            var newestAvailableIndex = deathIndexStack.Pop();
            particles[newestAvailableIndex] = particle;
            return;
        }

        particles[nextMaxIndex] = particle;
        nextMaxIndex++;
    }

    public static void PlayExplosion(Vector2 worldPosition, int size)
    {
        for (int i = 0; i < 15 + 5 * size; i++)
        {
            var randomAngleRadians = (float)rng.NextDouble() * MathHelper.Tau;
            var rx = MathF.Cos(randomAngleRadians);
            var ry = MathF.Sin(randomAngleRadians);
            var randomUnitVector = new Vector2(rx, ry);
            var velocity = randomUnitVector * ((float)rng.NextDouble() * 4f);

            var lifetime = 0.2f + 0.05f * size;
            AddParticle(new Particle(worldPosition, velocity, lifetime, Color.White,
                shouldSlowDown: true, slowdownSpeed: 4f + 3f / size));
        }

        for (int i = 0; i < 30 + 9 * size; i++)
        {
            var randomAngleRadians = (float)rng.NextDouble() * MathHelper.Tau;
            var rx = MathF.Cos(randomAngleRadians);
            var ry = MathF.Sin(randomAngleRadians);
            var randomUnitVector = new Vector2(rx, ry);
            var velocity = randomUnitVector * ((float)rng.NextDouble() * 2);

            var yellow = Color.FromNonPremultiplied(new Vector4(1f, 1f, 27f/255f, 1f));
            var orange = Color.FromNonPremultiplied(new Vector4(1f, 107f/255f, 5f/255f, 1f));
            var colorFadeSteps = new KeyValuePair<Color, float>[5]
            {
                KeyValuePair.Create(Color.White, 0f),
                KeyValuePair.Create(yellow, 0.2f),
                KeyValuePair.Create(orange, 0.6f),
                KeyValuePair.Create(Color.Gray, 0.85f),
                KeyValuePair.Create(Color.Transparent, 1f)
            };

            var fade = new ColorFade(colorFadeSteps);
            var lifetime = 0.4f + 0.09f * size;

            AddParticle(new Particle(worldPosition, velocity, lifetime, fade,
                shouldFadeOut: false, shouldSlowDown: true, depth: 0.8f, slowdownSpeed: 1f + 3f / size));
        }

        for (int i = 0; i < 50 + 12 * size; i++)
        {
            var randomAngleRadians = (float)rng.NextDouble() * MathHelper.Tau;
            var rx = MathF.Cos(randomAngleRadians);
            var ry = MathF.Sin(randomAngleRadians);
            var randomUnitVector = new Vector2(rx, ry);
            var velocity = randomUnitVector * ((float)rng.NextDouble() * 0.6f);
            var lifetime = 0.75f + 0.125f * size;

            AddParticle(new Particle(worldPosition, velocity, lifetime, Color.Gray,
                shouldSlowDown: true, depth: 0.9f, slowdownSpeed: 2f + 3f / (size * 2)));
        }
    }

    public static void PlayTowerPlacementEffect(Vector2 worldPosition)
    {
        for (int i = 0; i < 20; i++)
        {
            var x = ((float)rng.NextDouble() - 0.5f) * 2f;
            var y = ((float)rng.NextDouble() - 0.5f);
            var velocity = new Vector2(x, y) * 0.7f;

            AddParticle(new Particle(worldPosition, velocity, 0.7f, Color.White,
                shouldSlowDown: true, shouldFadeOut: true));
        }
    }

    public static void PlayTowerUpgradeEffect(Vector2 worldPosition)
    {
        var color = Color.FromNonPremultiplied(new Vector4(1f, 219f/255f, 163f/255f, 1f));

        for (int i = 0; i < 20; i++)
        {
            var x = ((float)rng.NextDouble() - 0.5f) * 2;
            var y = ((float)rng.NextDouble() - 0.5f) * 2;
            var velocity = new Vector2(x, y);
            var offset = velocity;
            offset.Normalize();
            offset = offset * Grid.TileLength + offset * 2;
            velocity *= 0.2f;

            AddParticle(new Particle(worldPosition + offset, velocity, 0.7f, color,
                shouldSlowDown: true, shouldFadeOut: true));
        }

    }

    public static void PlaySparkEffect(Vector2 worldPosition, Vector2 direction)
    {
        direction.Normalize();
        var perpendicular = new Vector2(direction.Y, -direction.X);

        for (int i = 0; i < rng.Next(3, 8); i++)
        {
            var outwardsMagnitude = (float)rng.NextDouble() * 0.5f + 0.5f;
            var sidewaysMagnitude = ((float)rng.NextDouble() - 0.5f) * 2;
            var velocity = direction * outwardsMagnitude + perpendicular * sidewaysMagnitude;
            var maxLifetime = 0.35f;
            var minLifetime = 0.1f;
            var lifetime = (float)rng.NextDouble() * (maxLifetime - minLifetime) + minLifetime;
            var yellow = Color.FromNonPremultiplied(new Vector4(1f, 1f, 27f/255f, 1f));

            AddParticle(new Particle(worldPosition, velocity, lifetime, yellow,
                shouldSlowDown: true, shouldFadeOut: true));
        }
    }

    public static void PlayFloater(Vector2 worldPosition, Color color, Vector2? momentumDirection = null)
    {
        var direction = ((float)rng.NextDouble() - 0.5f) * 2f;
        var velocity = Vector2.UnitY * direction * 0.15f;

        if (momentumDirection is not null)
        {
            velocity += (Vector2)momentumDirection * 0.2f;
        }

        AddParticle(new Particle(worldPosition, velocity, lifetime: 0.3f, color,
            shouldSlowDown: true, shouldFadeOut: true));
    }

    public static void PlayPhotonLaserImpact(Vector2 worldPosition)
    {
        var spriteSheet = AssetManager.GetTexture("laser_particle");
        var animation = new AnimationSystem.AnimationData(
            texture: spriteSheet,
            frameCount: 4,
            frameSize: new Vector2(spriteSheet.Width / 4, spriteSheet.Height),
            delaySeconds: 0.1f);

        var rx = ((float)rng.NextDouble() - 0.5f) * 2;
        var ry = ((float)rng.NextDouble() - 0.5f) * 2;
        var randomDirection = new Vector2(rx, ry);
        randomDirection.Normalize();
        var velocity = randomDirection * 0.5f;
        var lifetime = animation.FrameCount * animation.DelaySeconds;

        AddParticle(new Particle(worldPosition, velocity, lifetime, Color.White,
            animation: animation, shouldSlowDown: true, shouldFadeOut: true));
    }

    public static void PlayBotchunkExplosion(Vector2 worldPosition)
    {
        for (int i = 0; i < rng.Next(3, 8); i++)
        {
            // Set max angle degrees to 270 (3 * pi / 2 radians).
            var maxSectorRadians = MathHelper.PiOver2 * 3;
            // Pick random angle from between 0 to 270 degrees.
            var randomAngleRadians = (float)rng.NextDouble() * maxSectorRadians;
            // Rotate random angle sector so that it never picks angles facing down
            // (45 degree dead zone downwards).
            var directedRadians = randomAngleRadians + MathHelper.PiOver4 * 3;
            var rx = MathF.Cos(directedRadians);
            var ry = MathF.Sin(directedRadians);
            var randomUnitVector = new Vector2(rx, ry);

            var upwardsDot = Vector2.Dot(randomUnitVector, -Vector2.UnitY);
            var upwardsVelocityOffsetMagnitude = MathHelper.Max(upwardsDot, -0.2f) + 0.2f;
            var randomMagnitude = ((float)rng.NextDouble() * 1.5f) + 0.5f; // 0.5 to 2.0

            var velocity = randomUnitVector * (randomMagnitude * upwardsVelocityOffsetMagnitude);
            var maxLifetime = 0.4f;
            var minLifetime = 0.2f;
            var lifetime = (float)rng.NextDouble() * (maxLifetime - minLifetime) + minLifetime;
            var sprite = botchunks[rng.Next(0, botchunks.Length)];
            var rotationSpeed = ((float)rng.NextDouble() - 0.5f) * 2;

            AddParticle(new Particle(worldPosition, velocity, lifetime, Color.White,
                shouldSlowDown: true, shouldFadeOut: true, sprite: sprite, hasGravity: true,
                rotationSpeed: rotationSpeed));
        }
    }

    public static void PlayShotSmokeEffect(Vector2 worldPosition)
    {
        for (int i = 0; i < rng.Next(3, 8); i++)
        {
            var randomAngleRadians = (float)rng.NextDouble() * MathHelper.Tau;
            var rx = MathF.Cos(randomAngleRadians);
            var ry = MathF.Sin(randomAngleRadians);
            var randomUnitVector = new Vector2(rx, ry);
            var velocity = randomUnitVector;
            var maxLifetime = 0.4f;
            var minLifetime = 0.2f;
            var lifetime = (float)rng.NextDouble() * (maxLifetime - minLifetime) + minLifetime;
            var rotationSpeed = ((float)rng.NextDouble() - 0.5f);

            var smokeSprite = AssetManager.GetTexture("smoke");
            var animation = new AnimationSystem.AnimationData(
                texture: smokeSprite,
                frameCount: 5,
                frameSize: new Vector2(smokeSprite.Width / 5, smokeSprite.Height),
                delaySeconds: lifetime / 5);

            AddParticle(new Particle(worldPosition, velocity, lifetime, Color.White,
                shouldSlowDown: true, shouldFadeOut: true, animation: animation,
                rotationSpeed: rotationSpeed));
        }
    }
}
