// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text;

namespace MultithreadedVectorFields.Engine.Extensions;

public static class SpriteBatchExtensions
{
    public static void BeginTextRendering(this SpriteBatch sb)
    {
        sb.Begin(0, null, null, null, RasterizerState.CullClockwise, null, BaseGame.Instance.TextMatrix);
    }

    public static void DrawText(this SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color, Alignment alignment = Alignment.BottomLeft)
    {
        var size = font.MeasureString(text);

        if (alignment == Alignment.Centre)
            position += new Vector2(-size.X / 2, -size.Y / 2);

        sb.DrawString(font, text, position, color, 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);
    }

    public static void DrawText(this SpriteBatch sb, SpriteFont font, StringBuilder stringBuilder, Vector2 position, Color color, Alignment alignment = Alignment.BottomLeft)
    {
        var size = font.MeasureString(stringBuilder);

        if (alignment == Alignment.Centre)
            position += new Vector2(-size.X / 2, -size.Y / 2);

        sb.DrawString(font, stringBuilder, position, color, 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);
    }

    public static void DrawLine(this SpriteBatch sb, Vector2 start, Vector2 end, Color color, int thickness)
    {
        var edge = end - start;
        var angle = (float)Math.Atan2(edge.Y, edge.X);

        var rect = new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness);

        sb.Draw(Resources.PixelTexture, rect, null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
    }

    public static void DrawLine(this SpriteBatch sb, Vector2 start, Vector2 end, Color color, float thickness = 1)
    {
        var edge = end - start;
        var angle = (float)Math.Atan2(edge.Y, edge.X);

        sb.Draw(Resources.PixelTexture, start + edge / 2.0f, null, color, angle, new Vector2(0.5f), new Vector2(edge.Length(), thickness), SpriteEffects.None, 0);
    }

    public static void DrawCircle(this SpriteBatch sb, Vector2 position, float radius, Color color, float thickness, int step = 8)
    {
        for (var i = 0; i < 360; i += step)
        {
            var start = position + VectorHelper.Polar(MathHelper.ToRadians(i), radius);
            var end = position + VectorHelper.Polar(MathHelper.ToRadians(i + step), radius);
            DrawLine(sb, start, end, color, thickness);
        }
    }

    public static void DrawFilledBox(this SpriteBatch sb, Vector2 position, BoxF bounds, Color color)
    {
        var worldBox = bounds.Translated(position);
        var rect = new Rectangle((int)worldBox.Left, (int)worldBox.Bottom, (int)worldBox.Width, (int)worldBox.Height);

        sb.Draw(Resources.PixelTexture, rect, null, color, 0, new Vector2(0, 0), SpriteEffects.None, 0);
    }

    public static void DrawPoint(this SpriteBatch sb, Vector2 centre, Color color, int size = 6)
    {
        var rect = new Rectangle((int)centre.X - size / 2, (int)centre.Y - size / 2, size, size);

        sb.Draw(Resources.PixelTexture, rect, null, color, 0, new Vector2(0, 0), SpriteEffects.None, 0);
    }

    public static void DrawBox(this SpriteBatch sb, Vector2 position, BoxF bounds, Color color)
    {
        //Box bounds
        var worldBox = bounds.Translated(position);
        DrawLine(sb, new Vector2(worldBox.Min.X, worldBox.Min.Y), new Vector2(worldBox.Min.X, worldBox.Max.Y), color, 2);
        DrawLine(sb, new Vector2(worldBox.Min.X, worldBox.Max.Y), new Vector2(worldBox.Max.X, worldBox.Max.Y), color, 2);
        DrawLine(sb, new Vector2(worldBox.Max.X, worldBox.Max.Y), new Vector2(worldBox.Max.X, worldBox.Min.Y), color, 2);
        DrawLine(sb, new Vector2(worldBox.Max.X, worldBox.Min.Y), new Vector2(worldBox.Min.X, worldBox.Min.Y), color, 2);
    }

    public static void DrawBox(this SpriteBatch sb, BoxF worldBox, Color color)
    {
        DrawLine(sb, new Vector2(worldBox.Min.X, worldBox.Min.Y), new Vector2(worldBox.Min.X, worldBox.Max.Y), color, 2);
        DrawLine(sb, new Vector2(worldBox.Min.X, worldBox.Max.Y), new Vector2(worldBox.Max.X, worldBox.Max.Y), color, 2);
        DrawLine(sb, new Vector2(worldBox.Max.X, worldBox.Max.Y), new Vector2(worldBox.Max.X, worldBox.Min.Y), color, 2);
        DrawLine(sb, new Vector2(worldBox.Max.X, worldBox.Min.Y), new Vector2(worldBox.Min.X, worldBox.Min.Y), color, 2);
    }
}