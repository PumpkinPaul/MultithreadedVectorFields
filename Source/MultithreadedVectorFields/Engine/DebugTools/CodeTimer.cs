// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using System;

namespace MultithreadedVectorFields.Engine.DebugTools;

public readonly record struct CodeTimer : IDisposable
{
    private readonly string _name;

    public CodeTimer(string name, Color color)
    {
        _name = name;

        DebugSystem.TimeRuler.BeginMark(name, color);
    }

    public void Dispose()
    {
        DebugSystem.TimeRuler.EndMark(_name);
    }
}