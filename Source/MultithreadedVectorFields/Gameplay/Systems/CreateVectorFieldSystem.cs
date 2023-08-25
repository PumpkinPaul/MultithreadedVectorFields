// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using MultithreadedVectorFields.Engine;
using MultithreadedVectorFields.Engine.Threading;
using MultithreadedVectorFields.Gameplay.Components;
using MultithreadedVectorFields.Gameplay.GameMaps;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using static MultithreadedVectorFields.Gameplay.VectorField;

namespace MultithreadedVectorFields.Gameplay.Systems;

public struct VectorFieldResult
{
    public Entity Entity;
    public VectorField VectorField;
    public VectorFieldState State;
    public Pool<VectorFieldState> Pool;
}

public sealed class CreateVectorFieldSystem : MoonTools.ECS.System
{
    readonly TileMap _tileMap;
    readonly BlockingCollection<VectorFieldResult> _vectorFieldResults;
    readonly Pool<VectorField> _vectorFieldPool;
    readonly Pool<VectorFieldState> _state = new(64, true, () => new VectorFieldState());

    readonly Filter _filter;

    public CreateVectorFieldSystem(
        World world,
        TileMap tileMap,
        BlockingCollection<VectorFieldResult> vectorFieldResults,
        Pool<VectorField> vectorFieldPool
    ) : base(world)
    {
        _tileMap = tileMap;
        _vectorFieldResults = vectorFieldResults;
        _vectorFieldPool = vectorFieldPool;

        _filter = FilterBuilder
           .Include<CreateVectorFieldComponent>()
           .Include<PositionComponent>()
           .Build();
    }
    static void Test() { }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            var position = Get<PositionComponent>(entity);

            Remove<CreateVectorFieldComponent>(entity);

            //var vectorField = _vectorFieldPool.Allocate();

            //var state = _state.Allocate();
            //state.Pool = _state;
            //state.VectorField = vectorField;
            //state.TileMap = _tileMap;
            //state.GoalX = (int)position.Value.X / TileMap.TILE_SIZE;
            //state.GoalY = (int)position.Value.Y / TileMap.TILE_SIZE;
            //state.Width = (int)TileMap.MAX_COLUMNS;
            //state.Height = (int)TileMap.MAX_ROWS;
            //state.Entity = entity;
            //state.Results = _vectorFieldResults;

            //No allocations are in fact created here - is the analyzer wrong or does the compiler/jitter do some magics
            //MultithreadedVectorFieldsGame.Instance.DesktopThreadPool.AddTask(Test, null, null);

            var vectorField = _vectorFieldPool.Allocate();

            var state = _state.Allocate();
            state.Pool = _state;
            state.VectorField = vectorField;
            state.TileMap = _tileMap;
            state.GoalX = (int)position.Value.X / TileMap.TILE_SIZE;
            state.GoalY = (int)position.Value.Y / TileMap.TILE_SIZE;
            state.Width = (int)TileMap.MAX_COLUMNS;
            state.Height = (int)TileMap.MAX_ROWS;
            state.Entity = entity;
            state.Results = _vectorFieldResults;

            var action = new Action<VectorFieldState>(vectorField.Calculate);

            ThreadPool.QueueUserWorkItem(
                action,
                state,
                false
            );

            //var vectorField = _vectorFieldPool.Allocate();

            //var state = _state.Allocate();
            //state.Pool = _state;
            //state.VectorField = vectorField;
            //state.TileMap = _tileMap;
            //state.GoalX = (int)position.Value.X / TileMap.TILE_SIZE;
            //state.GoalY = (int)position.Value.Y / TileMap.TILE_SIZE;
            //state.Width = (int)TileMap.MAX_COLUMNS;
            //state.Height = (int)TileMap.MAX_ROWS;
            //state.Entity = entity;
            //state.Results = _vectorFieldResults;

            //Task.Factory.StartNew(
            //    Calculate,
            //    state,
            //    CancellationToken.None,
            //    TaskCreationOptions.DenyChildAttach,
            //    TaskScheduler.Default
            //);

            //Task.Factory.StartNew(
            //    () => vectorField.Calculate(
            //        _tileMap,
            //        (int)position.Value.X / TileMap.TILE_SIZE,
            //        (int)position.Value.Y / TileMap.TILE_SIZE,
            //        (int)TileMap.MAX_COLUMNS,
            //        (int)TileMap.MAX_ROWS,
            //        (v) =>
            //        {
            //            _vectorFieldResults.Add(new VectorFieldResult
            //            {
            //                Entity = entity,
            //                VectorField = v
            //            });
            //        }),
            //    CancellationToken.None,
            //    TaskCreationOptions.DenyChildAttach,
            //    TaskScheduler.Default
            //);

            //Task.Run(
            //    () => vectorField.Calculate(
            //        _tileMap,
            //        (int)position.Value.X / TileMap.TILE_SIZE,
            //        (int)position.Value.Y / TileMap.TILE_SIZE,
            //        (int)TileMap.MAX_COLUMNS,
            //        (int)TileMap.MAX_ROWS,
            //        () =>
            //        {
            //            _vectorFieldResults.Add(new VectorFieldResult
            //            {
            //                Entity = entity,
            //                VectorField = vectorField
            //            });
            //        })
            //);
        }
    }
}