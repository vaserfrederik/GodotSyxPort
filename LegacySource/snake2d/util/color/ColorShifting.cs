using System;

namespace Snake2D.Util.Color
{
    public class ColorShifting : ColorImp
    {
        private int dRed;
        private int dGreen;
        private int dBlue;
        private int r;
        private int g;
        private int b;
        private float speed = 0.5f;
        private double old = -1;

        public ColorShifting(COLOR from, COLOR to) : base(0, 0, 0)
        {
            r = Byte.ToUnsignedInt(from.Red());
            g = Byte.ToUnsignedInt(from.Green());
            b = Byte.ToUnsignedInt(from.Blue());

            dRed = Byte.ToUnsignedInt(to.Red()) - r;
            dGreen = Byte.ToUnsignedInt(to.Green()) - g;
            dBlue = Byte.ToUnsignedInt(to.Blue()) - b;
        }

        private void Update()
        {
            if (old != CORE.GetUpdateInfo().GetSecondsSinceFirstUpdate())
            {
                old = CORE.GetUpdateInfo().GetSecondsSinceFirstUpdate();
                double timer = CORE.GetUpdateInfo().GetSecondsSinceFirstUpdate() * speed;

                double d = timer - (int)timer;

                if (d > 0.5)
                {
                    timer = 1.0 - (d - 0.5) * 2;
                }
                else
                {
                    timer = d * 2.0;
                }

                SetRed((int)(r + timer * dRed));
                SetGreen((int)(g + timer * dGreen));
                SetBlue((int)(b + timer * dBlue));
            }
        }

        public override void Bind()
        {
            Update();
            base.Bind();
        }

        public override byte Red()
        {
            Update();
            return base.Red();
        }

        public ColorShifting SetSpeed(double speed)
        {
            this.speed = (float)speed;
            return this;
        }
    }
}