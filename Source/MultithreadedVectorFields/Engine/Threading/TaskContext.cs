//Code from Jon Watte
//http://www.enchantedage.com/
//http://xboxforums.create.msdn.com/forums/p/16153/84662.aspx

using System;
using System.Threading;

namespace MultithreadedVectorFields.Engine.Threading;

public class TaskContext : ITaskContext, IDisposable
{
    internal DesktopThreadPoolComponent FnaThreadPoolComponent;
    internal TaskContext Next;
    internal ManualResetEvent Event = new(false);
    internal Worker Worker;

    internal TaskContext(DesktopThreadPoolComponent tpc)
    {
        FnaThreadPoolComponent = tpc;
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

    public void Dispose()
    {
        if (Worker == null)
            throw new ObjectDisposedException("TaskContext.Dispose()");

        Worker.Context = null;
        Worker = null;
        FnaThreadPoolComponent.Reclaim(this);

        GC.SuppressFinalize(this);
    }
}