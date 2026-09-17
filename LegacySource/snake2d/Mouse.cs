using System;
using System.Numerics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Snake2D
{
    public static class Mouse
    {
        public static CLICKABLE currentClicked = null;

        private static float mXC;
        private static float mYC;
        private static Vector2 mPos = new Vector2();
        private static Coo MOUSE_COO = new Coo();

        private static int clickMax = 100;
        private static MButt[] clicks = new MButt[clickMax];
        private static volatile int clickCurrent = 0;

        private static GameWindow window;

        private static MouseState previousMouseState;
        private static MouseState currentMouseState;

        static Mouse(GameWindow window)
        {
            Mouse.window = window;

            window.MouseDown += (sender, e) =>
            {
                if (e.Button > 2)
                    return;

                if (MButt.ALL[(int)e.Button].isDown = e.IsPressed)
                {
                    long nanoOld = MButt.ALL[(int)e.Button].nanoNow;
                    MButt.ALL[(int)e.Button].nanoNow = Input.nanoNow;

                    if (Input.nanoNow - nanoOld < 250000000 && clickCurrent < clickMax)
                        MButt.ALL[(int)e.Button].isDouble = true;

                    if (clickCurrent < clickMax)
                        clicks[clickCurrent++] = MButt.ALL[(int)e.Button];
                }
            };

            window.MouseWheel += (sender, e) =>
            {
                MButt.delta += e.Delta;
                MButt.wheelDy += (int)MButt.delta;
                MButt.delta -= (int)MButt.delta;

                if ((int)MButt.wheelDy != 0 && clickCurrent < clickMax)
                    clicks[clickCurrent++] = MButt.WHEEL_SPIN;
            };
        }

        void ApplySettings(SETTINGS sett)
        {
            mXC = (float)sett.getNativeWidth() / (float)CORE.getGraphics().mouseWindow.x();
            mYC = (float)sett.getNativeHeight() / (float)CORE.getGraphics().mouseWindow.y();
        }

        bool Update()
        {
            if (!CORE.getGraphics().isFocused())
                return false;

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            mPos = new Vector2(currentMouseState.X, currentMouseState.Y);
            float newX = (float)Math.Ceiling((mPos.X * mXC));
            float newY = (float)Math.Ceiling((mPos.Y * mYC));
            bool ret = false;

            if (newX >= 0 && newX <= CORE.getGraphics().nativeWidth && newY >= 0 && newY <= CORE.getGraphics().nativeHeight)
            {
                if (newY != MOUSE_COO.y() || newX != MOUSE_COO.x())
                {
                    ret = true;
                    MOUSE_COO.ySet(newY);
                    MOUSE_COO.xSet(newX);
                }
            }
            return ret;
        }

        void Poll(CORE_STATE current)
        {
            Update();
            for (MButt b : MButt.ALL)
                b.clicks = 0;

            if (clickCurrent > 0)
                currentClicked = null;

            for (int i = 0; i < clickCurrent; i++)
            {
                clicks[i].clicks++;
                current.mouseClick(clicks[i]);
            }

            for (MButt b : MButt.ALL)
                b.isDouble = false;

            clickCurrent = 0;
        }

        void Clear()
        {
            clickCurrent = 0;
            MButt.wheelDy = 0;
            for (MButt b : MButt.ALL)
            {
                b.isDouble = false;
                b.isDown = false;
                b.clicks = 0;
            }
        }

        public void ShowCursor(bool yes)
        {
            window.CursorVisible = yes;
        }

        public Coo GetCoo()
        {
            return MOUSE_COO;
        }

        public void SetMousePos(float x, float y)
        {
            window.Mouse[0] = x;
            window.Mouse[1] = y;
        }

        void Release()
        {
            // Callbacks are handled by OpenTK, no need to close manually
        }
    }
}