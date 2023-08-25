using System;
using System.IO;

namespace PumpkinGames.Glitchangels.Extensions
{
    /// <summary>
    /// Extension methods for Enums.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Gets the parent of a directory a number of levels 'up'.
        /// </summary>
        /// <returns>The directoty info object.</returns>
        public static DirectoryInfo GetParent(this DirectoryInfo directoryInfo, uint levels) 
        {
            if (directoryInfo == null) throw 
                new ArgumentNullException(nameof(directoryInfo));

            uint count = 0;
            var di = directoryInfo;

            while (count < levels && di != null)
            {
                di = di.Parent;

                count++;
            }

            return di;
        }
    }
}
