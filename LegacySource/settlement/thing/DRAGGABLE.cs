using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace settlement.thing
{
    public interface DRAGGABLE : BODY_HOLDER
    {
        void drag(DIR d, int cx, int cy, int fromDist);
        void drag(DIR d, int cx, int cy);
        bool canBeDragged();
    }

    public abstract class DRAGGABLE_HOLDER
    {
        public readonly byte index;

        private static readonly ArrayList<DRAGGABLE_HOLDER> all = new ArrayList<DRAGGABLE_HOLDER>(16);

        static DRAGGABLE_HOLDER()
        {
            new GameDisposable
            {
                protected override void dispose()
                {
                    all.Clear();
                }
            };
        }

        public DRAGGABLE_HOLDER()
        {
            index = (byte)all.Add(this);
        }

        public abstract DRAGGABLE draggable(int index);

        public static LIST<DRAGGABLE_HOLDER> all()
        {
            return all;
        }
    }
}