// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using MultithreadedVectorFields.Engine;
using MultithreadedVectorFields.Gameplay.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MultithreadedVectorFields.Gameplay.Systems;

public sealed class ConsumeVectorFieldSystem : MoonTools.ECS.System
{
    readonly BlockingCollection<VectorFieldResult> _vectorFieldResults;
    readonly Dictionary<Entity, VectorField> _vectorFields;
    readonly Pool<VectorField> _vectorFieldPool;

    public ConsumeVectorFieldSystem(
        World world,
        BlockingCollection<VectorFieldResult> vectorFieldResults,
        Dictionary<Entity, VectorField> vectorFields,
        Pool<VectorField> vectorFieldPool
    ) : base(world)
    {
        _vectorFieldResults = vectorFieldResults;
        _vectorFields = vectorFields;
        _vectorFieldPool = vectorFieldPool;
    }

    public override void Update(TimeSpan delta)
    {
        while (_vectorFieldResults.Count > 0)
        {
            var result = _vectorFieldResults.Take();

            if (_vectorFields.TryGetValue(result.Entity, out var vectorField))
                _vectorFieldPool.Deallocate(vectorField);

            result.Pool.Deallocate(result.State);

            _vectorFields[result.Entity] = result.VectorField;
            Set(result.Entity, new CreateVectorFieldComponent());
        }
    }
}