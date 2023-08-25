// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;

namespace MultithreadedVectorFields.Engine;

public struct ColorHsl
{
    public ColorHsl(float h, float s, float l)
    {
        H = h; S = s; L = l;
    }

    public ColorHsl(int h, byte s, byte l)
    {
        H = h / 360.0f; S = s / 100.0f; L = l / 100.0f;
    }

    public float H, S, L;

    /// <summary>
    /// Converts a Color from HSL to RGB
    /// See http://en.wikipedia.org/wiki/HSL_color_space#Conversion_from_HSL_to_RGB for details
    /// </summary>
    /// <param name="hsl"></param>
    /// <returns></returns>
    public Color ToRgb()
    {
        if (S == 0)
            return new Color(L, L, L);

        float temp2;

        if (L < 0.5f)
            temp2 = L * (1.0f + S);
        else
            temp2 = L + S - L * S;

        var temp1 = 2.0f * L - temp2;

        var h = H / 360;

        var rtemp3 = h + 1.0f / 3.0f;
        var gtemp3 = h;
        var btemp3 = h - 1.0f / 3.0f;

        if (rtemp3 < 0)
            rtemp3 += 1.0f;
        if (rtemp3 > 1)
            rtemp3 -= 1.0f;
        if (gtemp3 < 0)
            gtemp3 += 1.0f;
        if (gtemp3 > 1)
            gtemp3 -= 1.0f;
        if (btemp3 < 0)
            btemp3 += 1.0f;
        if (btemp3 > 1)
            btemp3 -= 1.0f;

        float r, g, b;

        if (6.0f * rtemp3 < 1)
            r = temp1 + (temp2 - temp1) * 6.0f * rtemp3;
        else if (2.0f * rtemp3 < 1)
            r = temp2;
        else if (3.0f * rtemp3 < 2)
            r = temp1 + (temp2 - temp1) * (2.0f / 3.0f - rtemp3) * 6.0f;
        else
            r = temp1;

        if (6.0f * gtemp3 < 1)
            g = temp1 + (temp2 - temp1) * 6.0f * gtemp3;
        else if (2.0f * gtemp3 < 1)
            g = temp2;
        else if (3.0f * gtemp3 < 2)
            g = temp1 + (temp2 - temp1) * (2.0f / 3.0f - gtemp3) * 6.0f;
        else
            g = temp1;

        if (6.0f * btemp3 < 1)
            b = temp1 + (temp2 - temp1) * 6.0f * btemp3;
        else if (2.0f * btemp3 < 1)
            b = temp2;
        else if (3.0f * btemp3 < 2)
            b = temp1 + (temp2 - temp1) * (2.0f / 3.0f - btemp3) * 6.0f;
        else
            b = temp1;

        return new Color(r, g, b);

    }
}
