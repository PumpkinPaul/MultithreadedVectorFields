// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using MultithreadedVectorFields.Gameplay.Components;
using MultithreadedVectorFields.Gameplay.GameMaps;
using System;

namespace MultithreadedVectorFields.Gameplay.Systems;

public readonly record struct AgentSpawnMessage(
    Vector2 Position,
    Color Color
);

/// <summary>
/// Responsible for spawning agents with the correct components.
/// </summary>
public class AgentSpawnSystem : MoonTools.ECS.System
{    
    public AgentSpawnSystem(World world) : base(world)
    {
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<AgentSpawnMessage>())
        {
            var entity = CreateEntity();

            Set(entity, new PositionComponent(message.Position));
            Set(entity, new ScaleComponent(new Vector2(TileMap.TILE_SIZE, TileMap.TILE_SIZE)));
            Set(entity, new ColorComponent(message.Color));
            Set(entity, new VelocityComponent());
            Set(entity, new CreateVectorFieldComponent());
        }
    }
}