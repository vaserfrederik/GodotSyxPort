using System;
using OpenTK.Windowing.GraphicsContexts;
using OpenTK.Windowing.Input;
using OpenTK.Windowing.Native;

namespace Snake2D
{
    public class Input : CORE_RESOURCE
    {
        private readonly Mouse mouse;
        private readonly KeyBoard keyboard;
        static int inputs;
        static long nanoNow;

        private bool hasCleared;

        public Input(GraphicContext window, SETTINGS sett)
        {
            keyboard = new KeyBoard(window);
            mouse = new Mouse(window.Handle);
            mouse.ApplySettings(sett);
            mouse.Update();
            inputs = 0;
        }

        public Mouse GetMouse()
        {
            return mouse;
        }

        public KeyBoard GetKeyboard()
        {
            return keyboard;
        }

        public void ClearAllInput()
        {
            hasCleared = true;
            CORE.GetInput().keyboard.Listener = null;
            mouse.Clear();
            keyboard.Clear();
            inputs = 0;
        }

        void Poll(long nanoNow, bool focused)
        {
            MButt.WheelDy = 0;

            GLFW.PollEvents();
            if (!focused)
                ClearAllInput();
            Input.nanoNow = nanoNow;
        }

        void Poll(CORE_STATE current)
        {
            mouse.Poll(current);
            keyboard.Poll(current, hasCleared);
            hasCleared = false;
        }

        public override void Dis()
        {
            // keyboard.Release();
            // mouse.Release();
        }

        public abstract class CHAR_LISTENER
        {
            private readonly Str text;

            public CHAR_LISTENER(int size)
            {
                text = new Str(size);
            }

            protected void AcceptChar(char c)
            {
                if (Text().SpaceLeft() > 0 && Listening())
                {
                    Text().Add(c);
                    Change();
                }
            }

            protected void Enter()
            {
            }

            protected void Backspace()
            {
                if (Text().Length() > 0 && Listening())
                {
                    Text().ClearLast();
                    Change();
                }
            }

            public void Del()
            {
                Text().Clear();
                Change();
            }

            public void Set(CharSequence name)
            {
                Text().Clear();
                Text().Add(name);
                Change();
            }

            protected abstract void Change();

            public Str Text()
            {
                return text;
            }

            public void Listen()
            {
                CORE.GetInput().keyboard.Listener = this;
            }

            public bool Listening()
            {
                return CORE.GetInput().keyboard.Listener == this;
            }

            public void Left(bool mod)
            {
                // TODO Auto-generated method stub
            }

            public void Right(bool mod)
            {
                // TODO Auto-generated method stub
            }
        }
    }
}