namespace PumpkinGames.Glitchangels.Extensions
{
    /// <summary>
    /// Extension methods for Int.
    /// </summary>
    public static class FloatExtensions
    {
        public static bool FloatEquals(this float source, float value, float epsilon = 0.0001f)
        {
            return source + epsilon >= value && source - epsilon <= value;
        }
    }
}
