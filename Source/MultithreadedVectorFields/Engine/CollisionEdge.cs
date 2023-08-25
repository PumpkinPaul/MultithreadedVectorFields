// Copyright Pumpkin Games Ltd. All Rights Reserved.

using System;

namespace MultithreadedVectorFields.Engine;

//Indicates the location of a collision in relation to the entity / actor / game object's bound box

//        TOP
//   -------------
// L |           | R
// E |           | I
// F |           | G
// T |           | H
//   ------------- T
//       Bottom

[Flags]
public enum CollisionEdge
{
    None = 0,
    Top = 1,
    Left = 2,
    Bottom = 4,
    Right = 8,
}