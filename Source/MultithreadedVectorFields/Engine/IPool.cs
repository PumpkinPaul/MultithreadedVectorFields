// Copyright Pumpkin Games Ltd. All Rights Reserved.

namespace MultithreadedVectorFields.Engine;

public interface IPool<T>
{
    T Allocate();

    void Deallocate(T item);

    void DeallocateAll();
}
