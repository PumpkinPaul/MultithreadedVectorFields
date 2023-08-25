//-----------------------------------------------------------------------------
// DebugCommandUI.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MultithreadedVectorFields.Engine.DebugTools;

/// <summary>
/// Command Window class for Debug purpose.
/// </summary>
/// <remarks>
/// Debug command UI that runs in the Game.
/// You can type commands using the keyboard, even on the Xbox
/// just connect a USB keyboard to it
/// This works on all 3 platforms (Xbox, Windows, Phone)
/// 
/// How to Use:
/// 1) Add this component to the game.
/// 2) Register command by RegisterCommand method.
/// 3) Open/Close Debug window by Tab key.
/// </remarks>
public class DebugCommandUI : DrawableGameComponent, IDebugCommandHost
{
    #region Constants

    /// <summary>
    /// Maximum lines that shows in Debug command window.
    /// </summary>
    private const int MaxLineCount = 28;

    /// <summary>
    /// Maximum command history number.
    /// </summary>
    private const int MaxCommandHistory = 32;

    /// <summary>
    /// Cursor character.
    /// </summary>
    private const string Cursor = "_";

    /// <summary>
    /// Default Prompt string.
    /// </summary>
    public const string DefaultPrompt = "CMD>";

    #endregion

    #region Properties

    /// <summary>
    /// Gets/Sets Prompt string.
    /// </summary>
    public string Prompt { get; set; }

    /// <summary>
    /// Is it waiting for key inputs?
    /// </summary>
    public bool Focused => _state != State.Closed;

    #endregion

    #region Fields

    // Command window states.
    private enum State
    {
        Closed,
        Opening,
        Opened,
        Closing
    }

    /// <summary>
    /// CommandInfo class that contains information to run the command.
    /// </summary>
    private class CommandInfo
    {
        public CommandInfo(
            string command, string description, DebugCommandExecute callback)
        {
            Command = command;
            Description = description;
            Callback = callback;
        }

        // command name
        public readonly string Command;

        // Description of command.
        public readonly string Description;

        // delegate for execute the command.
        public readonly DebugCommandExecute Callback;
    }

    // Reference to DebugManager.
    private DebugResources _debugManager;

    // Current state
    private State _state = State.Closed;

    // timer for state transition.
    private float _stateTransition;

    // Registered echo listeners.
    private readonly List<IDebugEchoListner> _listenrs = new();

    // Registered command executioner.
    private readonly Stack<IDebugCommandExecutioner> _executioners = new();

    // Registered commands
    private readonly Dictionary<string, CommandInfo> _commandTable = new();

    // Current command line string and cursor position.
    private string _commandLine = string.Empty;
    private int _cursorIndex;

    private readonly Queue<string> _lines = new();

    // Command history buffer.
    private readonly List<string> _commandHistory = new();

    // Selecting command history index.
    private int _commandHistoryIndex;

    #region variables for keyboard input handling.

    // Previous frame keyboard state.
    private readonly KeyboardState[] _prevKeyState = new KeyboardState[5];

    // Key that pressed last frame.
    private readonly Keys[] _pressedKey = new Keys[5];

    // Timer for key repeating.
    private readonly float[] _keyRepeatTimer = new float[5];

    // Key repeat duration in seconds for the first key press.
    private readonly float[] _keyRepeatStartDuration = { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };

    // Key repeat duration in seconds after the first key press.
    private const float KeyRepeatDuration = 0.06f;

    #endregion

    #endregion

    #region Initialization

