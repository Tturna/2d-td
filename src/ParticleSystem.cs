using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

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
        public Vector2 Position;
        public Vector2 Velocity;
        public float MaxLifetime;
        public float LifetimeLeft;
        public bool ShouldFadeOut;
        public bool ShouldFadeColor;
        public bool ShouldSlowDown;
        public Color Color;
        public ColorFade ColorFade;
        public float Depth;
        public float SlowdownSpeed;

        public Particle(Vector2 position, Vector2 velocity, float lifetime, Color color,
            bool shouldFadeOut = true, bool shouldSlowDown = false, float depth = 1f,
            float slowdownSpeed = 4f)
        {
            Position = position;
            Velocity = velocity;
            MaxLifetime = lifetime;
            LifetimeLeft = lifetime;
            ShouldFadeOut = shouldFadeOut;
            ShouldSlowDown = shouldSlowDown;
            Color = color;
            Depth = depth;
            SlowdownSpeed = slowdownSpeed;
        }

        public Particle(Vector2 position, Vector2 velocity, float lifetime, ColorFade fade,
            bool shouldFadeOut = true, bool shouldSlowDown = false, float depth = 1f,
            float slowdownSpeed = 4f)
        {
            Position = position;
            Velocity = velocity;
            MaxLifetime = lifetime;
            LifetimeLeft = lifetime;
            ShouldFadeOut = shouldFadeOut;
            ShouldSlowDown = shouldSlowDown;
            SlowdownSpeed = slowdownSpeed;
            Depth = depth;
            ColorFade = fade;
            ShouldFadeColor = true;
        }
    }

    private static Game1 game;
    private static Texture2D pixelSprite;
    private static Random rng;
    private static Particle[] particles;
    private static Stack<int> deathIndexStack = new();
    private static int nextMaxIndex;
    private const int MaxParticles = 10000;

    public static void Initialize(Game1 game)
    {
        ParticleSystem.game = game;
        pixelSprite = TextureUtility.GetBlankTexture(game.SpriteBatch, 1, 1, Color.White);
        rng = new();
        particles = new Particle[MaxParticles];
    }

    public static void FixedUpdate()
    {
        var deltaTime = Game1.FixedDeltaTime;

        for (int i = 0; i < MaxParticles; i++)
        {
            var particle = particles[i];

            if (particle is null) continue;

            particle.LifetimeLeft -= deltaTime;

            if (particle.LifetimeLeft <= 0)
            {
                particles[i] = null;
                deathIndexStack.Push(i);
                continue;
            }

            var slowdownThreshold = 0.8f;
            var normalLifetime = particle.LifetimeLeft / particle.MaxLifetime;
            var slowdownFactor = normalLifetime < slowdownThreshold ? deltaTime * particle.SlowdownSpeed : 0f;
            particle.Velocity = Vector2.Lerp(particle.Velocity, Vector2.Zero, slowdownFactor);
            particle.Position += particle.Velocity;
        }
    }

    public static void DrawParticles(SpriteBatch spriteBatch, Matrix cameraTranslation)
    {
        spriteBatch.Begin(transformMatrix: cameraTranslation, sortMode: SpriteSortMode.BackToFront,
            samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default);

        for (int i = 0; i < MaxParticles; i++)
        {
            var particle = particles[i];

            if (particle is null) continue;

            var color = particle.Color;

            if (particle.ShouldFadeOut || particle.ShouldFadeColor)
            {
                var x = particle.LifetimeLeft / particle.MaxLifetime;

                if (particle.ShouldFadeColor)
                {
                    color = particle.ColorFade.Evaluate(1f - x);
                }

                if (particle.ShouldFadeOut)
                {
                    var fadeoutThreshold = 0.5f;
                    var fadeoutX = x / (1f - fadeoutThreshold);
                    fadeoutX = 1 - MathF.Cos((fadeoutX * MathF.PI) / 2);
                    color = Color.Lerp(color, Color.Transparent, 1f - fadeoutX);
                }
            }

            spriteBatch.Draw(pixelSprite,
                particle.Position,
                sourceRectangle: null,
                color,
                rotation: 0,
                origin: Vector2.Zero,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: particle.Depth);
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
}
