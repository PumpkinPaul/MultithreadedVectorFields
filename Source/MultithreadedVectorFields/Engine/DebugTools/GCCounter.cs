// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MultithreadedVectorFields.Engine.DebugTools;

public class GcCounter : DrawableGameComponent
{
    private const int MaxGraphFrames = 60;

    public float Collections { get; private set; }

    /// <summary>Gets/Sets GC sample duration.</summary>
    public TimeSpan SampleSpan { get; set; }

    // Reference for debug manager.
    private DebugResources _debugManager;

    // Stopwatch for GC measuring.
    private Stopwatch _stopwatch;

    private readonly WeakReference _gcTracker = new(new object());

    // stringBuilder for GC counter draw.
    private readonly StringBuilder _stringBuilder = new(128);

    private readonly float[] _tickBytes = new float[MaxGraphFrames];
    private readonly float[] _totalMegabytes = new float[MaxGraphFrames];
    private int _frameId = -1;

    private const int OneMegabyte = 1024 * 1024;
    private long _totalMemory;
    private long _previousMemory;
    private long _baseMemory;
    private long _tickMemory;

    public GcCounter(Game game) : base(game)
    {
        SampleSpan = TimeSpan.FromSeconds(1);
    }

    public override void Initialize()
    {
        // Get debug manager from game service.
        _debugManager = Game.Services.GetService(typeof(DebugResources)) as DebugResources;

        if (_debugManager == null)
            throw new InvalidOperationException("DebugManager is not registered.");

        // Register 'GC' command if debug command is registered as a service.

        if (Game.Services.GetService(typeof(IDebugCommandHost)) is IDebugCommandHost host)
        {
            host.RegisterCommand("GC", "Garbage Collection Counter [on|off|collect]", CommandExecute);
        }

        // Initialize parameters.
        Collections = 0;
        _stopwatch = Stopwatch.StartNew();
        _stringBuilder.Length = 0;

        _totalMemory = GC.GetTotalMemory(false);
        _previousMemory = _tickMemory;

        base.Initialize();
    }

    /// <summary>
    /// GC command implementation.
    /// </summary>
    private void CommandExecute(IDebugCommandHost host, string command, IList<string> arguments)
    {
        if (arguments.Count == 0)
            Visible = !Visible;

        foreach (var arg in arguments)
        {
            switch (arg.ToLower())
            {
                case "on":
                    Visible = true;
                    break;
                case "off":
                    Visible = false;
                    break;

                case "collect":
                    for (var i = 0; i < GC.MaxGeneration; ++i)
                        GC.Collect(0, GCCollectionMode.Forced);
                    break;

                case "/?":
                case "--help":
                    host.Echo("gc [on|off|collect]");
                    host.Echo("Options:");
                    host.Echo("       on     Display GC.");
                    host.Echo("       off    Hide GC.");
                    host.Echo("       collect  forces collection of all generations.");
                    break;
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        _totalMemory = GC.GetTotalMemory(false);
        _tickMemory = Math.Max(0, _totalMemory - _previousMemory);
        _baseMemory += _tickMemory;
        _previousMemory = _totalMemory;

        _frameId = (_frameId + 1) % MaxGraphFrames;

        _tickBytes[_frameId] = _tickMemory;
        _totalMegabytes[_frameId] = _totalMemory / 1024.0f / 1024.0f;

        if (!_gcTracker.IsAlive)
        {
            //TODO:
            //DebugSystem.Instance.OutputWindow.Echo(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fffffff") + " : Garbage Collection", Color.Red);

            //_tickMemory = 0;
            //_baseMemory = 0;
        }

        if (_stopwatch.Elapsed <= SampleSpan)
            return;

        //Need to know if > 1mb as that is when we get a collection on the xbox
        if (_baseMemory > OneMegabyte)
        {
            //XBox collection
            //TODO:
            //DebugSystem.OutputWindow.Echo(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fffffff") + " : Xbox360 Garbage Collection", Color.Orange);
            _baseMemory = 0;
        }

        _stopwatch.Reset();
        _stopwatch.Start();

        // Update draw string.
        _stringBuilder.Length = 0;

        for (var i = 0; i <= GC.MaxGeneration; ++i)
        {
            _stringBuilder.Append("Gen ");
            _stringBuilder.AppendNumber(i);
            _stringBuilder.Append(": ");
            _stringBuilder.AppendNumber(GC.CollectionCount(i));
            if (i < GC.MaxGeneration)
                _stringBuilder.Append('\n');
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = _debugManager.SpriteBatch;
        var font = _debugManager.DebugFont;

        // Compute size of border area.
        var size = font.MeasureString(_stringBuilder);
        //size.X = Math.Max(size.X, MaxGraphFrames * AllocationBarWidth);
        var rc = new Rectangle(0, BaseGame.Instance.Window.ClientBounds.Height - 60, (int)size.X + 0, (int)size.Y + 0);

        var layout = new Layout(spriteBatch.GraphicsDevice.Viewport);
        rc = layout.Place(rc, 0.01f, 0.05f, Align.TopRight);

        // Place GC string in border area.
        size = font.MeasureString(_stringBuilder);
        layout.ClientArea = rc;
        var pos = layout.Place(size, 0.03f, 0, Align.CenterLeft);

        spriteBatch.Begin();

        //Draw bg and output text
        spriteBatch.Draw(_debugManager.WhiteTexture, rc, DebugSystem.DebugResources.OverlayColor);
        spriteBatch.DrawString(font, _stringBuilder, pos, Color.White);

        var y = BaseGame.Instance.Window.ClientBounds.Height - 60;
        DebugSystem.Plotter.DrawLines("Memory Tick (Bytes)", _tickBytes, new Vector2(8, y), new Vector2(128, 48));
        DebugSystem.Plotter.DrawLines("Total (MB)", _totalMegabytes, new Vector2(144, y), new Vector2(64, 48), 2);
        DebugSystem.Plotter.DrawGauge("Xbox", new Vector2(216, y), new Vector2(16, 48), MathHelper.Clamp(_baseMemory / (float)OneMegabyte, 0.0f, 1.0f));

        spriteBatch.End();

        base.Draw(gameTime);
    }
}