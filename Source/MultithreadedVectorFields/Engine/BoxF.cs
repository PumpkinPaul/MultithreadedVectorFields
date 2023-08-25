// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using System;

namespace MultithreadedVectorFields.Engine;

/// <summary>
/// Represents a rectangle in 2d space.
/// </summary>
/// <remarks>
/// Use this class in a perspective type world where 
/// 
///                   +Y  
///                    |
///                    |
///                    |
///                    |0,0
///       -X --------------------- +X
///                    |
///                    |
///                    |
///                    |
///                   -Y
/// The x,y position of the box relates to the left, bottom properties. Top is y + height.
/// Hopefully this will make working in a 2d world with origin at lower left much easier than trying to retrofit the XNA Rectangle class where bottom > top.
/// </remarks>
public record struct BoxF
{
    public static readonly BoxF Empty = new();

    /// <summary>.</summary>
    public float X; //Left
    public float Y; //Bottom
    public float Width;
    public float Height;

    public readonly float Top => Y + Height;
    public readonly float Left => X;
    public readonly float Bottom => Y;
    public readonly float Right => X + Width;

    public readonly Vector2 Centre => new(X + Width / 2.0f, Y + Height / 2.0f);
    public readonly Vector2 Extents => new Vector2(Right, Top) - new Vector2(X + Width / 2.0f, Y + Height / 2.0f);

    public readonly Vector2 Min => new(Left, Bottom);
    public readonly Vector2 Max => new(Right, Top);

    /// <summary>
    /// Creates a new instance of a BoxF.
    /// </summary>
    public BoxF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Creates a new instance of a BoxF.
    /// </summary>
    public BoxF(Vector2 bottomLeft, float width, float height)
    {
        X = bottomLeft.X;
        Y = bottomLeft.Y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Creates a new instance of a BoxF.
    /// </summary>
    public BoxF(Vector2 bottomLeft, Vector2 size)
    {
        X = bottomLeft.X;
        Y = bottomLeft.Y;
        Width = size.X;
        Height = size.Y;
    }

    public readonly bool Contains(Vector2 position)
    {
        return position.X >= Left && position.X <= Right && position.Y <= Top && position.Y >= Bottom;
    }

    public readonly bool Overlaps(BoxF other)
    {
        return Left < other.Right && Right > other.Left && Top > other.Bottom && Bottom < other.Top;
    }

    public void Union(BoxF other)
    {
        var left = Math.Min(X, other.X);
        var bottom = Math.Min(Y, other.Y);

        var right = Math.Max(Right, other.Right);
        var top = Math.Max(Top, other.Top);

        X = left;
        Y = bottom;
        Width = right - left;
        Height = top - bottom;
    }

    public readonly BoxF Unioned(BoxF other)
    {
        var left = Math.Min(X, other.X);
        var bottom = Math.Min(Y, other.Y);

        var right = Math.Max(Right, other.Right);
        var top = Math.Max(Top, other.Top);

        return new BoxF(left, bottom, right - left, top - bottom);
    }

    public readonly bool Intersects(in BoxF value)
    {
        return value.Left < Right && Left < value.Right && value.Top > Bottom && Top > value.Bottom;
    }

    public static BoxF Intersect(in BoxF value1, in BoxF value2)
    {
        Intersect(in value1, in value2, out BoxF rectangle);
        return rectangle;
    }

    public static void Intersect(in BoxF value1, in BoxF value2, out BoxF result)
    {
        if (value1.Intersects(value2))
        {
            var right_side = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
            var top_side = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);

            var left_side = Math.Max(value1.X, value2.X);
            var bottom_side = Math.Max(value1.Y, value2.Y);

            result = new BoxF(left_side, bottom_side, right_side - left_side, top_side - bottom_side);
        }
        else
        {
            result = Empty;
        }
    }

    public readonly BoxF Translated(in Vector2 position) => new(position.X + X, position.Y + Y, Width, Height);

    public readonly Vector2 GetIntersectionDepth(BoxF rectB)
    {
        // Calculate half sizes.
        var halfWidthA = Width / 2.0f;
        var halfHeightA = Height / 2.0f;
        var halfWidthB = rectB.Width / 2.0f;
        var halfHeightB = rectB.Height / 2.0f;

        // Calculate centers.
        var centerA = new Vector2(X + halfWidthA, Y + halfHeightA);
        var centerB = new Vector2(rectB.X + halfWidthB, rectB.Y + halfHeightB);

        // Calculate current and minimum-non-intersecting distances between centers.
        var distanceX = centerA.X - centerB.X;
        var distanceY = centerA.Y - centerB.Y;
        var minDistanceX = halfWidthA + halfWidthB;
        var minDistanceY = halfHeightA + halfHeightB;

        // If we are not intersecting at all, return (0, 0).
        if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
            return Vector2.Zero;

        // Calculate and return intersection depths.
        var depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
        var depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;

        return new Vector2(depthX, depthY);
    }
}
