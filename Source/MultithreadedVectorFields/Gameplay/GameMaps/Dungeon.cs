// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultithreadedVectorFields.Engine;
using MultithreadedVectorFields.Engine.Extensions;

namespace MultithreadedVectorFields.Gameplay.GameMaps;

/// <summary>
/// Implementation of the tilemap.
/// </summary>
/// <remarks>
/// This brings our 'abstract' concecpt of a tile map to life,
/// We can add all sorts of extra things here; Dynamic Lighting, Textures, Fog of War, etc.
/// </remarks>
public class Dungeon : TileMap
{
    public Dungeon()
    {

        
    }

    public void LoadContent()
    {
        var map = BaseGame.Instance.Content.Load<Texture2D>("Maps/Map1");
        var pixelData = new Color[map.Height * map.Width];

        //Add the temp stucture to the tile array proper
        for (var r = 0; r < MAX_ROWS; r++)
        {
            for (var c = 0; c < MAX_COLUMNS; c++)
            {
                var flippedR = MAX_ROWS - r - 1;

                map.GetData(pixelData);

                var pixel = pixelData[flippedR * MAX_COLUMNS + c];
                _tiles[r][c] = pixel == Color.Black ? SOLID_TILE : SOLID_TILE + 1;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var checkerboard = false;

        var position = Vector2.Zero;
        var box = new BoxF(0, 0, TILE_SIZE, TILE_SIZE);

        //Tiles
        for (var r = 0; r < MAX_ROWS; r++)
        {
            position.X = 0;

            for (var c = 0; c < MAX_COLUMNS; c++)
            {
                //Solids...
                if (IsTileSolid(c, r))
                {
                    spriteBatch.DrawFilledBox(position, box, Color.White * 0.1f);
                }
                else
                {
                    spriteBatch.DrawFilledBox(position, box, Color.White * (checkerboard ? 0.7f : 0.75f));
                }

                checkerboard = !checkerboard;
                position.X += TILE_SIZE;
            }

            position.Y += TILE_SIZE;
            checkerboard = !checkerboard;
        }
    }
}