using System;
using init.constant;
using snake2d;
using snake2d.util.datatypes;
using util.gui.misc;

namespace view.main
{
    public class IMouseMessage
    {
        private readonly GBox timed = new GBox();
        private readonly GBox normal = new GBox();
        private double timer = 0;
        private readonly Coo clickCoo = new Coo();
        private int distance = 0;
        private bool above;

        public IMouseMessage()
        {
        }

        public void Render(Renderer r, float ds)
        {
            GBox box = this.timed;

            if (timer < 0)
            {
                timed.Clear();
                box = normal;
            }

            timer -= ds;

            if (!box.EmptyIs())
            {
                COORDINATE mCoo = VIEW.Mouse();

                int M = 48 * C.SG + distance;

                int y1 = mCoo.Y() + M;
                if (above || mCoo.Y() + M + box.Height() > C.HEIGHT())
                {
                    y1 = mCoo.Y() - M - box.Height();
                }

                int x1 = mCoo.X() - box.Width() / 2;
                if (x1 < M)
                {
                    x1 = M;
                }
                else if (x1 + box.Width() + M > C.WIDTH())
                {
                    x1 = C.WIDTH() - box.Width() - M;
                }

                if (y1 < M)
                {
                    y1 = M;
                    x1 = mCoo.X() <= C.DIM().CX() ? mCoo.X() + M : mCoo.X() - M - box.Width();
                }
                else if (y1 + box.Height() > C.DIM().Height())
                {
                    y1 = C.DIM().Height() - box.Height() - M;
                    x1 = mCoo.X() <= C.DIM().CX() ? mCoo.X() + M : mCoo.X() - M - box.Width();
                }

                box.Render(r, x1, y1);
            }

            normal.Clear();
            distance = 0;
            above = false;
        }

        public void SetAbove()
        {
            above = true;
        }

        public GBox Init(COORDINATE mCoo, bool time)
        {
            clickCoo.Set(mCoo);
            if (time)
            {
                timed.Clear();
                timer = 5;
                return timed;
            }
            return normal;
        }

        public GBox Get()
        {
            if (timer > 0)
                return timed;
            return normal;
        }

        public void Update(COORDINATE mCoo)
        {
            if (mCoo.TileDistanceTo(clickCoo) > 50)
                timer = -1;
        }

        public bool IsOn()
        {
            return timer >= 0 && !timed.EmptyIs();
        }

        public bool Close()
        {
            if (timer > 0)
            {
                timer = 0;
                return true;
            }
            return false;
        }

        public void SetDistance(int max)
        {
            this.distance = max;
        }
    }
}