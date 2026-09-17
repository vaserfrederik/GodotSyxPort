using System;

namespace Snake2D
{
    public abstract class VboAbsExt : VboAbs
    {
        protected int count = 0;

        protected int[] vFrom = new int[255];
        protected int[] vTo = new int[255];
        protected int current = 0;

        public VboAbsExt(int type, int maxElements, params VboAttribute[] attributes) : base(type, maxElements, attributes)
        {
        }

        public override void Clear()
        {
            base.Clear();
            current = 0;
            count = 0;
        }
    }
}