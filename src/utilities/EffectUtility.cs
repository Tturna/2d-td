using _2d_td.interfaces;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class EffectUtility
{
    /// <summary>
    /// Apply explosion effect. Knocks back and damages enemies and corpses in range.
    /// Pass useEffectFalloff = false to prevent damage and knockback from being reduced for
    /// entities further from the explosion.
    /// </summary>
    public static void Explode(Entity source, Vector2 worldPosition, float radius, float magnitude,
        int damage, bool useEffectFalloff = true, AnimationSystem.AnimationData? animation = null,
        bool hurtTowers = false)
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
                // raw magnitude is usually like 0.295 on a direct hit to a node enemy.
                // That should count as max damage. 1.3 - raw magnitude means about 1x
                // strength on a direct hit and at least about 0.3x for an indirect hit.
                effectStrength = 1.3f - rawMagnitude;
            }

            enemy.HealthSystem.TakeDamage(source, (int)(damage * effectStrength));

            if (blastDirection.Y > -1)
            {
                blastDirection = new Vector2(blastDirection.X , -1);
            }

            enemy.ApplyKnockback(blastDirection * (magnitude * effectStrength));
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

            if (blastDirection.Y > -1)
            {
                blastDirection = new Vector2(blastDirection.X , -1);
            }

            corpse.ApplyKnockback(blastDirection * (magnitude * effectStrength));
        }

        if (hurtTowers)
        {
            foreach (var tower in BuildingSystem.Towers)
            {
                var minSide = MathHelper.Min(tower.Size.X, tower.Size.Y);
                var diff = tower.Position + tower.Size / 2 - worldPosition;
                var distance = diff.Length();

                if (distance - minSide > radius) continue;

                ((ITower)tower).GetTowerCore().Health.TakeDamage(source, damage);
            }
        }

        SoundSystem.PlaySound("explosion");
        ParticleSystem.PlayExplosion(worldPosition, (int)magnitude / 10);
        CameraManager.Instance.ShakeCamera(0.75f, 0.1f);

        if (animation is not null)
        {
            new Explosion(Game1.Instance, worldPosition, animation: (AnimationSystem.AnimationData)animation);
        }
        else
        {
            new Explosion(Game1.Instance, worldPosition);
        }
    }
}
