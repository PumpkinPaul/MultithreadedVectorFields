// Copyright Pumpkin Games Ltd. All Rights Reserved.

namespace MultithreadedVectorFields.Engine;

//Indicates the relative location of an item's anchor / origin.

//    TopLeft      TopCentre       Topight
//            --------------------
//            |                  |
//            |                  |
// CentreLeft |      Centre      | CentreRight
//            |                  |
//            |                  |
//            --------------------
// BottomLeft     BottomCentre     BottomRight

public enum Alignment
{
    BottomLeft,
    CentreLeft,
    TopLeft,
    BottomCentre,
    Centre,
    TopCentre,
    BottomRight,
    CentreRight,
    TopRight,
}