// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using MultithreadedVectorFields.Engine;
using MultithreadedVectorFields.Engine.Threading;
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

    readonly BlockingCollection<VectorFieldRequest> _requests = new();
    readonly BlockingCollection<VectorFieldResult> _results = new();

    readonly TaskFunction _taskFunction; //making this an instance field removes one source of memory allocation

    public struct VectorFieldRequest
    {
        public Entity Entity;
        public int GoalX;
        public int GoalY;
        public VectorField VectorField;
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

        _taskFunction = ProcessRequest;
    }

    public void Add(
        Entity entity,
        int goalX,
        int goalY)
    {
        _requests.Add(new VectorFieldRequest
        {
            Entity = entity,
            GoalX = goalX,
            GoalY = goalY,
            VectorField = _vectorFieldPool.Allocate()
        });

        MultithreadedVectorFieldsGame.Instance.DesktopThreadPool.AddTask(
            _taskFunction,
            null,
            null);
    }

    void ProcessRequest()
    {
        //Beware - processing is performed on a worker thread here!
        //Be careful about accessing shared data.
        //_request and _results are BlockingCollection<T>, ideally suited for a multithreaded environment
        //where one thread produces data and another consumes it.

        if (_requests.Count == 0)
            return;

        var request = _requests.Take();

        request.VectorField.Calculate(_tileMap, request.GoalX, request.GoalY, _width, _height);

        _results.Add(new()
        {
            Entity = request.Entity,
            VectorField = request.VectorField,
        });
    }

    public bool HasResults => _results.Count > 0;

    public VectorFieldResult Consume()
    {
        //Back on the main thread here!

        var result = _results.Take();

        //Unsure if this should live here or the ConsumeVectorFieldSystem - will mull it over
        if (_vectorFields.TryGetValue(result.Entity, out var vectorField))
            _vectorFieldPool.Deallocate(vectorField);

        return result;
    }
}