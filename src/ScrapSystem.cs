using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public static class ScrapSystem
{
    private static bool clearingScrap;
    private static readonly float clearStepInterval = 0.1f;
    private static float clearStepTimer;

    public static List<Entity> Corpses = new();

    public static void Initialize()
    {
        foreach (var item in Corpses)
        {
            item.Destroy();
        }

        Corpses = new();

        WaveSystem.WaveEnded += ClearScrap;
    }

    public static void Update(GameTime gameTime)
    {
        if (!clearingScrap) return;

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        clearStepTimer -= deltaTime;

        if (clearStepTimer <= 0)
        {
            clearStepTimer = clearStepInterval;

            if (Corpses.Count > 0)
            {
                var index = Corpses.Count - 1;
                Corpses[index].Destroy();
                Corpses.RemoveAt(index);

                if (Corpses.Count == 0)
                {
                    clearingScrap = false;
                }
            }
            else
            {
                clearingScrap = false;
            }
        }
    }

    public static void AddCorpse(Game1 game, Vector2 position, AnimationSystem.AnimationData animation)
    {
        var corpse = new Entity(game, position, animation);
        Corpses.Add(corpse);
    }

    public static bool IsPointInCorpse(Vector2 point)
    {
        foreach (var corpse in Corpses)
        {
            if (Collision.IsPointInEntity(point, corpse)) return true;
        }

        return false;
    }

    private static void ClearScrap()
    {
        if (clearingScrap) return;

        clearingScrap = true;
        clearStepTimer = clearStepInterval;
    }
}
