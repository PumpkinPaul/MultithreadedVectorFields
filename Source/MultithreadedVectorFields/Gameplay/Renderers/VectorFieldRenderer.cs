// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using MultithreadedVectorFields.Gameplay.GameMaps;
using System.Collections.Generic;
using System.Linq;

namespace MultithreadedVectorFields.Gameplay.Renderers;

public sealed class VectorFieldRenderer : Renderer
{
    readonly SpriteBatch _spriteBatch;
    readonly Dictionary<Entity, VectorField> _vectorFields;

    public VectorFieldRenderer(
        World world,
        SpriteBatch spriteBatch,
        Dictionary<Entity, VectorField> vectorField) : base(world)
    {
        _spriteBatch = spriteBatch;
        _vectorFields = vectorField;
    }

    public void Draw(int idx)
    {
        if (_vectorFields.Count == 0)
            return;

        _vectorFields.Values.ElementAt(idx).Draw(_spriteBatch, (int)TileMap.MAX_COLUMNS, (int)TileMap.MAX_ROWS, VectorField.Visualizer.HeatMap);
    }
}