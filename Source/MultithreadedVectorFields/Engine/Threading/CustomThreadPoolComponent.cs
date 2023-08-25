//Code from Jon Watte
//http://www.enchantedage.com/
//http://xboxforums.create.msdn.com/forums/p/16153/84662.aspx

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

/// <summary>
/// You typically only create a single ThreadPoolComponent in your application,
/// and let all your threaded tasks run within this component. This allows for
/// ideal thread balancing. If you have multiple components, they will not know
/// how to share the CPU between them fairly.
/// </summary>
public class CustomThreadPoolComponent : GameComponent
{
    readonly object _nextThreadTargetLock = new();
    int _nextThreadTarget = 0;

    readonly ThreadPoolWrapper[] _wrappers;

    public int ThreadCount { get; private set; }

    /// <summary>
    /// Create the ThreadPoolComponent in your application constructor, and add it
    /// to your Components collection. The ThreadPool will deliver any completed
    /// tasks first in the update order.
    ///
    /// Creates one or more threads, depending on the number of CPU cores. 
    /// Always creates at least one thread. The thread tasks are assumed to be computationally
    /// expensive, so more threads than there are CPU cores is not recommended.
    /// </summary>
    /// <param name="game">Your game instance.</param>
    public CustomThreadPoolComponent(BaseGame game) : base(game)
    {
        UpdateOrder = int.MinValue;

        ThreadCount = Environment.ProcessorCount;

        var threadNames = CreateThreadNames(ThreadCount);

        _wrappers = new ThreadPoolWrapper[ThreadCount];
        for (var i = 0; i != ThreadCount; ++i)
            _wrappers[i] = new ThreadPoolWrapper(threadNames[i]);
    }

    static string[] CreateThreadNames(int count)
    {
        var threadNames = new string[count];

        for (var i = 0; i < count; i++)
            threadNames[i] = $"FNA Worker Thread {i}";

        return threadNames;
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var wrapper in _wrappers)
            wrapper.Update();
    }

    public ITask AddTask(TaskFunction function, TaskComplete completion, ITaskContext ctx)
    {
        //Just cycle to the next free one.
        lock (_nextThreadTargetLock)
            _nextThreadTarget = (_nextThreadTarget + 1) % ThreadCount;

        return AddTask(_nextThreadTarget, function, completion, ctx);
    }

    public ITask AddTask(int threadTarget, TaskFunction function, TaskComplete completion, ITaskContext ctx)
    {
        return _wrappers[threadTarget].AddTask(function, completion, ctx);
    }

    protected override void Dispose(bool disposing)
    {
        foreach (var wrapper in _wrappers)
            wrapper.Dispose();
    }

    public TaskContext NewTaskContext()
    {
        lock (this)
        {
            _taskContextList ??= new TaskContext(this);

            var ret = _taskContextList;
            _taskContextList = ret.Next;
            ret.Next = null;
            return ret;
        }
    }

    internal void Reclaim(TaskContext ctx)
    {
        lock (this)
        {
            ctx.Next = _taskContextList;
            _taskContextList = ctx;
        }
    }

    private TaskContext _taskContextList;
}
