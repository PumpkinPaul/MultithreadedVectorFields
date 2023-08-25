// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MultithreadedVectorFields.Engine.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MultithreadedVectorFields.Engine.Collections;

/// <summary>
/// A simple implementation of a list and hash combo
/// </summary>
/// <remarks>
/// Only Add and Get are implemented - no individual removal - this helps us keep the implementation simple and the usage clean. 
/// </remarks>
public class SimpleHashList<TKey, TValue> : IEnumerable<TValue>, IEnumerable
{
    readonly List<TValue> _list = new();
    readonly IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>(EqualityComparer<TKey>.Default);

    public TValue this[TKey key]
    {
        get => Get(key);
        set => Add(key, value);
    }

    public TValue GetIndex(int idx) => _list[idx];

    public void Clear()
    {
        _list.Clear();
        _dictionary.Clear();
    }

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public void Add(TKey key, TValue value)
    {
        if (_dictionary.ContainsKey(key))
            throw new ArgumentException($"The key {key} has already been added");

        _list.Add(value);
        _dictionary.Add(key, value);
    }

    public TValue Get(TKey key)
    {
        return _dictionary.GetByKeyOrDefault(key);
    }

    public List<TValue>.Enumerator GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> KeyValues()
    {
        foreach (var item in (Dictionary<TKey, TValue>)_dictionary)
            yield return item;
    }

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public IEnumerable<TValue> Values => _list;

    public int Count => _dictionary.Count;
}
