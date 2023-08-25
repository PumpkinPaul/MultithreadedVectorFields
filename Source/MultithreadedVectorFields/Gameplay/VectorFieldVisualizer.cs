// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultithreadedVectorFields.Engine;
using MultithreadedVectorFields.Engine.Extensions;
using MultithreadedVectorFields.Gameplay.GameMaps;
using System;
using System.Text;

namespace MultithreadedVectorFields.Gameplay;

public sealed class VectorFieldVisualizer
{
    public enum VisualizerType
    {
        Cost,
        Integration,
        FlowField,
        HeatMap
    }

    public VisualizerType Visualizer { get; set; } = VisualizerType.HeatMap;

    readonly SpriteBatch _spriteBatch;
    readonly StringBuilder _stringBuilder = new();

    public VectorFieldVisualizer(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
    }

    public void Draw(VectorField vectorField, int width, int height)
    {
        switch (Visualizer)
        {
            case VisualizerType.Cost:
                RenderCost(vectorField, width, height);
                break;

            case VisualizerType.Integration:
                RenderIntegration(vectorField, width, height);
                break;

            case VisualizerType.FlowField:
                RenderFlowField(vectorField, width, height);
                break;

            case VisualizerType.HeatMap:
            default:
                RenderHeatMap(vectorField, width, height);
                break;
        }
    }

    void RenderCost(VectorField vectorField, int width, int height)
    {
        for (var xi = 0; xi < width; xi++)
        {
            for (var yi = 0; yi < height; yi++)
            {
                _stringBuilder.Clear();
                _stringBuilder.AppendNumber(vectorField.GetCost(xi, yi));
                _spriteBatch.DrawText(Resources.SmallFont, _stringBuilder, new Vector2(TileMap.TILE_SIZE * xi, TileMap.TILE_SIZE * yi + 1), Color.White);
            }
        }
    }

    void RenderIntegration(VectorField vectorField, int width, int height)
    {
        for (var xi = 0; xi < width; xi++)
        {
            for (var yi = 0; yi < height; yi++)
            {
                if (vectorField.GetIntegration(xi, yi) < ushort.MaxValue)
                {
                    _stringBuilder.Clear();
                    _stringBuilder.AppendNumber(vectorField.GetIntegration(xi, yi));
                    _spriteBatch.DrawText(Resources.SmallFont, _stringBuilder, new Vector2(TileMap.TILE_SIZE * xi, TileMap.TILE_SIZE * yi + 1), Color.Yellow);
                }
            }
        }
    }

    void RenderFlowField(VectorField vectorField, int width, int height)
    {
        for (var xi = 0; xi < width; xi++)
        {
            for (var yi = 0; yi < height; yi++)
            {
                if (vectorField.GetIntegration(xi, yi) == ushort.MaxValue)
                    continue;

                var p = new Vector2(7 + (TileMap.TILE_SIZE * xi), 7 + (TileMap.TILE_SIZE * yi));
                _spriteBatch.DrawPoint(p, Color.Red, 4);

                p = new Vector2(8 + (TileMap.TILE_SIZE * xi), 8 + (TileMap.TILE_SIZE * yi));

                _spriteBatch.DrawLine(p, p + vectorField.GetFlow(xi, yi) * 6, Color.White);
            }
        }
    }

    void RenderHeatMap(VectorField vectorField, int width, int height)
    {
        var maxHeat = 0;
        for (var xi = 0; xi < width; xi++)
        {
            for (var yi = 0; yi < height; yi++)
            {
                var value = vectorField.GetIntegration(xi, yi);

                if (value >= byte.MaxValue)
                    continue;

                maxHeat = Math.Max(maxHeat, value);
            }
        }

        var c = Color.Yellow;
        var hsl = c.ToHsl();
        var yellow = hsl.H;
        for (var xi = 0; xi < width; xi++)
        {
            for (var yi = 0; yi < height; yi++)
            {
                var value = vectorField.GetIntegration(xi, yi);

                if (value >= byte.MaxValue)
                    continue;

                var ratio = (value / (float)maxHeat); //0.0f .. 1.0f

                hsl.H = MathHelper.Lerp(yellow, -120, ratio);
                var heatColour = hsl.ToRgb();
                var position = new Vector2(TileMap.TILE_SIZE * xi, TileMap.TILE_SIZE * yi);

                var box = new BoxF(0, 0, TileMap.TILE_SIZE, TileMap.TILE_SIZE);
                _spriteBatch.DrawFilledBox(position, box, heatColour);
            }
        }
    }
}