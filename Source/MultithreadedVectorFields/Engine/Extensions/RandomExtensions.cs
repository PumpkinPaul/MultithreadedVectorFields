// Copyright Pumpkin Games Ltd. All Rights Reserved.

using System;

namespace MultithreadedVectorFields.Engine.Extensions;

public static class RandomExtensions
{
    public static float NextFloat(this Random random)
    {
        return (float)random.NextDouble();
    }

    public static float NextFloat(this Random random, float minimum, float maximum)
    {
        return (float)random.NextDouble() * (maximum - minimum) + minimum;
    }
}
