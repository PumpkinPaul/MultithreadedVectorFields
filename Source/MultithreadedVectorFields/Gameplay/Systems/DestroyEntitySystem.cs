// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using System;

namespace MultithreadedVectorFields.Gameplay.Systems;

public readonly record struct DestroyEntityMessage(
    Entity Entity
);

/// <summary>
/// Removes 'dead' entities from the world.
/// </summary>
public sealed class DestroyEntitySystem : MoonTools.ECS.System
{
    public DestroyEntitySystem(World world) : base(world)
    {
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<DestroyEntityMessage>())
        {
            Destroy(message.Entity);
        }
    }
}
