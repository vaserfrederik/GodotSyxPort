using System;
using System.Collections.Generic;
using System.Reflection;
using OpenTK.Input;

namespace snake2d
{
    public static class KEYCODES
    {
        private KEYCODES()
        {
        }

        public const int KEY_SPACE = Key.Space;
        public const int KEY_APOSTROPHE = Key.Quote;
        public const int KEY_COMMA = Key.Comma;
        public const int KEY_MINUS = Key.Minus;
        public const int KEY_PERIOD = Key.Period;
        public const int KEY_SLASH = Key.Slash;
        public const int KEY_0 = Key._0;
        public const int KEY_1 = Key._1;
        public const int KEY_2 = Key._2;
        public const int KEY_3 = Key._3;
        public const int KEY_4 = Key._4;
        public const int KEY_5 = Key._5;
        public const int KEY_6 = Key._6;
        public const int KEY_7 = Key._7;
        public const int KEY_8 = Key._8;
        public const int KEY_9 = Key._9;

        public static readonly int[] KEY_NUMS = new int[]
        {
            KEY_0, KEY_1, KEY_2, KEY_3, KEY_4, KEY_5, KEY_6, KEY_7, KEY_8, KEY_9
        };

        public const int KEY_SEMICOLON = Key.Semicolon;
        public const int KEY_EQUAL = Key.Equals;
        public const int KEY_A = Key.A;
        public const int KEY_B = Key.B;
        public const int KEY_C = Key.C;
        public const int KEY_D = Key.D;
        public const int KEY_E = Key.E;
        public const int KEY_F = Key.F;
        public const int KEY_G = Key.G;
        public const int KEY_H = Key.H;
        public const int KEY_I = Key.I;
        public const int KEY_J = Key.J;
        public const int KEY_K = Key.K;
        public const int KEY_L = Key.L;
        public const int KEY_M = Key.M;
        public const int KEY_N = Key.N;
        public const int KEY_O = Key.O;
        public const int KEY_P = Key.P;
        public const int KEY_Q = Key.Q;
        public const int KEY_R = Key.R;
        public const int KEY_S = Key.S;
        public const int KEY_T = Key.T;
        public const int KEY_U = Key.U;
        public const int KEY_V = Key.V;
        public const int KEY_W = Key.W;
        public const int KEY_X = Key.X;
        public const int KEY_Y = Key.Y;
        public const int KEY_Z = Key.Z;
        public const int KEY_LEFT_BRACKET = Key.BracketLeft;
        public const int KEY_BACKSLASH = Key.BackSlash;
        public const int KEY_RIGHT_BRACKET = Key.BracketRight;
        public const int KEY_GRAVE_ACCENT = Key.GraveAccent;

        public const int KEY_WORLD_2 = Key.Unknown; // No direct equivalent in OpenTK
        public const int KEY_ESCAPE = Key.Escape;
        public const int KEY_ENTER = Key.Enter;
        public const int KEY_TAB = Key.Tab;
        public const int KEY_BACKSPACE = Key.BackSpace;
        public const int KEY_INSERT = Key.Insert;
        public const int KEY_DELETE = Key.Delete;
        public const int KEY_RIGHT = Key.Right;
        public const int KEY_LEFT = Key.Left;
        public const int KEY_DOWN = Key.Down;
        public const int KEY_UP = Key.Up;
        public const int KEY_PAGE_UP = Key.PageUp;
        public const int KEY_PAGE_DOWN = Key.PageDown;
        public const int KEY_HOME = Key.Home;
        public const int KEY_END = Key.End;
        public const int KEY_CAPS_LOCK = Key.CapsLock;
        public const int KEY_SCROLL_LOCK = Key.ScrollLock;
        public const int KEY_NUM_LOCK = Key.NumLock;
        public const int KEY_PRINT_SCREEN = Key.Print;
        public const int KEY_PAUSE = Key.Pause;
        public const int KEY_F1 = Key.F1;
        public const int KEY_F2 = Key.F2;
        public const int KEY_F3 = Key.F3;
        public const int KEY_F4 = Key.F4;
        public const int KEY_F5 = Key.F5;
        public const int KEY_F6 = Key.F6;
        public const int KEY_F7 = Key.F7;
        public const int KEY_F8 = Key.F8;
        public const int KEY_F9 = Key.F9;
        public const int KEY_F10 = Key.F10;
        public const int KEY_F11 = Key.F11;
        public const int KEY_F12 = Key.F12;
        public const int KEY_F13 = Key.F13;
        public const int KEY_F14 = Key.F14;
        public const int KEY_F15 = Key.F15;
        public const int KEY_F16 = Key.F16;
        public const int KEY_F17 = Key.F17;
        public const int KEY_F18 = Key.F18;
        public const int KEY_F19 = Key.F19;
        public const int KEY_F20 = Key.F20;
        public const int KEY_F21 = Key.F21;
        public const int KEY_F22 = Key.F22;
        public const int KEY_F23 = Key.F23;
        public const int KEY_F24 = Key.F24;
        public const int KEY_F25 = Key.Unknown; // No direct equivalent in OpenTK
        public const int KEY_KP_0 = Key.KP_0;
        public const int KEY_KP_1 = Key.KP_1;
        public const int KEY_KP_2 = Key.KP_2;
        public const int KEY_KP_3 = Key.KP_3;
        public const int KEY_KP_4 = Key.KP_4;
        public const int KEY_KP_5 = Key.KP_5;
        public const int KEY_KP_6 = Key.KP_6;
        public const int KEY_KP_7 = Key.KP_7;
        public const int KEY_KP_8 = Key.KP_8;
        public const int KEY_KP_9 = Key.KP_9;
        public const int KEY_KP_DECIMAL = Key.KP_Decimal;
        public const int KEY_KP_DIVIDE = Key.KP_Divide;
        public const int KEY_KP_MULTIPLY = Key.KP_Multiply;
        public const int KEY_KP_SUBTRACT = Key.KP_Subtract;
        public const int KEY_KP_ADD = Key.KP_Add;
        public const int KEY_KP_ENTER = Key.KP_Enter;
        public const int KEY_KP_EQUAL = Key.Unknown; // No direct equivalent in OpenTK
        public const int KEY_LEFT_SHIFT = Key.ShiftLeft;
        public const int KEY_LEFT_CONTROL = Key.ControlLeft;
        public const int KEY_LEFT_ALT = Key.AltLeft;
        public const int KEY_LEFT_SUPER = Key.SuperLeft;
        public const int KEY_RIGHT_SHIFT = Key.ShiftRight;
        public const int KEY_RIGHT_CONTROL = Key.ControlRight;
        public const int KEY_RIGHT_ALT = Key.AltRight;
        public const int KEY_RIGHT_SUPER = Key.SuperRight;
        public const int KEY_MENU = Key.Menu;

        public static readonly List<int> all;
        private static readonly HashSet<int> exists;

        public static int lastCode()
        {
            return KEY_MENU;
        }

        static KEYCODES()
        {
            int am = 0;
            foreach (FieldInfo f in typeof(KEYCODES).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (f.FieldType == typeof(int))
                {
                    am++;
                }
            }

            List<int> alll = new List<int>(am);
            exists = new HashSet<int>();

            am = 0;
            foreach (FieldInfo f in typeof(KEYCODES).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (f.FieldType == typeof(int))
                {
                    try
                    {
                        int value = (int)f.GetValue(null);
                        alll.Add(value);
                        exists.Add(value);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            all = alll;
        }

        public static bool exists(int code)
        {
            return exists.Contains(code);
        }
    }
}