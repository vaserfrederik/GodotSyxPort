using System;
using System.Collections.Generic;
using System.IO;

namespace game.faction.royalty
{
    public sealed class NPCCourt : NPCResource
    {
        public const int MAX = 4;
        private readonly King king = new King(this);
        private readonly List<Royalty> all = new List<Royalty>(MAX);
        public readonly FactionNPC faction;
        private double addT = 0;

        private static readonly string ¤¤sucession = "{0} ascends the throne of {1}, succeeding the old leader, {2}.";

        static NPCCourt()
        {
            D.ts(typeof(NPCCourt));
        }

        public NPCCourt(FactionNPC faction, List<NPCResource> all)
            : base(all)
        {
            this.faction = faction;
        }

        public List<Royalty> All()
        {
            return all;
        }

        public King King()
        {
            return king;
        }

        protected override SAVABLE Saver()
        {
            return new SAVABLE
            {
                Save = file =>
                {
                    file.WriteInt(all.Count);
                    foreach (var r in all)
                        r.Save(file);

                    king.Save(file);
                },
                Load = file =>
                {
                    int k = file.ReadInt();
                    all.Clear();
                    for (int i = 0; i < k; i++)
                    {
                        all.Add(new Royalty(this, file));
                    }
                    king.Load(this, file);
                },
                Clear = () => all.Clear()
            };
        }

        protected override void Update(FactionNPC faction, double seconds)
        {
            addT += seconds;
            if (addT > TIME.SecondsPerDay())
            {
                for (int i = 0; i < all.Count; i++)
                {
                    Royalty r = all[i];
                    STATS.POP().age.DAYS.Inc(r.induvidual, 1);
                }

                addT -= TIME.SecondsPerDay();
                if (RND.OneIn(16))
                    AddSuccessor();
            }

            for (int i = 0; i < all.Count; i++)
            {
                Royalty r = all[i];
                r.Update(seconds);
                if (TIME.Days().BitsSinceStart() > r.deathDay)
                {
                    Kill(r);
                    i--;
                }
            }
        }

        public void AddSuccessor()
        {
            if (!all.HasRoom())
                return;
            Royalty r = NewSuccessor(king.Roy().Induvidual.Race());
            int i = all.Count;
            all.Add(r);
            foreach (var l in RoyaltyEventListener.All)
                l.Change(i, r, null);
        }

        private Royalty NewSuccessor(Race roy)
        {
            double tot = 0;
            foreach (var r in RD.RACES().All)
            {
                double d = roy.Pref().Race(roy);
                if (r.Race == roy)
                    d += RD.RACES().All.Count * 16 - 12 * RD.RACES().All.Count * BOOSTABLES.NOBLE().TOLERANCE.Get(king.Roy().Induvidual);
                d *= r.Pop.Faction().Get(faction);
                tot += d;
            }
            tot *= RND.RFloat();
            foreach (var r in RD.RACES().All)
            {
                double d = roy.Pref().Race(roy);
                if (r.Race == roy)
                    d += RD.RACES().All.Count * 16 - 12 * RD.RACES().All.Count * BOOSTABLES.NOBLE().TOLERANCE.Get(king.Roy().Induvidual);
                d *= r.Pop.Faction().Get(faction);
                tot -= d;
                if (tot <= 0)
                    return new Royalty(this, r.Race);
            }
            return new Royalty(this, roy);
        }

        private void Kill(Royalty r)
        {
            STATS.APPEARANCE().dead.indu().Set(r.induvidual, 1);

            int si = r.SuccessionI();

            string oldKing = king.Name;

            for (int i = si; i < all.Count; i++)
            {
                foreach (var l in RoyaltyEventListener.All)
                    l.Change(i, all[i], i + 1 < all.Count ? all[i + 1] : null);
            }
            all.RemoveAt(si);
            if (si == 0)
            {
                if (all.Count == 0)
                {
                    Royalty rn = new Royalty(this, r.induvidual.Race());
                    all.Add(rn);
                    foreach (var l in RoyaltyEventListener.All)
                        l.Change(1, null, rn);
                }

                king.Init();

                string newKing = king.Name;

                Str.TMP.Clear().Add(¤¤sucession);
                Str.TMP.Insert(0, newKing);
                Str.TMP.Insert(1, faction.Name);
                Str.TMP.Insert(2, oldKing);
                WORLD.LOG().Log(null, faction, UI.icons().s.crown, Str.TMP, faction.Cx(), faction.Cy());
            }
        }

        protected override void Generate(RDRace race, FactionNPC faction, bool fromScratch)
        {
            all.Clear();
            all.Add(new Royalty(this, race.Race));
            while (all.HasRoom())
                AddSuccessor();
            king.Init();
        }

        public void Init()
        {
            if (all.Count == 0)
                all.Add(new Royalty(this, RD.RACES().All[0].Race));
        }

        public void Promote(Royalty roy, bool message)
        {
            if (!all.Contains(roy))
                throw new RuntimeException();
            int i = all.IndexOf(roy);
            foreach (var l in RoyaltyEventListener.All)
                l.Change(1, all[1], roy);
            foreach (var l in RoyaltyEventListener.All)
                l.Change(i, roy, all[i]);
            all.Swap(1, i);
        }

        public Race Race()
        {
            if (king.Roy() == null)
                return FACTIONS.Player().Race();
            return king.Roy().Induvidual.Race();
        }

        public abstract class RoyaltyEventListener
        {
            private static readonly ListGrower<RoyaltyEventListener> all = new ListGrower<RoyaltyEventListener>();
            static RoyaltyEventListener()
            {
                new GameDisposable
                {
                    Dispose = () => all.Clear()
                };
            }

            protected RoyaltyEventListener()
            {
                all.Add(this);
            }

            public abstract void Change(int successionI, Royalty old, Royalty nn);
        }
    }
}