//-----------------------------------------------------------------------------
// DebugManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace MultithreadedVectorFields.Engine.DebugTools;

/// <summary>
/// DebugManager class that holds graphics resources for debug
/// </summary>
public class DebugResources : DrawableGameComponent
{
    private readonly string _fontName;

    public SpriteBatch SpriteBatch { get; private set; }
    public Texture2D WhiteTexture { get; private set; }

    public SpriteFont DebugFont { get; private set; }

    public StringBuilder StringBuilder { get; } = new StringBuilder();

    public Color OverlayColor = new(32, 32, 32, 210);
    //public Color OverlayColor = new Color(81, 8, 35) * 0.9f;
    public Color AccentColor = new(226, 22, 94);

    public DebugResources(Game game, string fontName) : base(game)
    {
        // Added as a Service.
        Game.Services.AddService(typeof(DebugResources), this);
        _fontName = fontName;

        // This component doesn't need be call neither update nor draw.
        Enabled = false;
        Visible = false;
    }

    protected override void LoadContent()
    {
        //Load debug content.
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        DebugFont = Game.Content.Load<SpriteFont>(_fontName);

        //Create white texture.
        WhiteTexture = new Texture2D(GraphicsDevice, 1, 1);
        WhiteTexture.SetData(new[] { Color.White });

        base.LoadContent();
    }
}