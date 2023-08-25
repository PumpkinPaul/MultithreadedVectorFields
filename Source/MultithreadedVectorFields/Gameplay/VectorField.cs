// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MultithreadedVectorFields.Gameplay.GameMaps;
using System;
using System.Collections.Generic;

namespace MultithreadedVectorFields.Gameplay;

public sealed class VectorField
{
    public readonly record struct Cell(
        int X,
        int Y
    );

    static readonly Point[] CardinalNeigbours = {
        new Point( 0,  1), //up
        new Point( 1,  0), //right
        new Point( 0, -1), //down
        new Point(-1,  0), //left
    };

    static readonly Point[] AllNeigbours = {
        new Point( 0,  1), //up
        new Point( 1,  0), //right
        new Point( 0, -1), //down
        new Point(-1,  0), //left
        new Point( 1,  1),
        new Point( 1, -1),
        new Point(-1,  1),
        new Point(-1, -1)
    };

    readonly Queue<Cell> _openList = new();
    readonly byte[,] _costField;
    readonly ushort[,] _integrationField;
    readonly Vector2[,] _flowField;

    public VectorField(uint width, uint height)
    {
        _costField = new byte[width, height];
        _integrationField = new ushort[width, height];
        _flowField = new Vector2[width, height];
    }

    public byte GetCost(int x, int y) => _costField[x, y];
    public ushort GetIntegration(int x, int y) => _integrationField[x, y];
    public ref Vector2 GetFlow(int x, int y) => ref _flowField[x, y];


    public void Calculate(TileMap tileMap, int goalX, int goalY, int width, int height)
    {
        //----------------------------------------------------------------------------------------------------
        //Cost Field
        //Calculate the cost for all cells in the grid.
        //Goal cost is 0 and solid, unpassable tiles are 255
        //A clear path to goal would cost 1 so anything else (2-254) represents more difficult 'terrain'
        const byte GOAL_COST = 0;
        const byte DEFAULT_COST = 1;
        const byte BLOCKED_COST = 255;

        for (var xi = 0; xi < width; xi++)
        {
            for (var yi = 0; yi < height; yi++)
                _costField[xi, yi] = tileMap.IsTileSolid(xi, yi) ? BLOCKED_COST : DEFAULT_COST;
        }

        _costField[goalX, goalY] = GOAL_COST;

        //----------------------------------------------------------------------------------------------------
        //Integration Field
        for (var xi = 0; xi < width; xi++)
        {
            for (var yi = 0; yi < height; yi++)
                _integrationField[xi, yi] = ushort.MaxValue;
        }

        _integrationField[goalX, goalY] = _costField[goalX, goalY];

        _openList.Clear();
        _openList.Enqueue(new Cell(goalX, goalY));

        while (_openList.Count > 0)
        {
            var current = _openList.Dequeue();
            var x = current.X;
            var y = current.Y;

            for (var i = 0; i < CardinalNeigbours.Length; i++)
            {
                var neighbourX = x + CardinalNeigbours[i].X;
                var neighbourY = y + CardinalNeigbours[i].Y;

                //Calculate the new cost of the neighbour node based on the cost of the current node and the weight of the next node
                var neighbourCost = _costField[neighbourX, neighbourY];
                if (neighbourCost == BLOCKED_COST)
                    continue;

                var endNodeCost = _integrationField[x, y] + neighbourCost;

                //If a shorter path has been found, add the node into the open list
                if (endNodeCost < _integrationField[neighbourX, neighbourY])
                {
                    //Check if the neighbour cell is already in the list.
                    //If it is not then add it to the end of the list.
                    var neighbour = new Cell(neighbourX, neighbourY);
                    if (_openList.Contains(neighbour) == false)
                        _openList.Enqueue(neighbour);

                    //Set the new cost of the neighbor node.
                    _integrationField[neighbourX, neighbourY] = (ushort)endNodeCost;
                }
            }
        }

        //----------------------------------------------------------------------------------------------------
        //Flow Field
        for (var xi = 0; xi < width; xi++)
        {
            for (var yi = 0; yi < height; yi++)
                _flowField[xi, yi] = Vector2.Zero;
        }

        for (var xi = 0; xi < width; xi++)
        {
            for (var yi = 0; yi < height; yi++)
            {
                var lowestValue = _integrationField[xi, yi];
                var lowestNeighbourId = -1;

                for (var i = 0; i < AllNeigbours.Length; i++)
                {
                    var neighbourX = xi + AllNeigbours[i].X;
                    var neighbourY = yi + AllNeigbours[i].Y;

                    //Ignore tile if it would be invalid (out of bounds)
                    if (neighbourX < 0 || neighbourX >= width || neighbourY < 0 || neighbourY >= height)
                        continue;

                    var neighbourValue = _integrationField[neighbourX, neighbourY];

                    if (neighbourValue < lowestValue)
                    {
                        lowestValue = neighbourValue;
                        lowestNeighbourId = i;
                    }
                }

                if (lowestNeighbourId > -1)
                    _flowField[xi, yi] = new Vector2(AllNeigbours[lowestNeighbourId].X, AllNeigbours[lowestNeighbourId].Y);
            }
        }

        //Stop diagonal vectors pointing into impassable tiles - make them horizontal / vertical instead
        for (var xi = 0; xi < width; xi++)
        {
            for (var yi = 0; yi < height; yi++)
            {
                var flow = _flowField[xi, yi];

                if (_integrationField[xi + (int)flow.X, yi] == ushort.MaxValue)
                    _flowField[xi, yi].X = 0;

                if (_integrationField[xi, yi + (int)flow.Y] == ushort.MaxValue)
                    _flowField[xi, yi].Y = 0;
            }
        }
    }

    public Vector2 GetVectorFromWorldPosition(Vector2 position)
    {
        return _flowField[(int)position.X / TileMap.TILE_SIZE, (int)position.Y / TileMap.TILE_SIZE];
    }
}