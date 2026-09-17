using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.battle
{
    public class ArmyDivOrder : LIST<Div>
    {
        private readonly Army army;
        private readonly int[] order;
        private int ii = 0;

        public ArmyDivOrder(Army army)
        {
            this.army = army;
            order = Alloc.ii(Config.battle().DIVISIONS_PER_ARMY);
            for (int i = 0; i < order.Length; i++)
                order[i] = i;
        }

        public override IEnumerator<Div> GetEnumerator()
        {
            ii = 0;
            return iterer;
        }

        private readonly IEnumerator<Div> iterer = new Iterator<Div>();

        private class Iterator : IEnumerator<Div>
        {
            private int ii = 0;
            private readonly ArmyDivOrder outer;

            public Iterator(ArmyDivOrder outer)
            {
                this.outer = outer;
            }

            public bool MoveNext()
            {
                return ii < outer.order.Length - 1;
            }

            public Div Current => outer.army.divisions().get(outer.order[ii++]);

            object System.Collections.IEnumerator.Current => Current;

            public void Reset()
            {
                ii = 0;
            }

            public void Dispose()
            {
                // Nothing to dispose
            }
        }

        public void Swap(Div d1, Div d2)
        {
            if (d1.army() != army || d2.army() != army)
                throw new RuntimeException();

            int i1 = -1;
            int i2 = -1;
            for (int i = 0; i < order.Length; i++)
            {
                if (order[i] == d1.index())
                    i1 = i;
                if (order[i] == d2.index())
                    i2 = i;
            }

            if (i1 == -1 || i2 == -1)
                throw new RuntimeException();

            int temp = order[i1];
            order[i1] = order[i2];
            order[i2] = temp;
        }

        public void ShoveIn(Div div, Div before)
        {
            if (div.army() != army || before.army() != army)
                throw new RuntimeException();

            bool f = false;
            for (int i = 0; i < order.Length - 1; i++)
            {
                f |= Get(i) == div;
                if (f)
                {
                    order[i] = order[i + 1];
                }
            }

            for (int i = order.Length - 1; i > 0; i--)
            {
                order[i] = order[i - 1];
                if (Get(i - 1) == before)
                {
                    order[i - 1] = div.index();
                    break;
                }
            }
        }

        void Save(FilePutter file)
        {
            file.is(order);
        }

        void Load(FileGetter file) throws IOException
        {
            file.is(order);
        }

        void Clear()
        {
            for (int i = 0; i < order.Length; i++)
                order[i] = i;
        }

        public override Div Get(int index)
        {
            return army.divisions().get(order[index]);
        }

        public override bool Contains(int i)
        {
            return true;
        }

        public override bool Contains(Div object)
        {
            return object.army() == army;
        }

        public override int Size()
        {
            return order.Length;
        }

        public override bool IsEmpty()
        {
            return false;
        }
    }
}