using System.Collections.Generic;

namespace PumpkinGames.Glitchangels
{
    /// <summary>
    /// Extension methods for List<T>.
    /// </summary>
    public static class ListExtensions
    {
        public static void Shuffle<T>(this List<T> source) 
        {
            for (int n = source.Count - 1; n > 0; --n)
            {
                var k = RandomHelper.FastRandom.Next(n + 1);
                var temp = source[n];
                source[n] = source[k];
                source[k] = temp;
            }
        }
    }
}
