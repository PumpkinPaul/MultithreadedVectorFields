//Code from Jon Watte
//http://www.enchantedage.com/
//http://xboxforums.create.msdn.com/forums/p/16153/84662.aspx

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
public class ThreadPoolWrapper : IDisposable
{
    public ThreadPoolWrapper(string name)
    {
        StartThread(name, ThreadFunc);
    }

    static void StartThread(string name, ThreadStart threadStart)
    {
        var t = new Thread(threadStart)
        {
            Name = name,
            IsBackground = true
        };
        t.Start();
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

        //deliver all completed items
        DeliverComplete();

        GC.SuppressFinalize(this);
    }

    private readonly AutoResetEvent _workEvent = new(false);

    public void Update()
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
                worker1.Completion?.Invoke(worker1, worker1.Error);
            }
            catch (Exception x)
            {
                Debug.WriteLine($"Exception thrown within worker completion! {x.Message}");

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