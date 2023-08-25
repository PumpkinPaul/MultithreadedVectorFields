// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using MultithreadedVectorFields.Gameplay.Components;
using System;

namespace MultithreadedVectorFields.Gameplay.Systems;

/// <summary>
/// Responsible for moving entities by updating their positions from their velocity.
/// </summary>
public sealed class MovementSystem : MoonTools.ECS.System
{
    readonly Filter _filter;

    public MovementSystem(World world) : base(world)
    {
        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Include<VelocityComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var position = ref Get<PositionComponent>(entity);
            ref readonly var velocity = ref Get<VelocityComponent>(entity);

            Set(entity, new PositionComponent(position.Value + velocity.Value));
        }
    }
}
