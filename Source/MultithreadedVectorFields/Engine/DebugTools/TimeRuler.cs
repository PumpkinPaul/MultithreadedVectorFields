//-----------------------------------------------------------------------------
// TimeRuler.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;

namespace MultithreadedVectorFields.Engine.DebugTools;

/// <summary>
/// Realtime CPU measuring tool
/// </summary>
/// <remarks>
/// You can visually find bottle neck, and know how much you can put more CPU jobs
/// by using this tool.
/// Because of this is real time profile, you can find glitches in the game too.
/// 
/// TimeRuler provide the following features:
///  * Up to 16 bars (Configurable)
///  * Change colors for each markers
///  * Marker logging.
///  * It won't even generate BeginMark/EndMark method calls when you got rid of the
///    DEBUG_TOOLS constant.
///  * It supports up to 32 (Configurable) nested BeginMark method calls.
///  * Multithreaded safe
///  * Automatically changes display frames based on frame duration.
///  
/// How to use:
/// Added TimerRuler instance to Game.Components and call timerRuler.StartFrame in
/// top of the Game.Update method.
/// 
/// Then, surround the code that you want measure by BeginMark and EndMark.
/// 
/// timeRuler.BeginMark( "Update", Color.Blue );
/// // process that you want to measure.
/// timerRuler.EndMark( "Update" );
/// 
/// Also, you can specify bar index of marker (default value is 0)
/// 
/// timeRuler.BeginMark( 1, "Update", Color.Blue );
/// 
/// All profiling methods has CondionalAttribute with "PROFILE".
/// If you not specified "DEBUG_TOOLS" constant, it doesn't even generate
/// method calls for BeginMark/EndMark.
/// So, don't forget remove "DEBUG_TOOLS" constant when you release your game.
/// 
/// </remarks>
public class TimeRuler : DrawableGameComponent
{
    public int DrawLevels { get; set; } = 1;

    /// <summary>
    /// Max bar count.
    /// </summary>
    private const int MaxBars = 16;

    /// <summary>
    /// Maximum sample number for each bar.
    /// </summary>
    private const int MaxSamples = 2560;

    /// <summary>
    /// Maximum nest calls for each bar.
    /// </summary>
    private const int MaxNestCall = 32;

    /// <summary>
    /// Maximum display frames.
    /// </summary>
    private const int MaxSampleFrames = 4;

    /// <summary>
    /// Duration (in frame count) for take snap shot of log.
    /// </summary>
    private const int LogSnapDuration = 120;

    /// <summary>
    /// Height(in pixels) of bar.
    /// </summary>
    private const int BarHeight = 8;

    /// <summary>
    /// Padding(in pixels) of bar.
    /// </summary>
    private const int BarPadding = 2;

    /// <summary>
    /// Delay frame count for auto display frame adjustment.
    /// </summary>
    private const int AutoAdjustDelay = 30;

    /// <summary>
    /// Gets/Set log display or no.
    /// </summary>
    public bool ShowLog { get; set; } = false;

    /// <summary>
    /// Gets/Sets target sample frames.
    /// </summary>
    public int TargetSampleFrames { get; set; }

    /// <summary>
    /// Gets/Sets TimeRuler rendering position.
    /// </summary>
    public Vector2 Position = new(8, 8);

    /// <summary>
    /// Gets/Sets timer ruler width.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Marker structure.
    /// </summary>
    private struct Marker
    {
        public int MarkerId;
        public float BeginTime;
        public float EndTime;
        public Color Color;
    }

    /// <summary>
    /// Collection of markers.
    /// </summary>
    private class MarkerCollection
    {
        // Marker collection.
        public readonly Marker[] Markers = new Marker[MaxSamples];
        public int MarkCount;

        // Marker nest information.
        public readonly int[] MarkerNests = new int[MaxNestCall];
        public int NestCount;
    }

    /// <summary>
    /// Frame logging information.
    /// </summary>
    private class FrameLog
    {
        public readonly MarkerCollection[] Bars;

        public FrameLog()
        {
            // Initialize markers.
            Bars = new MarkerCollection[MaxBars];
            for (var i = 0; i < MaxBars; ++i)
                Bars[i] = new MarkerCollection();
        }
    }

    /// <summary>
    /// Marker information
    /// </summary>
    private class MarkerInfo
    {
        // Name of marker.
        public readonly string Name;

        // Marker log.
        public readonly MarkerLog[] Logs = new MarkerLog[MaxBars];

