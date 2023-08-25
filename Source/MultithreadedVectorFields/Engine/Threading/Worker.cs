//Code from Jon Watte
//http://www.enchantedage.com/
//http://xboxforums.create.msdn.com/forums/p/16153/84662.aspx

using System;

namespace MultithreadedVectorFields.Engine.Threading;

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
