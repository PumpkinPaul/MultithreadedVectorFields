// Copyright Pumpkin Games Ltd. All Rights Reserved.

using System.Threading;

namespace MultithreadedVectorFields.Engine.Threading;

public interface IPlatformThreading
{
    void StartThread(string name, int hardwareThread, ThreadStart threadStart);

    int[] CreateHardwareThreadIds();

    string[] CreateThreadNames(int count);
}