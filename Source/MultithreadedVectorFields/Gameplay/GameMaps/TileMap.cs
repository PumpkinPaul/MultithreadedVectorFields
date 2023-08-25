// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MultithreadedVectorFields.Engine;
using System;
using System.Collections.Generic;

namespace MultithreadedVectorFields.Gameplay.GameMaps;

/// <summary>
/// Represents an abstract concept for a 2d tile map with an extra 'height' dimension.
/// </summary>
/// <remarks>
/// Implementations are required to add features like rendering and tile types (the values in _tile data) to describe the map.
/// It will assume a tile type of 0 is solid for collisions and line of sight queries
/// </remarks>
public abstract class TileMap
{
    protected const int SOLID_TILE = 0;

    //Zero based tile data for each tile is stored in [row][cell] order.

    //TileMap starts at bottom, left and positive y = up (so _cells[0] is bottom row and _cells[0][0] is bottom, left)
    // 
    //                        MAX_COLUMNS - 1, MAX_ROWS- 1
    //  ---------------------X
    //  |                    |
    //  |                    |
    //  |                    |
    //  |                    |
    //  ----------------------
    //0,0

    //Therefore to check the tile above the player (x, y + 1)

    public const int TILE_SIZE = 16;

    //Maximum allowed size of the map
    public const uint MAX_ROWS = 32;
    public const uint MAX_COLUMNS = 32;

    //The data in the tiles array determines the height of a tile and its type
    //The higher 4 bits represent the level
    //The lower 4 bits represent the type of tile (solid, walkable)
    //So a hex value of 0x31 translates to...

    //8421 8421
    //0011 0001
    //  =3   =1
    //..tile on layer 3 that is walkable
    protected readonly int[][] _tiles; //[row][column]

    //Gets / sets the actual number of rows and columns currently utilised. 
    //Maps have a maximum size but not all levels will use all the available space. 
    //e.g. some of the earlier levels will only use a small proportion of the available space.
    // 
    //  ----------------------
    //  |                    |
    //  |1111111X UtiliseColumnCount - 1, UtilisedRowCount - 1
    //  |11111111            |
    //  |11111111            |
    //  ----------------------
    public uint UtilisedRowCount { get; protected set; } = MAX_ROWS;
    public uint UtilisedColumnCount { get; protected set; } = MAX_COLUMNS;


    protected TileMap()
    {
        _tiles = new int[MAX_ROWS][];
        for (var r = 0; r < MAX_ROWS; r++)
            _tiles[r] = new int[MAX_COLUMNS];
    }

    public int GetTileLayer(Vector2 worldPosition)
    {
        var tileCoords = GetTileCoordinates(worldPosition);
        var tileId = _tiles[tileCoords.Y][tileCoords.X];
        return GetTileLayer(tileId);
    }

    public int GetTileLayer(int colId, int rowId)
    {
        var tileId = _tiles[rowId][colId];
        return GetTileLayer(tileId);
    }

    public static int GetTileLayer(int tileId)
    {
        var sign = Math.Sign(tileId);
        return sign * (Math.Abs(tileId) >> 4);
    }

    public static int GetTileMask(int tileId) => tileId & 0xf;

    //----------------------------------------------------------------------------------------------------
    //The following routines simply check the actual map to see if the tile is solid
    //We do not care about the source layer here

    /// <summary>Gets the tile coordinates (colId, rowId) from a position in worldSpace.</summary>
    public static Point GetTileCoordinates(Vector2 worldPosition)
    {
        return new Point
        {
            X = (int)(worldPosition.X / TILE_SIZE),
            Y = (int)(worldPosition.Y / TILE_SIZE)
        };
    }

    /// <summary>Gets the bounding rectangle of a tile in world space from tile coords.</summary>        
    public static BoxF GetTileWorldBounds(Point coordinates) => GetTileWorldBounds(coordinates.X, coordinates.Y);

    /// <summary>Gets the bounding rectangle of a tile in world space from tile coords.</summary>        
    public static BoxF GetTileWorldBounds(int colId, int rowId) => new(colId * TILE_SIZE, rowId * TILE_SIZE, TILE_SIZE, TILE_SIZE);


    public bool IsTileSolid(Point coordinates)
    {
        return IsTileSolid(coordinates.X, coordinates.Y);
    }

    public bool IsTileSolid(Vector2 worldPosition)
    {
        var tileCoords = GetTileCoordinates(worldPosition);

        return IsTileSolid(tileCoords.X, tileCoords.Y);
    }

    public bool IsTileSolid(int colId, int rowId)
    {
        // Prevent escaping past the level ends.
        if (colId < 0 || colId >= UtilisedColumnCount)
            return true;

        if (rowId < 0 || rowId >= UtilisedRowCount)
            return true;

        var tileId = _tiles[rowId][colId];

        return GetTileMask(tileId) == SOLID_TILE;
    }

    //----------------------------------------------------------------------------------------------------
    //The following routines determine if a tile is to be considered solid with respect to some other layer
    //You would use these for line of sight or collision detection for an entity

    public bool IsTileSolid(Vector2 worldPosition, uint mapLayer)
    {
        var tileCoords = GetTileCoordinates(worldPosition);
        return IsTileSolid(tileCoords.X, tileCoords.Y, mapLayer);
    }

    public bool IsTileSolid(Point coordinates, uint mapLayer) => IsTileSolid(coordinates.X, coordinates.Y, mapLayer);

    public bool IsTileSolid(int colId, int rowId, uint mapLayer)
    {
        // Prevent escaping past the level ends.
        if (colId < 0 || colId >= UtilisedColumnCount)
            return true;

        if (rowId < 0 || rowId >= UtilisedRowCount)
            return true;

        var tileId = _tiles[rowId][colId];

        //Get the tile layer - if there is a difference of more than one then the tile is considered to be solid with 
        //respect to the source mapLayer
        var layer = GetTileLayer(tileId);
        layer = Math.Abs(layer);

        return Math.Abs(layer - mapLayer) > 1 || GetTileMask(tileId) == SOLID_TILE;
    }

    //----------------------------------------------------------------------------------------------------

    public int GetTileType(int colId, int rowId) => _tiles[rowId][colId];

    public void SetTileType(Vector2 worldPosition, int tileType)
    {
        var tileCoords = GetTileCoordinates(worldPosition);
        _tiles[tileCoords.Y][tileCoords.X] = tileType;
    }
}