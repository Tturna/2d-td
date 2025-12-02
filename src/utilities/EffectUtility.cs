using Microsoft.Xna.Framework;

namespace _2d_td;

public static class EffectUtility
{
    /// <summary>
    /// Apply explosion effect. Knocks back and damages enemies and corpses in range.
    /// Pass useEffectFalloff = false to prevent damage and knockback from being reduced for
    /// entities further from the explosion.
    /// </summary>
    public static void Explode(Vector2 worldPosition, float radius, float magnitude, int damage,
        bool useEffectFalloff = true)
    {
        var enemies = EnemySystem.EnemyBins.GetValuesFromBinsInRange(worldPosition, radius).ToArray();

        foreach (var enemy in enemies)
        {
            var diff = enemy.Position + enemy.Size / 2 - worldPosition;
            var distance = diff.Length();

            if (distance > radius) continue;

            var blastDirection = diff;
            blastDirection.Normalize();

            var effectStrength = 1f;

            if (useEffectFalloff)
            {
                // 0 at center, 1 at radius edge
                var rawMagnitude = distance / radius;
                effectStrength = MathHelper.Max(1f - rawMagnitude, 0.2f);
            }

            enemy.HealthSystem.TakeDamage((int)(damage * effectStrength));
            enemy.ApplyKnockback(blastDirection * (magnitude * effectStrength));
            ParticleSystem.PlayExplosion(worldPosition, (int)magnitude / 10);
        }

        var corpses = ScrapSystem.Corpses.GetValuesFromBinsInRange(worldPosition, radius).ToArray();

        foreach (var corpse in corpses)
        {
            var diff = corpse.Position + corpse.Size / 2 - worldPosition;
            var distance = diff.Length();

            if (distance > radius) continue;

            var blastDirection = diff;
            blastDirection.Normalize();

            var effectStrength = 1f;

            if (useEffectFalloff)
            {
                // 0 at center, 1 at radius edge
                var rawMagnitude = distance / radius;
                effectStrength = MathHelper.Max(1f - rawMagnitude, 0.2f);
            }

            corpse.ApplyKnockback(blastDirection * (magnitude * effectStrength));
        }
    }
}