    /// <summary>
    /// Constructor
    /// </summary>
    public DebugCommandUI(Game game) : base(game)
    {
        Prompt = DefaultPrompt;

        // Add this instance as a service.
        Game.Services.AddService(typeof(IDebugCommandHost), this);

        // Draw the command UI on top of everything
        DrawOrder = int.MaxValue;

        // Adding default commands.

        // Help command displays registered command information.
        RegisterCommand("help", "Show Command helps", (host, command, arguments) =>
        {
            var maxLen = 0;
            foreach (var cmd in _commandTable.Values)
                maxLen = Math.Max(maxLen, cmd.Command.Length);

            var fmt = $"{{0,-{maxLen}}}    {{1}}";

            foreach (var cmd in _commandTable.Values)
            {
                Echo(string.Format(fmt, cmd.Command, cmd.Description));
            }
        });

        // Clear screen command
        RegisterCommand("cls", "Clear Screen", (host, command, arguments) => _lines.Clear());

        // Echo command
        RegisterCommand("echo", "Display Messages", (host, command, args) => Echo(command.Substring(5)));
    }

    /// <summary>
    /// Initialize component
    /// </summary>
    public override void Initialize()
    {
        _debugManager = Game.Services.GetService(typeof(DebugResources)) as DebugResources;

        if (_debugManager == null)
            throw new InvalidOperationException("Coudn't find DebugManager.");

        base.Initialize();
    }

    #endregion

    #region IDebugCommandHostinterface implemenration

    public void RegisterCommand(
        string command, string description, DebugCommandExecute callback)
    {
        var lowerCommand = command.ToLower();
        if (_commandTable.ContainsKey(lowerCommand))
        {
            throw new InvalidOperationException($"Command \"{command}\" is already registered.");
        }

        _commandTable.Add(lowerCommand, new CommandInfo(command, description, callback));
    }

    public void UnregisterCommand(string command)
    {
        var lowerCommand = command.ToLower();
        if (!_commandTable.ContainsKey(lowerCommand))
        {
            throw new InvalidOperationException($"Command \"{command}\" is not registered.");
        }

        _ = _commandTable.Remove(command);
    }

    public void ExecuteCommand(string command)
    {
        // Call registered executioner.
        if (_executioners.Count != 0)
        {
            _executioners.Peek().ExecuteCommand(command);
            return;
        }

        // Run the command.
        var spaceChars = new[] { ' ' };

        Echo(Prompt + command);

        command = command.TrimStart(spaceChars);

        var args = new List<string>(command.Split(spaceChars));
        var cmdText = args[0];
        args.RemoveAt(0);

        if (_commandTable.TryGetValue(cmdText.ToLower(), out CommandInfo cmd))
        {
            try
            {
                // Call registered command delegate.
                cmd.Callback(this, command, args);
            }
            catch (Exception e)
            {
                // Exception occurred while running command.
                EchoError("Unhandled Exception occurred");

                var lines = e.Message.Split('\n');
                foreach (var line in lines)
                    EchoError(line);
            }
        }
        else
        {
            Echo("Unknown Command");
        }

        // Add to command history.
        _commandHistory.Add(command);
        while (_commandHistory.Count > MaxCommandHistory)
            _commandHistory.RemoveAt(0);

        _commandHistoryIndex = _commandHistory.Count;
    }

    public void RegisterEchoListner(IDebugEchoListner listner)
    {
        _listenrs.Add(listner);
    }

    public void UnregisterEchoListner(IDebugEchoListner listner)
    {
        _listenrs.Remove(listner);
    }

    public void Echo(DebugCommandMessage messageType, string text)
    {
        _lines.Enqueue(text);
        while (_lines.Count >= MaxLineCount)
            _lines.Dequeue();

        // Call registered listeners.
        foreach (var listner in _listenrs)
            listner.Echo(messageType, text);
    }

    public void Echo(string text)
    {
        Echo(DebugCommandMessage.Standard, text);
    }

    public void EchoWarning(string text)
    {
        Echo(DebugCommandMessage.Warning, text);
    }

    public void EchoError(string text)
    {
        Echo(DebugCommandMessage.Error, text);
    }

    public void PushExecutioner(IDebugCommandExecutioner executioner)
    {
        _executioners.Push(executioner);
    }

