// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MultithreadedVectorFields.Engine;
using MultithreadedVectorFields.Engine.Geometry;
using System;

namespace MultithreadedVectorFields.Engine.Collisions;

//Christer Ericson - Real-Time Collision Detection
public static class SweptAabbAabbCollision
{
    public static bool Collide(BoxF worldBounds1, Vector2 velocity1, BoxF worldBounds2, Vector2 velocity2, out float t0, out float t1)
    {
        return Collide(worldBounds1.Extents, worldBounds1.Centre, velocity1, worldBounds2.Extents, worldBounds2.Centre, velocity2, out t0, out t1);
    }

    public static bool Collide(Vector2 extents1, Vector2 centre1, Vector2 velocity1, Vector2 extents2, Vector2 centre2, Vector2 velocity2, out float t0, out float t1)
    {
        var aabb1 = new AABB2(centre1, extents1);
        var aabb2 = new AABB2(centre2, extents2);

        //Check if the boxes are currently overlapping
        if (aabb1.Overlaps(aabb2))
        {
            t0 = t1 = 0;
            return true;
        }

        //The problem is solved in A's frame of reference so get relative velocity (in normalized time)
        var relativeVelocity = velocity2 - velocity1;

        //First and last time of overlap along each axis
        var firstOverlapTime = new Vector2(float.MinValue);
        var lastOverlapTime = new Vector2(float.MaxValue);

        var overlapX = false;
        var overlapY = false;

        //Find the first possible times of overlap...
        if (relativeVelocity.X < 0)
        {
            if (aabb1.MinX < aabb2.MaxX)
            {
                overlapX = true;
                firstOverlapTime.X = (aabb1.MaxX - aabb2.MinX) / relativeVelocity.X;
            }
        }
        else if (relativeVelocity.X > 0)
        {
            if (aabb2.MinX < aabb1.MaxX)
            {
                overlapX = true;
                firstOverlapTime.X = (aabb1.MinX - aabb2.MaxX) / relativeVelocity.X;
            }
        }
        else if (aabb1.MinX <= aabb2.MaxX && aabb1.MaxX >= aabb2.MinX)
        {
            overlapX = true;
            firstOverlapTime.X = 0.0f;
            lastOverlapTime.X = 1.0f;
        }

        if (relativeVelocity.Y < 0)
        {
            if (aabb1.MinY < aabb2.MaxY)
            {
                overlapY = true;
                firstOverlapTime.Y = (aabb1.MaxY - aabb2.MinY) / relativeVelocity.Y;
            }
        }
        else if (relativeVelocity.Y > 0)
        {
            if (aabb2.MinY < aabb1.MaxY)
            {
                overlapY = true;
                firstOverlapTime.Y = (aabb1.MinY - aabb2.MaxY) / relativeVelocity.Y;
            }
        }
        else if (aabb1.MinY <= aabb2.MaxY && aabb1.MaxY >= aabb2.MinY)
        {
            overlapY = true;
            firstOverlapTime.Y = 0.0f;
            lastOverlapTime.Y = 1.0f;
        }

        //...and the last possible time of overlap
        if (aabb2.MaxX > aabb1.MinX && relativeVelocity.X < 0)
            lastOverlapTime.X = (aabb1.MinX - aabb2.MaxX) / relativeVelocity.X;
        else if (aabb1.MaxX > aabb2.MinX && relativeVelocity.X > 0)
            lastOverlapTime.X = (aabb1.MaxX - aabb2.MinX) / relativeVelocity.X;

        if (aabb2.MaxY > aabb1.MinY && relativeVelocity.Y < 0)
            lastOverlapTime.Y = (aabb1.MinY - aabb2.MaxY) / relativeVelocity.Y;
        else if (aabb1.MaxY > aabb2.MinY && relativeVelocity.Y > 0)
            lastOverlapTime.Y = (aabb1.MaxY - aabb2.MinY) / relativeVelocity.Y;

        if (overlapX == false || overlapY == false)
        {
            t0 = t1 = 0;
            return false;
        }

        //Possible first time of overlap is the largest of the axis values
        t0 = Math.Max(firstOverlapTime.X, firstOverlapTime.Y);

        //So then the possible last time of overlap is the smallest of the overlaps values
        t1 = Math.Min(lastOverlapTime.X, lastOverlapTime.Y);

        //They could have only collided if the first time of overlap occurred before the last time of overlap
        return t0 >= 0 && t0 <= 1 && t0 <= t1;
    }
}