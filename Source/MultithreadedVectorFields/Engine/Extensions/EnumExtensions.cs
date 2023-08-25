using System;
using System.ComponentModel;

namespace PumpkinGames.Glitchangels
{
    /// <summary>
    /// Extension methods for Enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the description for an enum.
        /// </summary>
        /// <param name="type">The type of enum.</param>
        /// <returns>The value of the Description attribute or that value as a string.</returns>
        public static string Description(this Enum type) 
        {
            var attributes = type.GetType().GetField(type.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : type.ToString();
        }
    }
}
