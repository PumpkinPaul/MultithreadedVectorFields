// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MultithreadedVectorFields.Engine.DebugTools;

public static class DebugSystem
{
    public static uint FrameId { get; private set; }

    public static DebugResources DebugResources { get; private set; }

    public static DebugCommandUI DebugCommandUI { get; private set; }

    public static GcCounter GcCounter { get; private set; }

    public static FpsCounter FpsCounter { get; private set; }

    public static TimeRuler TimeRuler { get; private set; }
    //public OutputWindow OutputWindow { get; private set; }

    public static Plotter Plotter { get; } = new Plotter();

    public static bool ResourcesChanged { get; set; }

    public static DebugSettings Settings { get; private set; }
    private static int _updateInactiveGameTicks;

    public static readonly Vector2 SmallGaugeSize = new(16, 48);
    public static readonly Vector2 MediumPlotSize = new(64, 48);
    public static readonly Vector2 LargePlotSize = new(128, 48);

    public static readonly List<Action> _actions = new();

    public static void Initialize(BaseGame game, string debugFont)
    {
        FrameId = 0;

        Settings = DebugSettings.Create();

        // Create all of the system components
        DebugResources = new DebugResources(game, debugFont);

        DebugCommandUI = new DebugCommandUI(game);

        FpsCounter = new FpsCounter(game);

        GcCounter = new GcCounter(game);

        TimeRuler = new TimeRuler(game);

        Plotter.Initialize();

#if DEBUG_TOOLS
        game.Components.Add(DebugResources);
        game.Components.Add(DebugCommandUI);
        game.Components.Add(FpsCounter);
        game.Components.Add(GcCounter);
        game.Components.Add(TimeRuler);

        FpsCounter.Visible = Settings.ShowGcCounter;
        GcCounter.Visible = Settings.ShowFpsCounter;
        TimeRuler.Visible = Settings.ShowTimeRuler;
        Plotter.Enabled = Settings.ShowPlots;
#endif
        DebugCommandUI.RegisterCommand("debug", "Toggle Debug Systems [on/off]", (commandHost, command, arguments) =>
        {
            //Get the visibility of all our systems - if any are visible we'll set the default to true.
            var visible = FpsCounter.Visible ||
                GcCounter.Visible ||
                Plotter.Enabled ||
                TimeRuler.Visible;

            if (arguments.Count == 0)
                visible = !visible;

            foreach (var arg in arguments)
            {
                switch (arg.ToLower())
                {
                    case "on":
                        visible = true;
                        break;

                    case "off":
                        visible = false;
                        break;
                }
            }

            DebugResources.Visible = visible;
            FpsCounter.Visible = visible;
            GcCounter.Visible = visible;
            TimeRuler.Visible = visible;
            Plotter.Enabled = visible;
        });
    }

    [Conditional("DEBUG_TOOLS")]
    public static void RegisterDrawing(Action action)
    {
        _actions.Add(action);
    }

    public static void BeginUpdate()
    {
        FrameId++;
        TimeRuler.BeginUpdate();
    }

    public static void Update()
    {

    }

    public static void Draw()
    {
        foreach (var action in _actions)
        {
            action.Invoke();
        }
    }

    [Conditional("DEBUG_TOOLS")]
    public static void RefreshInactiveGame(int ticks = 180)
    {
        if (_updateInactiveGameTicks > ticks)
            return;

        _updateInactiveGameTicks = ticks;
    }

    [Conditional("DEBUG_TOOLS")]
    public static void Exiting()
    {
        Settings.ShowGcCounter = FpsCounter.Visible;
        Settings.ShowFpsCounter = GcCounter.Visible;
        Settings.ShowTimeRuler = TimeRuler.Visible;
        Settings.ShowPlots = Plotter.Enabled;

        Settings.Save();
    }
}