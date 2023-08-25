// Copyright Pumpkin Games Ltd. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;

namespace MultithreadedVectorFields.Engine.DebugTools;

public class FixedLengthQueue<T> : IEnumerable<T>
{
    private readonly Queue<T> _queue;
    private int _capacity;

    /// <summary>
    /// When an item is dequeued it will get queued into <c>DequeueTarget</c>, if any.
    /// </summary>
    public FixedLengthQueue<T> DequeueTarget;

    /// <summary>
    /// Gets the number of items in the queue.
    /// </summary>
    /// <value>The count</value>
    public int Count => _queue.Count;

    /// <summary>
    /// Setting this is O(abs(Capacity - value))
    /// </summary>
    public int Capacity
    {
        get
        {
            return _capacity;
        }

        set
        {
            var diff = Count - value;
            while (diff-- > 0)
                Dequeue();
            _capacity = value;
        }
    }

    public FixedLengthQueue(int capacity)
    {
        _queue = new Queue<T>(capacity);
        _capacity = capacity;
    }

    /// <summary>
    /// Enqueues the specified item. If the queue is full, the oldest item
    /// will be droppped.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Enqueue(T item)
    {
        if (_queue.Count + 1 > _capacity && _queue.Count > 0)
        {
            var removedItem = _queue.Dequeue();
            DequeueTarget?.Enqueue(removedItem);
        }

        _queue.Enqueue(item);
    }

    /// <summary>
    /// Dequeues the oldest item.
    /// </summary>
    public T Dequeue()
    {
        return _queue.Dequeue();
    }

    /// <summary>
    /// Dequeues the oldest item.
    /// </summary>
    public void Clear()
    {
        _queue.Clear();
    }

    //Explicit implementation to avoid boxing the Enumerator.
    public Queue<T>.Enumerator GetEnumerator()
    {
        return _queue.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return _queue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _queue.GetEnumerator();
    }

}