//-----------------------------------------------------------------------------
// Layout.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultithreadedVectorFields.Engine.DebugTools;

[Flags]
public enum Align
{
    None = 0,

    Left = 1,
    Right = 2,
    HorizontalCenter = 4,

    Top = 8,
    Bottom = 16,
    VerticalCenter = 32,

    TopLeft = Top | Left,
    TopRight = Top | Right,
    TopCenter = Top | HorizontalCenter,

    BottomLeft = Bottom | Left,
    BottomRight = Bottom | Right,
    BottomCenter = Bottom | HorizontalCenter,

    CenterLeft = VerticalCenter | Left,
    CenterRight = VerticalCenter | Right,
    Center = VerticalCenter | HorizontalCenter
}

public struct Layout
{
    public Rectangle ClientArea;
    public Rectangle SafeArea;

    public Layout(Rectangle clientArea, Rectangle safeArea)
    {
        ClientArea = clientArea;
        SafeArea = safeArea;
    }

    public Layout(Rectangle clientArea) : this(clientArea, clientArea) { }

    public Layout(Viewport viewport)
    {
        ClientArea = new Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height);
        SafeArea = viewport.TitleSafeArea;
    }

    public Vector2 Place(Vector2 size, float horizontalMargin, float verticalMargin, Align alignment)
    {
        var rc = new Rectangle(0, 0, (int)size.X, (int)size.Y);
        rc = Place(rc, horizontalMargin, verticalMargin, alignment);
        return new Vector2(rc.X, rc.Y);
    }

    public Rectangle Place(Rectangle region, float horizontalMargin, float verticalMargin, Align alignment)
    {
        if ((alignment & Align.Left) != 0)
        {
            region.X = ClientArea.X + (int)(ClientArea.Width * horizontalMargin);
        }
        else if ((alignment & Align.Right) != 0)
        {
            region.X = ClientArea.X + (int)(ClientArea.Width * (1.0f - horizontalMargin)) - region.Width;
        }
        else if ((alignment & Align.HorizontalCenter) != 0)
        {
            region.X = ClientArea.X + (ClientArea.Width - region.Width) / 2 + (int)(horizontalMargin * ClientArea.Width);
        }

        if ((alignment & Align.Top) != 0)
        {
            region.Y = ClientArea.Y + (int)(ClientArea.Height * verticalMargin);
        }
        else if ((alignment & Align.Bottom) != 0)
        {
            region.Y = ClientArea.Y + (int)(ClientArea.Height * (1.0f - verticalMargin)) - region.Height;
        }
        else if ((alignment & Align.VerticalCenter) != 0)
        {
            region.Y = ClientArea.Y + (ClientArea.Height - region.Height) / 2 + (int)(verticalMargin * ClientArea.Height);
        }

        return region;
    }
}