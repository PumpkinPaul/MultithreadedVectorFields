using System.Linq;

namespace PumpkinGames.Glitchangels.Extensions
{
    public static class ObjectExtensions
    {
        public static bool In<T>(this T o, params T[] items)
        {
            return items.Contains(o);
        }
    }
}
