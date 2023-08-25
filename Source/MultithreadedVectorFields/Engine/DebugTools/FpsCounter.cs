// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace MultithreadedVectorFields.Engine.DebugTools;

/// <summary>
/// Component for measuring FPS.
/// </summary>
public class FpsCounter : DrawableGameComponent
{
    private const int MaxGraphFrames = 60;
    private readonly float[] _fps = new float[MaxGraphFrames];

    public float Fps { get; private set; }

    public TimeSpan SampleSpan { get; set; }

    Stopwatch _stopwatch;

    int _sampleFrames;

    public FpsCounter(Game game) : base(game)
    {
        SampleSpan = TimeSpan.FromSeconds(1);
    }

    public override void Initialize()
    {
        Fps = 0;
        _sampleFrames = 0;
        _stopwatch = Stopwatch.StartNew();

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        _fps[DebugSystem.FrameId % MaxGraphFrames] = (int)Fps;

        if (_stopwatch.Elapsed <= SampleSpan)
            return;

        // Update FPS value and start next sampling period.
        Fps = _sampleFrames / (float)_stopwatch.Elapsed.TotalSeconds;

        _stopwatch.Reset();
        _stopwatch.Start();
        _sampleFrames = 0;
    }

    public override void Draw(GameTime gameTime)
    {
        _sampleFrames++;
        var y = BaseGame.Instance.Window.ClientBounds.Height - 60;
        DebugSystem.DebugResources.SpriteBatch.Begin();
        DebugSystem.Plotter.DrawLines("FPS", _fps, new Vector2(272, y), DebugSystem.MediumPlotSize, 0, FlatTheme.BelizeHole);
        DebugSystem.DebugResources.SpriteBatch.End();
    }
}
