// Copyright Pumpkin Games Ltd. All Rights Reserved.

using System;

namespace MultithreadedVectorFields;

static class Program
{
    [STAThread]
    static void Main()
    {
        new MultithreadedVectorFieldsGame().Run();
    }
}
