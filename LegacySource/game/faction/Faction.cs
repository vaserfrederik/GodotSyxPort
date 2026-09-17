using System;
using System.Collections.Generic;
using System.IO;

namespace game.faction
{
    public abstract class Faction : BOOSTABLE_O, INDEXED
    {
        bool wasActive = false;
        public readonly Str name = new Str(24);
        private readonly int index;
        protected bool event;
        public short eventMark;

        protected Faction(LISTE<Faction> all)
        {
            index = all.Add(this);
        }

        public abstract Race Race();

        public int Index()
        {
            return index;
        }

        public final Region CapitolRegion()
        {
            return RD.REALM(this).Capitol();
        }

        protected void Save(FilePutter file)
        {
            name.Save(file);
            Res().Save(file);
            Banner().Save(file);
            Credits().Save(file);
            file.Bool(wasActive);
            file.Bool(event);
            file.S(eventMark);
        }

        protected void Load(FileGetter file)
        {
            name.Load(file);
            Res().Load(file);
            Banner().Load(file);
            Credits().Load(file);
            wasActive = file.Bool();
            event = file.Bool();
            eventMark = file.S();
        }

        protected void Clear()
        {
            name.Clear();
            Res().Clear();
            Banner().Clear();
            Credits().Clear();
            event = false;
            eventMark = 0;
        }

        protected void Update(double ds)
        {
            Res().Update(ds, this);
            Banner().Update(ds, this);
            Credits().Update(ds, this);
        }

        public final bool IsActive()
        {
            return Realm().Capitol() != null;
        }

        public int Cx()
        {
            if (Realm().Regions() > 0)
                return Realm().Capitol().Cx();
            else if (Armies().All().Count > 0)
                return Armies().All()[0].Ctx();
            return WORLD.TWIDTH() / 2;
        }

        public int Cy()
        {
            if (Realm().Regions() > 0)
                return Realm().Capitol().Cy();
            else if (Armies().All().Count > 0)
                return Armies().All()[0].Cty();
            return WORLD.THEIGHT() / 2;
        }

        public Realm Realm()
        {
            return RD.REALM(this);
        }

        public abstract FBUYER Buyer(TRADABLE t);

        public abstract FSELLER Seller(TRADABLE t);

        public abstract FResources Res();

        public abstract FBanner Banner();

        public abstract FCredits Credits();

        public ADArmies Armies()
        {
            return AD.Army(this);
        }

        public abstract string RulerName();

        public static abstract class FactionActivityListener
        {
            static readonly LinkedList<FactionActivityListener> all = new LinkedList<FactionActivityListener>();
            static FactionActivityListener()
            {
                new GameDisposable()
                {
                    protected override void Dispose()
                    {
                        all.Clear();
                    }
                };
            }

            public FactionActivityListener()
            {
                all.Add(this);
            }

            public abstract void Remove(FactionNPC f);
            public abstract void Add(FactionNPC f);
        }

        public override string ToString()
        {
            return "[" + index + "]" + name;
        }

        public double BoostableValue(BValue v)
        {
            return v.VGet(this);
        }

        public static string Name(Faction f)
        {
            if (f != null)
                return f.name;
            return Dic.¤¤Rebels;
        }

        public abstract double OffensivePower();

        public abstract int Citizens(Race race);

        public bool Event()
        {
            return event;
        }

        public void EventSet(bool e)
        {
            event = e;
        }
    }
}