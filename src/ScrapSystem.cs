using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public static class ScrapSystem
{
    private static bool clearingScrap;
    private static readonly float clearStepInterval = 0.1f;
    private static float clearStepTimer;
    private static Stack<ScrapCorpse>? corpseAddOrder;

    public static BinGrid<ScrapCorpse>? Corpses;

    public static void Initialize()
    {
        if (Corpses is not null) Corpses.Destroy();

        Corpses = new(Grid.TileLength * 2);
        corpseAddOrder = new();

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

            if (Corpses?.TotalValueCount > 0)
            {
                var corpse = corpseAddOrder!.Pop();
                CurrencyManager.AddBalance(corpse.ScrapValue);
                UIComponent.SpawnFlyoutText($"+{corpse.ScrapValue}", corpse.Position, -Vector2.UnitY * 25f,
                    lifetime: 1f, color: Color.White);
                corpse.Destroy();

                if (Corpses.TotalValueCount == 0)
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

    public static void AddCorpse(Game1 game, Vector2 position, AnimationSystem.AnimationData animation,
        int scrapValue, Vector2? knockback = null)
    {
        var corpse = new ScrapCorpse(game, position, animation, scrapValue);

        if (knockback is not null) {
            corpse.PhysicsSystem.AddForce((Vector2)knockback);
            corpse.PhysicsSystem.AddForce(-Vector2.UnitY * 0.5f);
        }

        Corpses!.Add(corpse);
        corpseAddOrder!.Push(corpse);
    }

    public static bool IsPointInCorpse(Vector2 point)
    {
        var corpseCandidates = Corpses!.GetBinAndNeighborValues(point);

        foreach (var corpse in corpseCandidates)
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
