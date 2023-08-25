// Copyright Pumpkin Games Ltd. All Rights Reserved.

using System.Collections.Generic;

namespace MultithreadedVectorFields.Engine.Extensions;

/// <summary>
/// Extensions to the IDictionary interface.
/// </summary>
public static class IDictionaryExtensions
{
    public static TValue GetByKeyOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.ContainsKey(key) ? dictionary[key] : default;
    }

    public static TValue GetByKeyOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
    }
}