        public MarkerInfo(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Marker log information.
    /// </summary>
    private struct MarkerLog
    {
        public float SnapAvg;

        public float Min;
        public float Max;
        public float Avg;

        public int Samples;

        public Color Color;

        public bool Initialized;
    }

    // Reference of debug manager.
    private DebugResources _debugManager;

    // Logs for each frames.
    private FrameLog[] _logs;

    // Previous frame log.
    private FrameLog _prevLog;

    // Current log.
    private FrameLog _curLog;

    // Current frame count.
    private int _frameCount;

    // Stopwatch for measure the time.
    private readonly Stopwatch _stopwatch = new();

    // Marker information array.
    private readonly List<MarkerInfo> _markers = new();

    // Dictionary that maps from marker name to marker id.
    private readonly Dictionary<string, int> _markerNameToIdMap = new();

    // Display frame adjust counter.
    private int _frameAdjust;

    // Current display frame count.
    private int _sampleFrames;

    // Marker log string.
    private readonly StringBuilder _logString = new(512);

    // You want to call StartFrame at beginning of Game.Update method.
    // But Game.Update gets calls multiple time when game runs slow in fixed time step mode.
    // In this case, we should ignore StartFrame call.
    // To do this, we just keep tracking of number of StartFrame calls until Draw gets called.
    private int _updateCount;

    // Auto generating levels
    private int _currentLevel = -1;
    private readonly Dictionary<string, int> _nameMap = new();

    private readonly object _frameLock = new();

    public TimeRuler(Game game) : base(game) { }

    public override void Initialize()
    {
#if DEBUG_TOOLS
        _debugManager = Game.Services.GetService(typeof(DebugResources)) as DebugResources;

        if (_debugManager == null)
            throw new InvalidOperationException("DebugManager is not registered.");

        // Add "tr" command if DebugCommandHost is registered.
        if (Game.Services.GetService(typeof(IDebugCommandHost)) is IDebugCommandHost host)
        {
            host.RegisterCommand("tr", "Toggles TimeRuler", CommandExecute);
            //Visible = true;
        }

        // Initialize Parameters.
        _logs = new FrameLog[2];
        for (var i = 0; i < _logs.Length; ++i)
            _logs[i] = new FrameLog();

        _sampleFrames = TargetSampleFrames = 1;

        // Time-Ruler's update method doesn't need to get called.
        Enabled = false;
#endif
        base.Initialize();
    }

    protected override void LoadContent()
    {
        Width = GraphicsDevice.Viewport.Width - 16;

        base.LoadContent();
    }

    /// <summary>
    /// 'tr' command execution.
    /// </summary>
    public void CommandExecute(IDebugCommandHost host, string command, IList<string> arguments)
    {
        var previousVisible = Visible;

        if (arguments.Count == 0)
            Visible = !Visible;

        var subArgSeparator = new[] { ':' };
        foreach (var orgArg in arguments)
        {
            var arg = orgArg.ToLower();
            var subargs = arg.Split(subArgSeparator);
            switch (subargs[0])
            {
                case "on":
                    Visible = true;
                    break;
                case "off":
                    Visible = false;
                    break;
                case "reset":
                    ResetLog();
                    break;
                case "log":
                    if (subargs.Length > 1)
                    {
                        if (string.CompareOrdinal(subargs[1], "on") == 0)
                            ShowLog = true;
                        if (string.CompareOrdinal(subargs[1], "off") == 0)
                            ShowLog = false;
                    }
                    else
                    {
                        ShowLog = !ShowLog;
                    }
                    break;
                case "frame":
                    var a = int.Parse(subargs[1]);
                    a = Math.Max(a, 1);
                    a = Math.Min(a, MaxSampleFrames);
                    TargetSampleFrames = a;
                    break;
                case "levels":
                    var l = int.Parse(subargs[1]);
                    l = (int)MathHelper.Clamp(l, -1, MaxBars);
                    DrawLevels = l;
                    break;
                case "/?":
                case "help":
                    host.Echo("tr [log|on|off|reset|frame|levels]");
                    host.Echo("Options:");
                    host.Echo("       on     Display TimeRuler.");
                    host.Echo("       off    Hide TimeRuler.");
                    host.Echo("       log    Show/Hide marker log.");
                    host.Echo("       reset  Reset marker log.");
                    host.Echo("       frame:sampleFrames");
                    host.Echo("              Change target sample frame count");
                    host.Echo("       levels:drawLevels");
                    host.Echo("              Amount of levels to draw (-1 for all)");
                    break;
            }
        }

        // Reset update count when Visible state changed.
        if (Visible != previousVisible)
        {
            Interlocked.Exchange(ref _updateCount, 0);
        }
    }

    /// <summary>
    /// Start new frame.
    /// </summary>
    [Conditional("DEBUG_TOOLS")]
    public void BeginUpdate()
    {
        lock (_frameLock)
        {
            // We skip reset frame when this method gets called multiple times.
            var count = Interlocked.Increment(ref _updateCount);
            if (Visible && 1 < count && count < MaxSampleFrames)
                return;

            // Update current frame log.
            _prevLog = _logs[_frameCount++ & 0x1];
            _curLog = _logs[_frameCount & 0x1];

            var endFrameTime = (float)_stopwatch.Elapsed.TotalMilliseconds;

            // Update marker and create a log.
            for (var barIdx = 0; barIdx < _prevLog.Bars.Length; ++barIdx)
            {
                var prevBar = _prevLog.Bars[barIdx];
                var nextBar = _curLog.Bars[barIdx];

                // Re-open marker that didn't get called EndMark in previous frame.
                for (var nest = 0; nest < prevBar.NestCount; ++nest)
                {
                    var markerIdx = prevBar.MarkerNests[nest];

                    prevBar.Markers[markerIdx].EndTime = endFrameTime;

                    nextBar.MarkerNests[nest] = nest;
                    nextBar.Markers[nest].MarkerId = prevBar.Markers[markerIdx].MarkerId;
                    nextBar.Markers[nest].BeginTime = 0;
                    nextBar.Markers[nest].EndTime = -1;
                    nextBar.Markers[nest].Color = prevBar.Markers[markerIdx].Color;
                }

                // Update marker log.
                for (var markerIdx = 0; markerIdx < prevBar.MarkCount; ++markerIdx)
                {
                    var duration = prevBar.Markers[markerIdx].EndTime - prevBar.Markers[markerIdx].BeginTime;

                    var markerId = prevBar.Markers[markerIdx].MarkerId;
                    var m = _markers[markerId];

                    m.Logs[barIdx].Color = prevBar.Markers[markerIdx].Color;

                    if (!m.Logs[barIdx].Initialized)
                    {
                        // First frame process.
                        m.Logs[barIdx].Min = duration;
                        m.Logs[barIdx].Max = duration;
                        m.Logs[barIdx].Avg = duration;

                        m.Logs[barIdx].Initialized = true;
                    }
                    else
                    {
                        // Process after first frame.
                        m.Logs[barIdx].Min = Math.Min(m.Logs[barIdx].Min, duration);
                        m.Logs[barIdx].Max = Math.Min(m.Logs[barIdx].Max, duration);
                        m.Logs[barIdx].Avg += duration;
                        m.Logs[barIdx].Avg *= 0.5f;

                        if (m.Logs[barIdx].Samples++ >= LogSnapDuration)
                        {
                            m.Logs[barIdx].SnapAvg = m.Logs[barIdx].Avg;
                            m.Logs[barIdx].Samples = 0;
                        }
                    }
                }

                nextBar.MarkCount = prevBar.NestCount;
                nextBar.NestCount = prevBar.NestCount;
            }

            // Start measuring.
            _stopwatch.Reset();
            _stopwatch.Start();
        }
    }

    [Conditional("DEBUG_TOOLS")]
    public void BeginMark(string markerName, Color color)
    {
        lock (_frameLock)
        {
            //Look up the name in map or create a new level if this is a new name

            int levelIndex;
            if (_nameMap.ContainsKey(markerName))
            {
                levelIndex = _nameMap[markerName];
            }
            else
            {
                _currentLevel++;
                levelIndex = _currentLevel;
                _nameMap[markerName] = levelIndex;
            }

            BeginMark(levelIndex, markerName, color);
        }
    }

    /// <summary>
    /// Start measure time.
    /// </summary>
    /// <param name="barIndex">index of bar</param>
    /// <param name="markerName">name of marker.</param>
    /// <param name="color">color</param>
    [Conditional("DEBUG_TOOLS")]
    public void BeginMark(int barIndex, string markerName, Color color)
    {
        lock (_frameLock)
        {
            if (barIndex < 0 || barIndex >= MaxBars)
                throw new ArgumentOutOfRangeException(nameof(barIndex));

            var bar = _curLog.Bars[barIndex];

            if (bar.MarkCount >= MaxSamples)
            {
                throw new OverflowException("Exceeded sample count.\n Either set larger number to TimeRuler.MaxSmpale or lower sample count.");
            }

            if (bar.NestCount >= MaxNestCall)
            {
                throw new OverflowException("Exceeded nest count.\n Either set larget number to TimeRuler.MaxNestCall or lower nest calls.");
            }

            // Gets registered marker.
            if (!_markerNameToIdMap.TryGetValue(markerName, out int markerId))
            {
                // Register this if this marker is not registered.
                markerId = _markers.Count;
                _markerNameToIdMap.Add(markerName, markerId);
                _markers.Add(new MarkerInfo(markerName));
            }

            // Start measuring.
            bar.MarkerNests[bar.NestCount++] = bar.MarkCount;

            // Fill marker parameters.
            bar.Markers[bar.MarkCount].MarkerId = markerId;
            bar.Markers[bar.MarkCount].Color = color;
            bar.Markers[bar.MarkCount].BeginTime = (float)_stopwatch.Elapsed.TotalMilliseconds;

            bar.Markers[bar.MarkCount].EndTime = -1;

            bar.MarkCount++;
        }
    }

    [Conditional("DEBUG_TOOLS")]
    public void EndMark(string markerName)
    {
        lock (_frameLock)
        {
            int levelIndex;
            if (_nameMap.ContainsKey(markerName))
            {
                levelIndex = _nameMap[markerName];
            }
            else
            {
                //End called before Begin throw!
                throw new InvalidOperationException("EndMark could not find name: " + markerName + ". Ensure you call BeginMark first.");
            }

            var nestLevels = EndMark(levelIndex, markerName);
            if (nestLevels == 0)
            {
                _nameMap.Remove(markerName);
                _currentLevel--;
            }
        }
    }

    /// <summary>
    /// End measuring.
    /// </summary>
    /// <param name="barIndex">Index of bar.</param>
    /// <param name="markerName">Name of marker.</param>
    public int EndMark(int barIndex, string markerName)
    {
#if DEBUG_TOOLS
        lock (_frameLock)
        {
            if (barIndex < 0 || barIndex >= MaxBars)
                throw new ArgumentOutOfRangeException(nameof(barIndex));

            var bar = _curLog.Bars[barIndex];

            if (bar.NestCount <= 0)
            {
                throw new InvalidOperationException("Call BeingMark method before call EndMark method.");
            }

            if (!_markerNameToIdMap.TryGetValue(markerName, out int markerId))
            {
                throw new InvalidOperationException($"Maker '{markerName}' is not registered. Make sure you specifed same name as you used for BeginMark method.");
            }

            var markerIdx = bar.MarkerNests[--bar.NestCount];
            if (bar.Markers[markerIdx].MarkerId != markerId)
            {
                throw new InvalidOperationException("Incorrect call order of BeginMark/EndMark method. You call it like BeginMark(A), BeginMark(B), EndMark(B), EndMark(A)" +
                " But you can't call it like BeginMark(A), BeginMark(B), EndMark(A), EndMark(B).");
            }

            bar.Markers[markerIdx].EndTime = (float)_stopwatch.Elapsed.TotalMilliseconds;

            return bar.NestCount;
        }
#else
        return 0;
#endif
    }

    /// <summary>
    /// Reset marker log.
    /// </summary>
    [Conditional("DEBUG_TOOLS")]
    public void ResetLog()
    {
        lock (_frameLock)
        {
            foreach (var markerInfo in _markers)
            {
                for (var i = 0; i < markerInfo.Logs.Length; ++i)
                {
                    markerInfo.Logs[i].Initialized = false;
                    markerInfo.Logs[i].SnapAvg = 0;

                    markerInfo.Logs[i].Min = 0;
                    markerInfo.Logs[i].Max = 0;
                    markerInfo.Logs[i].Avg = 0;

                    markerInfo.Logs[i].Samples = 0;
                }
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {

        Draw(Position, Width);
        base.Draw(gameTime);
    }

    [Conditional("DEBUG_TOOLS")]
    public void Draw(Vector2 position, int width)
    {
        // Reset update count.
        Interlocked.Exchange(ref _updateCount, 0);

        var drawLevels = DrawLevels == -1 ? MaxBars : DrawLevels;

        var spriteBatch = _debugManager.SpriteBatch;
        var font = _debugManager.DebugFont;
        var texture = _debugManager.WhiteTexture;

        // Adjust size and position based of number of bars we should draw.
        var height = 0;
        float maxTime = 0;
        for (int index = 0; index < _prevLog.Bars.Length && index < drawLevels; index++)
        {
            var bar = _prevLog.Bars[index];
            if (bar.MarkCount > 0)
            {
                height += BarHeight + BarPadding * 2;
                maxTime = Math.Max(maxTime, bar.Markers[bar.MarkCount - 1].EndTime);
            }
        }

        // Auto display frame adjustment.
        // For example, if the entire process of frame doesn't finish in less than 16.6ms
        // thin it will adjust display frame duration as 33.3ms.
        const float frameSpan = 1.0f / 60.0f * 1000f;
        var sampleSpan = _sampleFrames * frameSpan;

        if (maxTime > sampleSpan)
            _frameAdjust = Math.Max(0, _frameAdjust) + 1;
        else
            _frameAdjust = Math.Min(0, _frameAdjust) - 1;

        if (Math.Abs(_frameAdjust) > AutoAdjustDelay)
        {
            _sampleFrames = Math.Min(MaxSampleFrames, _sampleFrames);
            _sampleFrames = Math.Max(TargetSampleFrames, (int)(maxTime / frameSpan) + 1);

            _frameAdjust = 0;
        }

        // Compute factor that converts from ms to pixel.
        var msToPs = width / sampleSpan;

        // Draw start position.
        var startY = (int)position.Y;// - (height - BarHeight);

        // Current y position.
        var y = startY;

        spriteBatch.Begin();

        // Draw transparency background.
        var rc = new Rectangle((int)position.X, y, width, height);
        spriteBatch.Draw(texture, rc, DebugSystem.DebugResources.OverlayColor);

        // Draw markers for each bars.
        rc.Height = BarHeight;
        for (var index = 0; index < _prevLog.Bars.Length && index < drawLevels; index++)
        {
            var bar = _prevLog.Bars[index];
            rc.Y = y + BarPadding;
            if (bar.MarkCount > 0)
            {
                for (var j = 0; j < bar.MarkCount; ++j)
                {
                    var bt = bar.Markers[j].BeginTime;
                    var et = bar.Markers[j].EndTime;
                    var sx = (int)(position.X + bt * msToPs);
                    var ex = (int)(position.X + et * msToPs);
                    rc.X = sx;
                    rc.Width = Math.Max(ex - sx, 1);

                    spriteBatch.Draw(texture, rc, bar.Markers[j].Color);
                }
            }

            y += BarHeight + BarPadding;
        }

        // Draw grid lines.
        // Each grid represents ms.
        rc = new Rectangle((int)position.X, startY, 1, height);
        for (var t = 1.0f; t < sampleSpan; t += 1.0f)
        {
            rc.X = (int)(position.X + t * msToPs);
            spriteBatch.Draw(texture, rc, new Color(64, 64, 64));
        }

        // Draw frame grid.
        for (var i = 0; i <= _sampleFrames; ++i)
        {
            rc.X = (int)(position.X + frameSpan * i * msToPs);
            spriteBatch.Draw(texture, rc, Color.White);
        }

        // Draw log.
        if (ShowLog)
        {
            // Generate log string.
            y = rc.Y = rc.Height + 8;//startY + font.LineSpacing;
            _logString.Length = 0;
            foreach (var markerInfo in _markers)
            {
                for (var i = 0; i < MaxBars && i < drawLevels; ++i)
                {
                    if (markerInfo.Logs[i].Initialized)
                    {
                        if (_logString.Length > 0)
                            _logString.Append('\n');

                        _logString.Append(" Bar ");
                        _logString.AppendNumber(i);
                        _logString.Append(' ');
                        _logString.Append(markerInfo.Name);

                        _logString.Append(" Avg.:");
                        _logString.AppendNumber(markerInfo.Logs[i].SnapAvg);
                        _logString.Append("ms ");

                        //y += font.LineSpacing;
                    }
                }
            }

            // Compute background size and draw it.
            var size = font.MeasureString(_logString);
            rc = new Rectangle((int)position.X, y, (int)size.X + 12, (int)size.Y);
            spriteBatch.Draw(texture, rc, DebugSystem.DebugResources.OverlayColor);

            // Draw log string.
            spriteBatch.DrawString(font, _logString, new Vector2(position.X + 12, y + 1), Color.White);

            // Draw log color boxes.
            y += (int)(font.LineSpacing * 0.3f);
            rc = new Rectangle((int)position.X + 4, y, 10, 10);
            var rc2 = new Rectangle((int)position.X + 5, y + 1, 8, 8);
            foreach (var markerInfo in _markers)
            {
                for (var i = 0; i < MaxBars && i < drawLevels; ++i)
                {
                    if (markerInfo.Logs[i].Initialized)
                    {
                        rc.Y = y;
                        rc2.Y = y + 1;
                        spriteBatch.Draw(texture, rc, Color.White);
                        spriteBatch.Draw(texture, rc2, markerInfo.Logs[i].Color);

                        y += font.LineSpacing;
                    }
                }
            }
        }

        spriteBatch.End();
    }
}