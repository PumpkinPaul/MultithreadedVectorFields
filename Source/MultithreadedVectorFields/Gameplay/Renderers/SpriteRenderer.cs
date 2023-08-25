// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using MultithreadedVectorFields.Engine;
using MultithreadedVectorFields.Engine.Extensions;
using MultithreadedVectorFields.Gameplay.Components;

namespace MultithreadedVectorFields.Gameplay.Renderers;

public class SpriteRenderer : Renderer
{
    readonly SpriteBatch _spriteBatch;
    readonly Filter _filter;

    public SpriteRenderer(
        World world,
        SpriteBatch spriteBatch
    ) : base(world)
    {
        _spriteBatch = spriteBatch;

        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Include<ScaleComponent>()
            .Include<ColorComponent>()
            .Build();
    }

    public void Draw()
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var position = ref Get<PositionComponent>(entity);
            ref readonly var scale = ref Get<ScaleComponent>(entity);
            ref readonly var color = ref Get<ColorComponent>(entity);

            var halfSize = scale.Value / 2;
            var bounds = new BoxF(-halfSize, scale.Value);
            _spriteBatch.DrawFilledBox(position.Value, bounds, color.Value);
        }
    }
}