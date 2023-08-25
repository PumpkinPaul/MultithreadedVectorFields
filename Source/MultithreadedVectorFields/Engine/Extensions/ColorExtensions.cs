// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using System;

namespace MultithreadedVectorFields.Engine.Extensions;

/// <summary>
/// Extension methods for Colors.
/// </summary>
public static class ColorExtensions
{
    public static Color ScaledRgb(this Color color, float scale)
    {
        return new Color(
            (int)(color.R * scale),
            (int)(color.G * scale),
            (int)(color.B * scale),
            color.A = color.A);
    }

    /// <summary>
    /// Converts a Color from RGB to HSL
    /// See http://en.wikipedia.org/wiki/HSL_color_space#Conversion_from_RGB_to_HSL_or_HSV for details
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static ColorHsl ToHsl(this Color color)
    {
        var r = color.R / (double)byte.MaxValue;
        var g = color.G / (double)byte.MaxValue;
        var b = color.B / (double)byte.MaxValue;

        var min = Math.Min(Math.Min(r, g), b);
        var max = Math.Max(Math.Max(r, g), b);
        var delta = max - min;

        var colorHsl = new ColorHsl
        {
            H = 0.0f,
            S = 0.0f,
            L = (float)((max + min) / 2.0f)
        };

        if (delta != 0)
        {
            if (colorHsl.L < 0.5f)
                colorHsl.S = (float)(delta / (max + min));
            else
                colorHsl.S = (float)(delta / (2.0f - max - min));

            if (r == max)
                colorHsl.H = (float)((g - b) / delta);
            else if (g == max)
                colorHsl.H = (float)(2f + (b - r) / delta);
            else if (b == max)
                colorHsl.H = (float)(4f + (r - g) / delta);
        }

        //Convert to degrees
        colorHsl.H *= 60f;
        if (colorHsl.H < 0)
            colorHsl.H += 360;

        return colorHsl;
    }
}
