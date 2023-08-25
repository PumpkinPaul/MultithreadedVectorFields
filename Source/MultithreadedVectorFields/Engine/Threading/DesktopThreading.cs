// Copyright Pumpkin Games Ltd. All Rights Reserved.

using System.Threading;
using System;

namespace MultithreadedVectorFields.Engine.Threading;

public class DesktopThreading : IPlatformThreading
{
    public void StartThread(string name, int hardwareThread, ThreadStart threadStart)
    {
        var t = new Thread(threadStart)
        {
            Name = name,
            IsBackground = true
        };
        t.Start();
    }

    public int[] CreateHardwareThreadIds()
    {
        return new int[Environment.ProcessorCount];
    }

    public string[] CreateThreadNames(int count)
    {
        var threadNames = new string[count];

        for (var i = 0; i < count; i++)
            threadNames[i] = $"FNA Worker Thread {i}";

        return threadNames;
    }
}