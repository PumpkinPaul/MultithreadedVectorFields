//-----------------------------------------------------------------------------
// KeyboardUtils.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MultithreadedVectorFields.Engine.DebugTools;

/// <summary>
/// Helper class for keyboard input.
/// </summary>
public static class KeyboardUtils
{
    /// <summary>
    /// Character pair class that holds normal character and character with
    /// shift key pressed.
    /// </summary>
    class CharPair
    {
        public CharPair(char normalChar, char? shiftChar)
        {
            NormalChar = normalChar;
            ShiftChar = shiftChar;
        }

        public readonly char NormalChar;
        public char? ShiftChar;
    }

    // key:Keys, value:CharPair
    private static readonly Dictionary<Keys, CharPair> KeyMap = new();

    private static readonly Dictionary<Keys, char> ChatPadGreenKeyMap = new();
    private static readonly Dictionary<Keys, char> ChatPadOrangeKeyMap = new();

    /// <summary>
    /// Gets a character from key information.
    /// </summary>
    /// <param name="key">Pressing key</param>
    /// <param name="shitKeyPressed">Is shift key pressed?</param>
    /// <param name="chatPadGreen">Is ChatPad green key pressed?</param>
    /// <param name="chatPadOrange">Is ChatPad orange key pressed?</param>
    /// <param name="character">Converted character from key input.</param>
    /// <returns>Returns true when it gets a character</returns>
    public static bool KeyToString(Keys key, bool shitKeyPressed, bool chatPadGreen, bool chatPadOrange, out char character)
    {
        bool result = false;
        character = ' ';
        CharPair charPair;

        if (chatPadGreen)
        {
            if (ChatPadGreenKeyMap.TryGetValue(key, out character))
                result = true;
        }
        else if (chatPadOrange)
        {
            if (ChatPadOrangeKeyMap.TryGetValue(key, out character))
                result = true;
        }
        else if (Keys.A <= key && key <= Keys.Z || key == Keys.Space)
        {
            // Use as is if it is A～Z, or Space key.
            character = shitKeyPressed ? (char)key : char.ToLower((char)key);
            result = true;
        }
        else if (KeyMap.TryGetValue(key, out charPair))
        {
            // Otherwise, convert by key map.
            if (!shitKeyPressed)
            {
                character = charPair.NormalChar;
                result = true;
            }
            else if (charPair.ShiftChar.HasValue)
            {
                character = charPair.ShiftChar.Value;
                result = true;
            }
        }

        return result;
    }

    static KeyboardUtils()
    {
        InitializeKeyMap();
        InitializeChatPadGreenKeyMap();
        InitializeChatPadOrangeKeyMap();
    }

    /// <summary>
    /// Initialize character map.
    /// </summary>
    static void InitializeKeyMap()
    {
        // First row of US keyboard.
        AddKeyMap(Keys.OemTilde, "`~");
        AddKeyMap(Keys.D1, "1!");
        AddKeyMap(Keys.D2, "2@");
        AddKeyMap(Keys.D3, "3#");
        AddKeyMap(Keys.D4, "4$");
        AddKeyMap(Keys.D5, "5%");
        AddKeyMap(Keys.D6, "6^");
        AddKeyMap(Keys.D7, "7&");
        AddKeyMap(Keys.D8, "8*");
        AddKeyMap(Keys.D9, "9(");
        AddKeyMap(Keys.D0, "0)");
        AddKeyMap(Keys.OemMinus, "-_");
        AddKeyMap(Keys.OemPlus, "=+");

        // Second row of US keyboard.
        AddKeyMap(Keys.OemOpenBrackets, "[{");
        AddKeyMap(Keys.OemCloseBrackets, "]}");
        AddKeyMap(Keys.OemPipe, "\\|");

        // Third row of US keyboard.
        AddKeyMap(Keys.OemSemicolon, ";:");
        AddKeyMap(Keys.OemQuotes, "'\"");
        AddKeyMap(Keys.OemComma, ",<");
        AddKeyMap(Keys.OemPeriod, ".>");
        AddKeyMap(Keys.OemQuestion, "/?");

        // Keypad keys of US keyboard.
        AddKeyMap(Keys.NumPad1, "1");
        AddKeyMap(Keys.NumPad2, "2");
        AddKeyMap(Keys.NumPad3, "3");
        AddKeyMap(Keys.NumPad4, "4");
        AddKeyMap(Keys.NumPad5, "5");
        AddKeyMap(Keys.NumPad6, "6");
        AddKeyMap(Keys.NumPad7, "7");
        AddKeyMap(Keys.NumPad8, "8");
        AddKeyMap(Keys.NumPad9, "9");
        AddKeyMap(Keys.NumPad0, "0");
        AddKeyMap(Keys.Add, "+");
        AddKeyMap(Keys.Divide, "/");
        AddKeyMap(Keys.Multiply, "*");
        AddKeyMap(Keys.Subtract, "-");
        AddKeyMap(Keys.Decimal, ".");
    }

