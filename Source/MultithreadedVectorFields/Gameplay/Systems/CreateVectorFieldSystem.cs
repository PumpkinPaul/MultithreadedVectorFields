// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using MultithreadedVectorFields.Gameplay.Components;
using MultithreadedVectorFields.Gameplay.GameMaps;
using System;

namespace MultithreadedVectorFields.Gameplay.Systems;


public sealed class CreateVectorFieldSystem : MoonTools.ECS.System
{
    readonly CalculateVectorFieldsJob _calculateVectorFieldsJob;

    readonly Filter _filter;

    public CreateVectorFieldSystem(
        World world,
        CalculateVectorFieldsJob calculateVectorFieldsJob
    ) : base(world)
    {
        _calculateVectorFieldsJob = calculateVectorFieldsJob;

        _filter = FilterBuilder
           .Include<CreateVectorFieldComponent>()
           .Include<PositionComponent>()
           .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            var position = Get<PositionComponent>(entity);

            Remove<CreateVectorFieldComponent>(entity);

            _calculateVectorFieldsJob.Add(
                (int)position.Value.X / TileMap.TILE_SIZE, 
                (int)position.Value.Y / TileMap.TILE_SIZE,
                entity);
        }

        _calculateVectorFieldsJob.Start();
    }
}