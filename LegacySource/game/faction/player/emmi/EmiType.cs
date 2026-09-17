using System;
using System.IO;
using System.Linq;

namespace game.faction.player.emmi
{
    public abstract class EmiType<T> : INT_OE<T>
    {
        private static string ¤¤pointsAllocated = "Points Allocated";
        private static string ¤¤pointsAvailable = "Points Available";
        private static string ¤¤Effectivness = "Effectiveness";

        static EmiType()
        {
            D.ts(typeof(EmiType<>));
        }

        public readonly SPRITE icon;

        public readonly string name;
        public readonly string desc;
        private readonly int[] ams;
        private int total;
        private readonly int max;

        protected EmiType(SPRITE icon, string name, string desc, int size, int max)
        {
            this.name = name;
            this.desc = desc;
            this.icon = icon;
            ams = Alloc.ii(size);
            this.max = max;
        }

        public int Total()
        {
            return total;
        }

        public override int Get(T t)
        {
            return ams[Index(t)];
        }

        public override int Min(T t)
        {
            return 0;
        }

        public override int Max(T t)
        {
            return max;
        }

        public override double GetD(T t)
        {
            return (double)Get(t) / max;
        }

        int Get(int index)
        {
            return ams[index];
        }

        void Set(int index, int value)
        {
            Count(index, -ams[index]);
            ams[index] = value;
            Count(index, ams[index]);
        }

        void Count(int index, int am)
        {
            total += am;
        }

        public override void Set(T t, int i)
        {
            int index = Index(t);
            Set(index, i);
        }

        void Save(FilePutter file)
        {
            file.isE(ams);
        }

        void Load(FileGetter file) throws IOException
        {
            Clear();
            file.isE(ams);
            total = 0;
            for (int i = 0; i < ams.Length; i++)
            {
                Count(i, ams[i]);
            }
        }

        void Clear()
        {
            ams.Fill(0);
            total = 0;
        }

        protected abstract int Index(T t);

        public void Hover(T t, GUI_BOX text)
        {
            GBox b = (GBox)text;
            b.Title(name);
            b.Text(desc);
            b.NL(4);

            b.TextLL(¤¤pointsAllocated);
            b.Tab(6);
            b.Add(GFORMAT.i(b.Text(), Get(t)));
            b.NL();

            b.TextLL(¤¤pointsAvailable);
            b.Tab(6);
            b.Add(GFORMAT.iIncr(b.Text(), FACTIONS.Player().Emissaries.Available()));
            b.NL();

            b.TextLL(¤¤Effectivness);
            b.Tab(6);
            b.Add(GFORMAT.perc(b.Text(), FACTIONS.Player().Emissaries.PenaltyMul()));
            b.NL();
        }
    }
}