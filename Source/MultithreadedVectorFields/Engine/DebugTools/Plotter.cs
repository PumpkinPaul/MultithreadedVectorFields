// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MultithreadedVectorFields.Engine.Extensions;
using System;
using System.Collections.Generic;

namespace MultithreadedVectorFields.Engine.DebugTools;

public class Plotter
{
    private class Plot
    {
        internal float Min;
        internal float Max;
    }

    public bool Enabled { get; set; }

    private readonly Dictionary<string, Plot> _plots = new();

    public void Initialize()
    {
        // Register 'GC' command if debug command is registered as a service.

        if (BaseGame.Instance.Services.GetService(typeof(IDebugCommandHost)) is IDebugCommandHost debugCommand)
        {
            Enabled = true;
            debugCommand.RegisterCommand("PLOTS", "Show Plots [on|off]", (host, command, arguments) =>
            {
                if (arguments.Count == 0)
                    Enabled = !Enabled;

                foreach (var arg in arguments)
                {
                    switch (arg.ToLower())
                    {
                        case "on":
                            Enabled = true;
                            break;
                        case "off":
                            Enabled = false;
                            break;

                        case "/?":
                        case "--help":
                            host.Echo("plots [on|off]");
                            host.Echo("Options:");
                            host.Echo("       on     Display Plots.");
                            host.Echo("       off    Hide Plots.");
                            break;
                    }
                }
            });
        }
    }

    private static void GetLimits(float[] values, out float min, out float max)
    {
        max = float.MinValue;
        min = float.MaxValue;
        foreach (var value in values)
        {
            if (value > max)
                max = value;

            if (value < min)
                min = value;
        }
    }

    public void DrawLines(string name, float[] values, Vector2 position, Vector2 size)
    {
        if (Enabled == false)
            return;

        DrawLines(name, values, position, size, 0, DebugSystem.DebugResources.AccentColor);
    }

    public void DrawLines(string name, float[] values, Vector2 position, Vector2 size, int decimalPlaces)
    {
        if (Enabled == false)
            return;

        DrawLines(name, values, position, size, decimalPlaces, DebugSystem.DebugResources.AccentColor);
    }

    public void DrawLines(string name, float[] values, Vector2 position, Vector2 size, int decimalPlaces, Color lineColor)
    {
        if (Enabled == false)
            return;

        const float labelOffsetX = 2;

        var offset = DebugSystem.FrameId + 1;

        var stringBuilder = DebugSystem.DebugResources.StringBuilder;
        var spriteBatch = DebugSystem.DebugResources.SpriteBatch;
        var font = DebugSystem.DebugResources.DebugFont;
        var whiteTexture = DebugSystem.DebugResources.WhiteTexture;

        if (_plots.ContainsKey(name) == false)
            _plots[name] = new Plot();

        var plot = _plots[name];

        var previousMax = plot.Max;

        GetLimits(values, out float actualMin, out float actualMax);

        if (Math.Abs(actualMin - actualMax) < 0.00001f)
        {
            actualMax = previousMax;
        }
        var min = plot.Min += (actualMin - plot.Min) * 0.3f;
        var max = plot.Max += (actualMax - plot.Max) * 0.3f;

        spriteBatch.Draw(whiteTexture, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y), DebugSystem.DebugResources.OverlayColor);
        spriteBatch.DrawString(font, name, position + new Vector2(0, -16), Color.White);

        // Show the min/max labels.
        stringBuilder.Length = 0;
        stringBuilder.AppendNumber(actualMin, decimalPlaces);
        spriteBatch.DrawString(font, stringBuilder, position + new Vector2(labelOffsetX, size.Y - 14), Color.White);

        stringBuilder.Length = 0;
        stringBuilder.AppendNumber(actualMax, decimalPlaces);
        spriteBatch.DrawString(font, stringBuilder, position + new Vector2(labelOffsetX, -2), Color.White);

        if (min < 0 && max > 0)
        {
            var normalValue = (0 - min) / (max - min);
            var yoffset = new Vector2 { X = 0, Y = size.Y * (1 - normalValue) };
            spriteBatch.DrawLine(position + yoffset, position + new Vector2(size.X, 0) + yoffset, new Color(230, 0, 0, 220));
        }

        var previousPoint = Vector2.Zero;
        var i = 0;
        while (i < values.Length)
        {
            var value = values[(offset + i) % values.Length];
            var normalValue = (value - min) / (max - min);
            var point = new Vector2
            {
                X = position.X + i / ((float)values.Length - 1) * size.X,
                Y = position.Y + (size.Y - 1f) * MathHelper.Clamp(1 - normalValue, 0, 1)
            };

            if (i != 0)
            {
                spriteBatch.DrawLine(previousPoint, point, lineColor);
            }

            i++;
            previousPoint = point;
        }
    }

    public void DrawGauge(string label, Vector2 position, Vector2 size, float ratio)
    {
        if (Enabled == false)
            return;

        DrawGauge(label, position, size, ratio, DebugSystem.DebugResources.AccentColor);
    }

    public void DrawGauge(string label, Vector2 position, Vector2 size, float ratio, Color color)
    {
        if (Enabled == false)
            return;

        var spriteBatch = DebugSystem.DebugResources.SpriteBatch;
        var font = DebugSystem.DebugResources.DebugFont;
        var whiteTexture = DebugSystem.DebugResources.WhiteTexture;

        var barHeight = (int)(size.Y * ratio);
        var barBg = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        var bar = new Rectangle((int)position.X, (int)(position.Y + size.Y - barHeight), (int)size.X, barHeight);

        //var ratioColor = Color.Green;
        //if (ratio >= 0.8f)
        //    ratioColor = Color.Red;

        //else if (ratio >= 0.5f)
        //    ratioColor = Color.Orange;

        var ratioColor = color;

        spriteBatch.Draw(whiteTexture, barBg, DebugSystem.DebugResources.OverlayColor);
        spriteBatch.Draw(whiteTexture, bar, ratioColor);

        spriteBatch.DrawString(font, label, position + new Vector2(0, -16), Color.White);
    }
}