using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Battle
{
    using Game;
    using Game.Battle.Div;
    using Game.Faction;
    using Init.Constant;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;

    public sealed class Army
    {
        private readonly int index;
        private readonly IList<Div> divisions;
        private readonly List<Div> ordered;
        public readonly Cache Men = new Cache()
        {
            protected override int Count()
            {
                int a = 0;
                for (int di = 0; di < divisions.Count; di++)
                {
                    Div d = divisions[di];
                    a += d.MenNrOf();
                }
                return a;
            }
        };
        private readonly int menMax;

        public readonly int Bit;

        public Army(List<Army> armies, List<Div> divisions)
        {
            this.index = armies.Add(this);

            List<Div> divs = new List<Div>(Config.Battle().DivisionsPerArmy);

            for (int i = 0; i < Config.Battle().DivisionsPerArmy; i++)
                new Div(divisions, divs, this);
            this.divisions = divs;
            ordered = new List<Div>(divs);

            menMax = Config.Battle().MenPerDivision * Config.Battle().DivisionsPerArmy;
            Bit = 1 << index;
        }

        public int Index()
        {
            return index;
        }

        public IList<Div> Divisions()
        {
            return divisions;
        }

        public IList<Div> Ordered()
        {
            return ordered;
        }

        public void SetDivAtOrderedIndex(Div toBeReplaced, Div replacer)
        {
            if (ordered.RemoveOrdered(replacer) == -1)
                throw new RuntimeException();
            int oi = ordered.IndexOf(toBeReplaced);
            if (oi < 0)
                throw new RuntimeException();
            ordered.Insert(oi, replacer);
        }

        public Div GetNextEmptyOrdered()
        {
            for (int di = 0; di < ordered.Count; di++)
            {
                Div d = ordered[di];
                if (d.Info.Men() == 0 && d.MenNrOf() == 0)
                {
                    return d;
                }
            }
            return null;
        }

        public int Men()
        {
            return Men.I();
        }

        public int MenMax()
        {
            return menMax;
        }

        public Army Enemy()
        {
            return GAME.ARMIES().Armies().Get((index + 1) % 2);
        }

        public double Morale()
        {
            return GAME.ARMIES().Factors.Morale(this);
        }

        public Faction Faction()
        {
            if (this == GAME.ARMIES().Player())
                return FACTIONS.Player();
            return FACTIONS.OtherFaction();
        }

        private readonly SAVABLE saver = new SAVABLE()
        {
            public void Save(FilePutter file)
            {
                foreach (Div d in ordered)
                {
                    file.I(d.IndexArmy());
                }
            }

            public void Load(FileGetter file)
            {
                Men.Recount();

                ordered.ClearSloppy();
                for (int i = 0; i < divisions.Count; i++)
                {
                    ordered.Add(divisions.Get(file.I()));
                }
            }

            public void Clear()
            {
                Men.Recount();
                ordered.ClearSloppy();
                ordered.Add(divisions);
            }
        };

        public bool Defender()
        {
            return true;
        }

        public abstract class Cache
        {
            private bool dirty = true;
            private int i = 0;

            public int I()
            {
                if (dirty)
                {
                    i = Count();
                    dirty = false;
                }
                return i;
            }

            public void Recount()
            {
                dirty = true;
            }

            protected abstract int Count();
        }

        public bool Player()
        {
            return GAME.ARMIES().Player() == this;
        }
    }
}