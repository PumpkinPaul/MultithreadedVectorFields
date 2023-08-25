// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using System;

namespace MultithreadedVectorFields.Engine.Threading;

/// <summary>
/// Called when a given task is complete, or has errored out.
/// </summary>
/// <param name="task">The task that completed.</param>
/// <param name="error">null on success, non-null on error</param>
public delegate void TaskComplete(ITask task, Exception error);

/// <summary>
/// The TaskFunction delegate is called within the worker thread to do work.
/// </summary>
public delegate void TaskFunction();

public abstract class ThreadPoolComponent : GameComponent
{
    protected ThreadPoolComponent(BaseGame game) : base(game)
    {
    }

    public abstract ITask AddTask(ThreadTarget threadTarget, TaskFunction function, TaskComplete completion, ITaskContext ctx);
}