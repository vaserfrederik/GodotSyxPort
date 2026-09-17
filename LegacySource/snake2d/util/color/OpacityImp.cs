using System;

namespace Snake2D.Util.Color
{
    public class OpacityImp : OPACITY
    {
        public static readonly OpacityImp TMP = new OpacityImp(0);

        private byte opacity;

        public OpacityImp(int op)
        {
            Set(op);
        }

        public OpacityImp(OPACITY o)
        {
            this.opacity = o.Get();
        }

        public void Set(int op)
        {
            if (op < 0)
                op = 0;
            else if (op > 255)
                op = 255;
            opacity = (byte)op;
        }

        public void Set(double op)
        {
            Set((int)(op * 255));
        }

        public void Set(OPACITY o)
        {
            this.opacity = o.Get();
        }

        public override byte Get()
        {
            return opacity;
        }

        public void Increase(float factor)
        {
            Set((int)(opacity * factor));
        }

        public void Increase(int amount)
        {
            Set(opacity + amount);
        }

        public static void UnBind()
        {
            CORE.Renderer().SetNormalOpacity();
        }

        public override string ToString()
        {
            return this.GetType().Name + " " + opacity;
        }
    }
}