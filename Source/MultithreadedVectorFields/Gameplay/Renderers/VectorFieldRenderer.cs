// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using MultithreadedVectorFields.Gameplay.GameMaps;
using System.Collections.Generic;
using System.Linq;

namespace MultithreadedVectorFields.Gameplay.Renderers;

public sealed class VectorFieldRenderer : Renderer
{
    readonly Dictionary<Entity, VectorField> _vectorFields;
    readonly VectorFieldVisualizer _vectorFieldVisualizer;

    public VectorFieldRenderer(
        World world,
        SpriteBatch spriteBatch,
        Dictionary<Entity, VectorField> vectorField) : base(world)
    {
        _vectorFields = vectorField;

        _vectorFieldVisualizer = new VectorFieldVisualizer(spriteBatch);
    }

    public void Draw(int idx)
    {
        if (_vectorFields.Count == 0)
            return;

        var vectorField = _vectorFields.Values.ElementAt(idx);
        _vectorFieldVisualizer.Draw(vectorField, (int)TileMap.MAX_COLUMNS, (int)TileMap.MAX_ROWS);
    }
}