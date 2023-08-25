// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using MultithreadedVectorFields.Engine;
using MultithreadedVectorFields.Gameplay.GameMaps;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MultithreadedVectorFields.Gameplay;

public sealed class CalculateVectorFieldsJob
{
    readonly TileMap _tileMap;
    readonly int _width;
    readonly int _height;

    readonly Dictionary<Entity, VectorField> _vectorFields;
    readonly Pool<VectorField> _vectorFieldPool;

    readonly Queue<VectorFieldRequest> _requests = new();
    readonly BlockingCollection<VectorFieldResult> _results = new();

    public struct VectorFieldRequest
    {
        public VectorField VectorField;
        public int GoalX;
        public int GoalY;
        public Entity Entity;
    };

    public struct VectorFieldResult
    {
        public Entity Entity;
        public VectorField VectorField;
    }

    public CalculateVectorFieldsJob(
        Dictionary<Entity, VectorField> vectorFields,
        TileMap tileMap,
        uint width,
        uint height)
    {
        _vectorFields = vectorFields;
        _tileMap = tileMap;
        _width = (int)width;
        _height = (int)height;

        _vectorFieldPool = new(64, true, () => new VectorField(width, height));
    }

    public void Add(
        int goalX,
        int goalY,
        Entity entity)
    {
        _requests.Enqueue(new VectorFieldRequest
        {
            VectorField = _vectorFieldPool.Allocate(),
            GoalX = goalX,
            GoalY = goalY,
            Entity = entity
        });
    }

    public void Start()
    {
        while (_requests.Count > 0)
        {
            var request = _requests.Dequeue();

            MultithreadedVectorFieldsGame.Instance.DesktopThreadPool.AddTask(
                () => request.VectorField.Calculate(_tileMap, request.GoalX, request.GoalY, _width, _height, (v) =>
                {
                    _results.Add(new()
                    {
                        Entity = request.Entity,
                        VectorField = request.VectorField,
                    });
                }),
                null,
                null);
        }
    }

    public bool HasResults => _results.Count > 0;

    public VectorFieldResult Consume()
    {
        var result = _results.Take();

        if (_vectorFields.TryGetValue(result.Entity, out var vectorField))
            _vectorFieldPool.Deallocate(vectorField);

        _vectorFields[result.Entity] = result.VectorField;

        return result;
    }
}