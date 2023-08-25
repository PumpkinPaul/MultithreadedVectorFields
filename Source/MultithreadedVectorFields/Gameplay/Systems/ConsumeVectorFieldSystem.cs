// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using MultithreadedVectorFields.Gameplay.Components;
using System;
using System.Collections.Generic;

namespace MultithreadedVectorFields.Gameplay.Systems;

public sealed class ConsumeVectorFieldSystem : MoonTools.ECS.System
{
    readonly CalculateVectorFieldsJob _calculateVectorFieldsJob;

    readonly Dictionary<Entity, VectorField> _vectorFields;

    public ConsumeVectorFieldSystem(
        World world,
        Dictionary<Entity, VectorField> vectorFields,
        CalculateVectorFieldsJob calculateVectorFieldsJob
    ) : base(world)
    {
        _vectorFields = vectorFields;
        _calculateVectorFieldsJob = calculateVectorFieldsJob;
    }

    public override void Update(TimeSpan delta)
    {
        while (_calculateVectorFieldsJob.HasResults)
        {
            var result = _calculateVectorFieldsJob.Consume();

            _vectorFields[result.Entity] = result.VectorField;
            Set(result.Entity, new CreateVectorFieldComponent());
        }
    }
}