    public void PopExecutioner()
    {
        _executioners.Pop();
    }

    #endregion

    #region Update and Draw

    /// <summary>
    /// Show Debug Command window.
    /// </summary>
    public void Show()
    {
        if (_state != State.Closed)
            return;

        _stateTransition = 0.0f;
        _state = State.Opening;
    }

    /// <summary>
    /// Hide Debug Command window.
    /// </summary>
    public void Hide()
    {
        if (_state != State.Opened)
            return;

        _stateTransition = 1.0f;
        _state = State.Closing;
    }

    public override void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        const float openSpeed = 8.0f;
        const float closeSpeed = 8.0f;

        switch (_state)
        {
            case State.Opening:
                _stateTransition += dt * openSpeed;
                if (_stateTransition > 1.0f)
                {
                    _stateTransition = 1.0f;
                    _state = State.Opened;
                }
                break;

            case State.Closing:
                _stateTransition -= dt * closeSpeed;
                if (_stateTransition < 0.0f)
                {
                    _stateTransition = 0.0f;
                    _state = State.Closed;
                }
                break;
        }

        //Handle input from USB keyboard and 4 ChatPads.

        HandleKeyboardState(Keyboard.GetState(), 0, dt);

        base.Update(gameTime);
    }

    private void HandleKeyboardState(KeyboardState keyState, int index, float dt)
    {
        switch (_state)
        {
            case State.Closed:
                if (keyState.IsKeyDown(Keys.ChatPadGreen) && keyState.IsKeyUp(Keys.ChatPadOrange) && _prevKeyState[index].IsKeyDown(Keys.ChatPadOrange)
                    || keyState.IsKeyUp(Keys.OemTilde) && _prevKeyState[index].IsKeyDown(Keys.OemTilde)
                    || keyState.IsKeyUp(Keys.F1) && _prevKeyState[index].IsKeyDown(Keys.F1)
                    || keyState.IsKeyUp(Keys.F12) && _prevKeyState[index].IsKeyDown(Keys.F12))

                    Show();

                break;

            case State.Opened:
                ProcessKeyInputs(keyState, index, dt);
                break;
            case State.Opening:
                break;
            case State.Closing:
                break;
            default:
                throw new InvalidEnumArgumentException("Unknown state");
        }

        _prevKeyState[index] = keyState;
    }

    public void ProcessKeyInputs(KeyboardState keyState, int index, float dt)
    {
        var keys = keyState.GetPressedKeys();
        //var keys = KeyboardHelper.GetPressedKeys();

        var shift = keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift);
        var chatPadGreen = keyState.IsKeyDown(Keys.ChatPadGreen);
        var chatPadOrange = keyState.IsKeyDown(Keys.ChatPadOrange);

        if (keyState.IsKeyDown(Keys.ChatPadGreen) && keyState.IsKeyUp(Keys.ChatPadOrange) && _prevKeyState[index].IsKeyDown(Keys.ChatPadOrange)
                || keyState.IsKeyUp(Keys.OemTilde) && _prevKeyState[index].IsKeyDown(Keys.OemTilde)
                || keyState.IsKeyUp(Keys.F1) && _prevKeyState[index].IsKeyDown(Keys.F1)
                || keyState.IsKeyUp(Keys.F12) && _prevKeyState[index].IsKeyDown(Keys.F12))
        {
            Hide();
            return;
        }

        foreach (var key in keys)
        {
            if (key == Keys.OemTilde)
                continue;

            if (IsKeyPressed(index, key, dt) == false)
                continue;

            if (KeyboardUtils.KeyToString(key, shift, chatPadGreen, chatPadOrange, out char ch))
            {
                // Handle typical character input.
                _commandLine = _commandLine.Insert(_cursorIndex, new string(ch, 1));
                _cursorIndex++;
            }
            else
            {
                switch (key)
                {
                    case Keys.Back:
                        if (_cursorIndex > 0)
                            _commandLine = _commandLine.Remove(--_cursorIndex, 1);
                        break;
                    case Keys.Delete:
                        if (_cursorIndex < _commandLine.Length)
                            _commandLine = _commandLine.Remove(_cursorIndex, 1);
                        break;
                    case Keys.Left:
                        if (_cursorIndex > 0)
                            _cursorIndex--;
                        break;
                    case Keys.Right:
                        if (_cursorIndex < _commandLine.Length)
                            _cursorIndex++;
                        break;
                    case Keys.Enter:
                        // Run the command.
                        ExecuteCommand(_commandLine);
                        _commandLine = string.Empty;
                        _cursorIndex = 0;
                        break;
                    case Keys.Up:
                        // Show command history.
                        if (_commandHistory.Count > 0)
                        {
                            _commandHistoryIndex =
                                Math.Max(0, _commandHistoryIndex - 1);

                            _commandLine = _commandHistory[_commandHistoryIndex];
                            _cursorIndex = _commandLine.Length;
                        }
                        break;
                    case Keys.Down:
                        // Show command history.
                        if (_commandHistory.Count > 0)
                        {
                            _commandHistoryIndex = Math.Min(_commandHistory.Count - 1,
                                                            _commandHistoryIndex + 1);
                            _commandLine = _commandHistory[_commandHistoryIndex];
                            _cursorIndex = _commandLine.Length;
                        }
                        break;
                }
            }
        }
    }

    private bool IsKeyPressed(int index, Keys key, float dt)
    {
        // Treat it as pressed if given key has not pressed in previous frame.
        if (_prevKeyState[index].IsKeyUp(key))
        {
            _keyRepeatTimer[index] = _keyRepeatStartDuration[index];
            _pressedKey[index] = key;
            return true;
        }

        // Handling key repeating if given key has pressed in previous frame.
        if (key == _pressedKey[index])
        {
            _keyRepeatTimer[index] -= dt;
            if (_keyRepeatTimer[index] <= 0.0f)
            {
                _keyRepeatTimer[index] += KeyRepeatDuration;
                return true;
            }
        }

        return false;
    }

    public override void Draw(GameTime gameTime)
    {
        // Do nothing when command window is completely closed.
        if (_state == State.Closed)
            return;

        var font = _debugManager.DebugFont;
        var spriteBatch = _debugManager.SpriteBatch;
        var whiteTexture = _debugManager.WhiteTexture;

        // Compute command window size and draw.
        float w = GraphicsDevice.Viewport.Width;
        float h = GraphicsDevice.Viewport.Height;
        var topMargin = 0;//h * 0.1f;
        var leftMargin = 0;//w * 0.1f;

        var rect = new Rectangle
        {
            X = leftMargin,
            Y = topMargin,
            //Width = (int) (w * 0.8f),
            Width = (int)w,
            Height = (int)h// (MaxLineCount * font.LineSpacing)
        };

        var mtx = Matrix.CreateTranslation(new Vector3(0, -rect.Height * (1.0f - _stateTransition), 0));

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, mtx);

        spriteBatch.Draw(whiteTexture, rect, new Color(0, 0, 0, 200));

        // Draw each lines.
        var pos = new Vector2(leftMargin, topMargin);
        foreach (var line in _lines)
        {
            spriteBatch.DrawString(font, line, pos, Color.White);
            pos.Y += font.LineSpacing;
        }

        // Draw prompt string.
        var leftPart = string.Concat(Prompt, _commandLine.AsSpan(0, _cursorIndex));
        var cursorPos = pos + font.MeasureString(leftPart);
        cursorPos.Y = pos.Y;

        spriteBatch.DrawString(font,
            $"{Prompt}{_commandLine}", pos, Color.White);
        spriteBatch.DrawString(font, Cursor, cursorPos, Color.White);

        spriteBatch.End();
    }

    #endregion

}