using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public class EffectSystem
{
    private Entity entity;
    private List<Action<Entity>> effects = [];

    public EffectSystem(Entity _parent)
    {
        entity = _parent;
    }

    public void Update(float deltaTime)
    {
        foreach (var effect in effects)
        {
            effect(entity);
        }
    }

    public void AddEffects(Action<Entity> effectFunction)
    {
        effects.Add(effectFunction);
    }

    public void RemoveEffects(Action<Entity> effectFunction)
    {
        effects.Remove(effectFunction);
    }
}
