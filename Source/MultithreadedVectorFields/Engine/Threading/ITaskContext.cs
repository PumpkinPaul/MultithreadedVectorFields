// Copyright Pumpkin Games Ltd. All Rights Reserved.

namespace MultithreadedVectorFields.Engine.Threading;

public interface ITaskContext
{
    void Init(ITask task);
}