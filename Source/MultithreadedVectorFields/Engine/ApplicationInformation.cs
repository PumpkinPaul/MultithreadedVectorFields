// Copyright Pumpkin Games Ltd. All Rights Reserved.

namespace MultithreadedVectorFields.Engine;

public static class ApplicationInformation
{
    public static System.Reflection.Assembly ExecutingAssembly => _executingAssembly ??= System.Reflection.Assembly.GetExecutingAssembly();

    private static System.Reflection.Assembly _executingAssembly;
}
