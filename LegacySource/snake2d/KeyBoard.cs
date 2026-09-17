using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsContexts;
using OpenTK.Windowing.Input;
using OpenTK.Wpf.Input;

namespace Snake2D
{
    public class KeyBoard
    {
        private readonly OpenTK.GLFW.GLFWKeyCallback callback;
        private readonly OpenTK.GLFW.GLFWCharCallback charCallback;

        private readonly char[] chars = new char[128];
        private readonly int[] keys = new int[3 * 128];

        private volatile int charsI = 0;
        private volatile int keysI = 0;
        CHAR_LISTENER listener;
        private bool listening = false;

        private readonly List<KeyEvent> pollsA = new List<KeyEvent>(128);
        private readonly List<KeyEvent> polls = new List<KeyEvent>(128);

        public enum KEYACTION
        {
            RELEASE,
            PRESS,
            REPEAT
        }

        KeyBoard(GraphicContext w)
        {
            while (pollsA.Count < 128)
            {
                pollsA.Add(new KeyEvent());
            }

            callback = new OpenTK.GLFW.GLFWKeyCallback((window, key, scancode, action, mods) =>
            {
                if (keysI >= keys.Length - 1)
                    return;

                if (KEYCODES.Exists(key))
                {
                    if (key == KEYCODES.KEY_PRINT_SCREEN)
                    {
                        if (action == 1)
                            w.TakeScreenShot();
                        return;
                    }

                    keys[keysI] = key;
                    keys[keysI + 1] = action;
                    keys[keysI + 2] = mods;

                    keysI += 3;
                }
            });
            OpenTK.GLFW.GLFW.SetKeyCallback(w.GetWindow(), callback);

            charCallback = new OpenTK.GLFW.GLFWCharCallback((window, codepoint) =>
            {
                if (charsI >= chars.Length)
                    return;
                chars[charsI] = (char)codepoint;
                charsI++;
            });

            OpenTK.GLFW.GLFW.SetCharCallback(w.GetWindow(), charCallback);
        }

        void Poll(CORE_STATE current, bool cleared)
        {
            int pi = 0;
            for (int i = 0; i < keysI; i += 3)
            {
                KEYACTION a = (KEYACTION)keys[i + 1];
                int c = keys[i];
                if (listener != null && c != KEYCODES.KEY_ESCAPE)
                {
                    if (listener != null && (a == KEYACTION.PRESS || a == KEYACTION.REPEAT))
                    {
                        if (c == KEYCODES.KEY_ENTER)
                            listener.Enter();
                        if (c == KEYCODES.KEY_BACKSPACE)
                            listener.Backspace();
                        if (c == KEYCODES.KEY_LEFT)
                            listener.Left(OpenTK.GLFW.GLFW.GetKey(CORE.GetGraphics().GetWindow(), KEYCODES.KEY_LEFT_SHIFT) == OpenTK.GLFW.GLFW.GLFW_PRESS);
                        if (c == KEYCODES.KEY_RIGHT)
                            listener.Right(OpenTK.GLFW.GLFW.GetKey(CORE.GetGraphics().GetWindow(), KEYCODES.KEY_LEFT_SHIFT) == OpenTK.GLFW.GLFW.GLFW_PRESS);
                        if (c == KEYCODES.KEY_DELETE)
                            listener.Del();
                    }
                    if (c == KEYCODES.KEY_ENTER)
                    {
                        polls.Add(pollsA[pi++].Assign(keys[i], a, keys[i + 2]));
                    }
                }
                else
                {
                    polls.Add(pollsA[pi++].Assign(keys[i], a, keys[i + 2]));
                }
            }

            current.KeyPush(polls, cleared);

            if (listener != null)
            {
                for (int i = 0; i < charsI; i++)
                {
                    listener.AcceptChar(chars[i]);
                }
                listening = true;
            }
            else
            {
                listening = false;
            }
            Clear();
        }

        void Clear()
        {
            keysI = 0;
            charsI = 0;
            polls.Clear();
            listener = null;
        }

        void Release()
        {
            callback.Dispose();
            charCallback.Dispose();
        }

        public bool IsPressed(int code)
        {
            return CORE.GetGraphics().Focused() && listener == null && !listening && OpenTK.GLFW.GLFW.GetKey(CORE.GetGraphics().GetWindow(), code) == OpenTK.GLFW.GLFW.GLFW_PRESS;
        }

        public string Translate(int code)
        {
            return OpenTK.GLFW.GLFW.GetKeyName(code, OpenTK.GLFW.GLFW.GLFW_KEY_UNKNOWN);
        }

        public class KeyEvent
        {
            private int code;
            private KEYACTION action;
            private int mod;

            private KeyEvent()
            {
            }

            public KeyEvent Assign(int code, KEYACTION action, int mod)
            {
                this.code = code;
                this.action = action;
                this.mod = mod;
                return this;
            }

            public int Code()
            {
                return code;
            }

            public KEYACTION Action()
            {
                return action;
            }

            public int Mod()
            {
                return mod;
            }
        }
    }
}