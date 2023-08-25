//Code from Jon Watte
//http://www.enchantedage.com/
//http://xboxforums.create.msdn.com/forums/p/16153/84662.aspx

using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Threading;

namespace MultithreadedVectorFields.Engine.Threading;

/// <summary>
/// You typically only create a single ThreadPoolComponent in your application,
/// and let all your threaded tasks run within this component. This allows for
/// ideal thread balancing. If you have multiple components, they will not know
/// how to share the CPU between them fairly.
/// </summary>
public class DesktopThreadPoolComponent : ThreadPoolComponent
{
    readonly object _nextThreadTargetLock = new();
    ThreadTarget _nextThreadTarget = ThreadTarget.CORE1_THREAD3;

    readonly ThreadPoolWrapper[] _wrappers;

    readonly int _nThreads;

    readonly IPlatformThreading _platformThreading;

    /// <summary>
    /// Create the ThreadPoolComponent in your application constructor, and add it
    /// to your Components collection. The ThreadPool will deliver any completed
    /// tasks first in the update order.
    ///
    /// On Xbox, creates 3 threads. On PC, creates one or more threads, depending
    /// on the number of CPU cores. Always creates at least one thread. The thread
    /// tasks are assumed to be computationally expensive, so more threads than
    /// there are CPU cores is not recommended.
    /// </summary>
    /// <param name="game">Your game instance.</param>
    public DesktopThreadPoolComponent(BaseGame game) : base(game)
    {
        _platformThreading = new DesktopThreading();

        var hardwareThread = _platformThreading.CreateHardwareThreadIds();
        _nThreads = hardwareThread.Length;

        var threadNames = _platformThreading.CreateThreadNames(_nThreads);

        _wrappers = new ThreadPoolWrapper[_nThreads];
        for (var i = 0; i != _nThreads; ++i)
            _wrappers[i] = new ThreadPoolWrapper(_platformThreading, threadNames[i], hardwareThread[i]);

        UpdateOrder = int.MinValue;
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var wrapper in _wrappers)
            wrapper.Update(gameTime);
    }

    public ITask AddTask(TaskFunction function, TaskComplete completion, ITaskContext ctx)
    {
        //Just cycle to the next free one.
        lock (_nextThreadTargetLock)
            _nextThreadTarget = (ThreadTarget)(((int)(_nextThreadTarget) + 1) % _nThreads);

        return AddTask(_nextThreadTarget, function, completion, ctx);
    }

    public override ITask AddTask(ThreadTarget threadTarget, TaskFunction function, TaskComplete completion, ITaskContext ctx)
    {
        return _wrappers[(int)threadTarget].AddTask(function, completion, ctx);
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

public class TaskContext : ITaskContext, IDisposable
{
    internal TaskContext(DesktopThreadPoolComponent tpc)
    {
        FnaThreadPoolComponent = tpc;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (Worker == null)
            throw new ObjectDisposedException("TaskContext.Dispose()");

        Worker.Context = null;
        Worker = null;
        FnaThreadPoolComponent.Reclaim(this);
    }

    public void Init(ITask w)
    {
        Worker = (Worker)w;
        Worker.Context = this;
        Event.Reset();
    }

    /// <summary>
    /// Wait will wait for the given task to complete, and then dispose
    /// the context. After Wait() returns, you should do nothing else to
    /// the context.
    /// </summary>
    public void Wait()
    {
        if (Worker == null)
            throw new ObjectDisposedException("TaskContext.Wait()");

        Worker = null;
        Event.WaitOne();
        FnaThreadPoolComponent.Reclaim(this);
    }

    public void Complete()
    {
        Event.Set();
    }

    internal DesktopThreadPoolComponent FnaThreadPoolComponent;
    internal TaskContext Next;
    internal ManualResetEvent Event = new(false);
    internal Worker Worker;
}

/// <summary>
/// You typically only create a single ThreadPoolComponent in your application,
/// and let all your threaded tasks run within this component. This allows for
/// ideal thread balancing. If you have multiple components, they will not know
/// how to share the CPU between them fairly.
/// </summary>
public class ThreadPoolWrapper : IDisposable
{
    public ThreadPoolWrapper(IPlatformThreading platformThreading, string name, int hardwareThread)
    {
        platformThreading.StartThread(name, hardwareThread, ThreadFunc);
    }

    /// <summary>
    /// Disposing the ParallelThreadPool component will immediately deliver all work
    /// items with an object disposed exception.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            throw new ObjectDisposedException("ParallelThreadPool", "double dispose of ParallelThreadPool");

        _disposed = true;
        lock (this)
        {
            //mark all work items as completed with exception
            if (_completeList == null)
                _completeList = _workList;
            else
                _completeListEnd.Next = _workList;

            _completeListEnd = _workListEnd;
            while (_workList != null)
            {
                _workList.Error = new ObjectDisposedException("ParallelThreadPool");
                _workList = _workList.Next;
            }
            _workListEnd = null;
            //unblock the threads
        }

        //let some thread know their time has come
        _workEvent.Set();
        //todo: wait for each thread
        //deliver all completed items
        DeliverComplete();
    }

    private readonly AutoResetEvent _workEvent = new(false);

    public void Update(GameTime gameTime)
    {
        //avoid an unnecessary lock if there's nothing to do
        if (_completeList != null)
            DeliverComplete();
    }

    /// <summary>
    /// Deliver all complete tasks. This is usually called for you, but can be
    /// called by you if you know that some tasks have completed.
    /// </summary>
    public void DeliverComplete()
    {
        Worker worker1, worker2;
        lock (this)
        {
            worker1 = _completeList;
            worker2 = worker1;
            _completeList = null;
            _completeListEnd = null;
        }

        if (worker2 == null)
            return;

        while (worker1 != null)
        {
            try
            {
                if (worker1.Completion != null)
                    worker1.Completion(worker1, worker1.Error);
            }
            catch (Exception x)
            {
                Debug.WriteLine("Exception thrown within worker completion! {0}", x.Message);
                //retain the un-delivered notifications; leak the worker records already delivered
                if (_completeList == null)
                    _completeList = worker1.Next;
                else
                    _completeListEnd.Next = worker1.Next;

                _completeListEnd = worker1.Next;
                throw new Exception("The thread pool user threw an exception on delivery.", x);
            }
            worker1 = worker1.Next;
        }
        lock (this)
        {
            //I could link in the entire chain in one swoop if I kept some
            //more state around, but this seems simpler.
            while (worker2 != null)
            {
                worker1 = worker2.Next;
                worker2.Next = _freeList;
                _freeList = worker2;
                worker2 = worker1;
            }
        }
    }

    /// <summary>
    /// Add a task to the thread queue. When a thread is available, it will
    /// dequeue this task and run it. Once complete, the task will be marked
    /// complete, but your application won't be called back until the next
    /// time Update() is called (so that callbacks are from the main thread).
    /// </summary>
    /// <param name="function">The function to call within the thread.</param>
    /// <param name="completion">The callback to report results to, or null. If
    /// you care about which particular task has completed, use a different instance
    /// for this delegate per task (typically, a delegate on the task itself).</param>
    /// <param name="ctx">A previously allocated TaskContext, to allow for waiting
    /// on the task, or null. It cannot have been already used.</param>
    /// <returns>A Task identifier for the operation in question. Note: because
    /// of the threaded behavior, the task may have already completed when it
    /// is returned. However, if you AddTask() from the main thread, the completion
    /// function will not yet have been called.</returns>
    public ITask AddTask(TaskFunction function, TaskComplete completion, ITaskContext ctx)
    {
        if (function == null)
            throw new ArgumentNullException(nameof(function));

        Worker w;
        lock (this)
        {
            if (_disposed)
                throw new ObjectDisposedException("ParallelThreadPool");

            _qDepth++;
            w = NewWorker(function, completion);
            ctx?.Init(w);

            if (_workList == null)
                _workList = w;
            else
                _workListEnd.Next = w;

            _workListEnd = w;
        }
        _workEvent.Set();
        return w;
    }

    private void WorkOne()
    {
        Worker w;
        _workEvent.WaitOne();

        if (_disposed)
        {
            _workEvent.Set(); //tell the next guy through
            return;
        }

        lock (this)
        {
            w = _workList;
            if (w != null)
            {
                _workList = w.Next;
                if (_workList == null)
                    _workListEnd = null;
                else
                    _workEvent.Set(); //tell the next guy through

                w.Next = null;
            }
            else
                return;
        }

        try
        {
            w.Function();
        }
        catch (Exception x)
        {
            w.Error = x;
        }

        lock (this)
        {
            if (_disposed && w.Error == null)
                w.Error = new ObjectDisposedException("ParallelThreadPool");

            if (_completeList == null)
                _completeList = w;
            else
                _completeListEnd.Next = w;

            _completeListEnd = w;
            --_qDepth;
            w.Context?.Complete();
        }
    }

    private void ThreadFunc()
    {
        while (!_disposed)
            WorkOne();
    }

    private Worker NewWorker(TaskFunction tf, TaskComplete tc)
    {
        _freeList ??= new Worker(null, null);

        var ret = _freeList;
        _freeList = ret.Next;
        ret.Function = tf;
        ret.Completion = tc;
        ret.Context = null;
        ret.Error = null;
        ret.Next = null;
        return ret;
    }

    private Worker _freeList;
    private Worker _workList;
    private Worker _workListEnd;
    private Worker _completeList;
    private Worker _completeListEnd;
    private volatile bool _disposed;
    private volatile int _qDepth;
}

public class Worker : ITask
{
    internal Worker(TaskFunction function, TaskComplete completion)
    {
        Function = function;
        Completion = completion;
        Error = null;
    }

    internal TaskContext Context;
    internal TaskFunction Function;
    internal TaskComplete Completion;
    internal Exception Error;
    internal Worker Next;
}
