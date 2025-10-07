using System;
using System.Collections.Generic;

namespace _2d_td;

#nullable enable
public class EffectSystem
{
    private Entity entity;
    private Dictionary<Action<Entity, float>, float> LifetimeOfEffects = new();

    public EffectSystem(Entity _parent)
    {
        entity = _parent;

        LifetimeOfEffects.Add(DeathEffect, 0);
    }

    public void Update(float deltaTime)
    {
        foreach (var pair in LifetimeOfEffects)
        {
            var effect = pair.Key;
            var lifetime = pair.Value;

            if (lifetime <= 0)
            {
                continue;
            }

            lifetime -= deltaTime;

            effect(entity, deltaTime);
        }
    }

    public void AddEffects(Action<Entity, float> effectFunction, float lifetime)
    {  
        LifetimeOfEffects[effectFunction] = Math.Max(LifetimeOfEffects[effectFunction], lifetime);
    }

    public void RemoveEffects(Action<Entity, float> effectFunction)
    {
        LifetimeOfEffects[effectFunction] = 0f;
    }

    public void FireEffect(Entity entity, float deltaTime)
    {
        if (entity is Enemy enemy)
        {
            enemy.HealthSystem.TakeDamage((int)Math.Floor(5*deltaTime+0.5));
        }
    }
}