    /// <summary>
    /// Initialize character map.
    /// </summary>
    static void InitializeChatPadGreenKeyMap()
    {
        // First row of ChatPad.
        ChatPadGreenKeyMap.Add(Keys.Q, '!');
        ChatPadGreenKeyMap.Add(Keys.W, '@');
        //ChatPadGreenKeyMap.Add(Keys.E, '€');
        ChatPadGreenKeyMap.Add(Keys.R, '#');
        ChatPadGreenKeyMap.Add(Keys.T, '%');
        ChatPadGreenKeyMap.Add(Keys.Y, '^');
        ChatPadGreenKeyMap.Add(Keys.U, '&');
        ChatPadGreenKeyMap.Add(Keys.I, '*');
        ChatPadGreenKeyMap.Add(Keys.O, '(');
        ChatPadGreenKeyMap.Add(Keys.P, ')');

        //Second row 
        ChatPadGreenKeyMap.Add(Keys.A, '~');
        //ChatPadGreenKeyMap.Add(Keys.S, ':');
        ChatPadGreenKeyMap.Add(Keys.D, '{');
        ChatPadGreenKeyMap.Add(Keys.F, '}');
        //ChatPadGreenKeyMap.Add(Keys.G, ':');
        ChatPadGreenKeyMap.Add(Keys.H, '/');
        ChatPadGreenKeyMap.Add(Keys.J, '\'');
        ChatPadGreenKeyMap.Add(Keys.K, '[');
        ChatPadGreenKeyMap.Add(Keys.L, ']');
        ChatPadGreenKeyMap.Add(Keys.OemComma, ':');

        //Third row
        //ChatPadGreenKeyMap.Add(Keys.Z, '');
        //ChatPadGreenKeyMap.Add(Keys.X, ':');
        //ChatPadGreenKeyMap.Add(Keys.C, ':');
        ChatPadGreenKeyMap.Add(Keys.V, '-');
        ChatPadGreenKeyMap.Add(Keys.B, '|');
        ChatPadGreenKeyMap.Add(Keys.N, '<');
        ChatPadGreenKeyMap.Add(Keys.M, '>');
        ChatPadGreenKeyMap.Add(Keys.OemPeriod, '?');
    }

    /// <summary>
    /// Initialize character map.
    /// </summary>
    static void InitializeChatPadOrangeKeyMap()
    {
        // First row of ChatPad.
        //ChatPadOrangeKeyMap.Add(Keys.Q, '!');
        //ChatPadOrangeKeyMap.Add(Keys.W, '@');
        //ChatPadOrangeKeyMap.Add(Keys.E, '€');
        ChatPadOrangeKeyMap.Add(Keys.R, '$');
        //ChatPadOrangeKeyMap.Add(Keys.T, '%');
        //ChatPadOrangeKeyMap.Add(Keys.Y, '^');
        //ChatPadOrangeKeyMap.Add(Keys.U, '&');
        //ChatPadOrangeKeyMap.Add(Keys.I, '*');
        //ChatPadOrangeKeyMap.Add(Keys.O, '(');
        ChatPadOrangeKeyMap.Add(Keys.P, '=');

        //Second row 
        //ChatPadOrangeKeyMap.Add(Keys.A, '~');
        //ChatPadOrangeKeyMap.Add(Keys.S, ':');
        //ChatPadOrangeKeyMap.Add(Keys.D, '{');
        ChatPadOrangeKeyMap.Add(Keys.F, '£');
        //ChatPadOrangeKeyMap.Add(Keys.G, ':');
        ChatPadOrangeKeyMap.Add(Keys.H, '\\');
        ChatPadOrangeKeyMap.Add(Keys.J, '"');
        //ChatPadOrangeKeyMap.Add(Keys.K, '[');
        ChatPadOrangeKeyMap.Add(Keys.L, '0');
        ChatPadOrangeKeyMap.Add(Keys.OemComma, ';');

        //Third row
        //ChatPadOrangeKeyMap.Add(Keys.Z, '');
        //ChatPadOrangeKeyMap.Add(Keys.X, ':');
        //ChatPadOrangeKeyMap.Add(Keys.C, ':');
        ChatPadOrangeKeyMap.Add(Keys.V, '_');
        ChatPadOrangeKeyMap.Add(Keys.B, '+');
        //ChatPadOrangeKeyMap.Add(Keys.N, '<');
        //ChatPadOrangeKeyMap.Add(Keys.M, '>');
        //ChatPadOrangeKeyMap.Add(Keys.OemPeriod, '?');
    }

    /// <summary>
    ///　Added key and character map.
    /// </summary>
    /// <param name="key">Keyboard key</param>
    /// <param name="charPair">
    /// Character, If it is two characters, first character is for not holding the shift key,
    /// and the second character for holding the shift key.</param>
    static void AddKeyMap(Keys key, string charPair)
    {
        char char1 = charPair[0];
        char? char2 = null;
        if (charPair.Length > 1)
            char2 = charPair[1];

        KeyMap.Add(key, new CharPair(char1, char2));
    }
}