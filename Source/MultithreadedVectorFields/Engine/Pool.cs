// Copyright Pumpkin Games Ltd. All Rights Reserved.

using System;
using System.Collections.Generic;

namespace MultithreadedVectorFields.Engine;

/// <summary>
/// A pool of items.
/// </summary>
public class Pool<T> : IPool<T>
{
    readonly Stack<T> _availableItems;
    readonly List<T> _allocatedItems;

    readonly bool _canGrow;
    readonly Func<T> _create;

    public Pool(int size, bool canGrow, Func<T> create)
    {
        _create = create ?? throw new ArgumentNullException(nameof(create));

        _canGrow = canGrow;

        _availableItems = new Stack<T>(size);
        _allocatedItems = new List<T>(size);

        for (var i = 0; i < size; i++)
            _availableItems.Push(_create());
    }

    public T Allocate()
    {
        if (_availableItems.Count == 0)
        {
            if (_canGrow)
                _availableItems.Push(_create());
            else
                return default;
        }

        var item = _availableItems.Pop();
        (item as IInitialisable)?.Initialise();

        _allocatedItems.Add(item);

        return item;
    }

    private void DeallocateItem(T item)
    {
        //We need to do this so we can reuse a single factory for protobuf-net
        //other wise we can have stale data in fields and properties of the new entities
        //(item as IInitialisable)?.Initialise();
        _availableItems.Push(item);
    }

    public void Deallocate(T item)
    {
        DeallocateItem(item);
        _allocatedItems.Remove(item);
    }

    public void DeallocateAll()
    {
        foreach (var item in _allocatedItems)
            DeallocateItem(item);

        _allocatedItems.Clear();
    }
}
