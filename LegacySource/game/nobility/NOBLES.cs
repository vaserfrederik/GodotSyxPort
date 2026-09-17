using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using game;
using game.boosting;
using game.debug;
using game.faction.npc;
using game.faction.player;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;
using util.updating;
using view.interrupter;
using view.main;
using view.ui.message;

namespace game.nobility
{
    public sealed class NOBLES : GameResource
    {
        public static int MAX_MAX = 256;

        public readonly CharSequence[] nameRanks;
        public static readonly int RANK_INCREASE = 2;

        private readonly ArrayList<Noble> active = new ArrayList<Noble>(256);
        private readonly ArrayList<Noble> all = new ArrayList<Noble>(256);
        private readonly IUpdater upper;

        public readonly BoostSpecs boosters;
        public readonly Boostable MAX;
        public readonly Boostable MAX_RANKS;
        readonly BoostCompound<NobleOffice> bos;

        public readonly LIST<NobleOffice> OFFICES = NobleOfficeUtil.Make();

        private int ranksAllocated = 0;
        private int[] allocations;
        private int ri = -1;

        public NOBLES() : base("NOBILITIES", false)
        {
            D.t(this);
            MAX = BOOSTING.Push("NOBLES_MAX", 0, HCLASSES.NOBLE().names, D.g("desc", "The amount of nobles you may appoint."), UI.icons().s.noble, BOOSTABLES.CIVICS());
            MAX_RANKS = BOOSTING.Push("NOBLES_RANKS_MAX", 0, D.g("rname", "Noble Promotions"), D.g("rdesc", "The amount of promotions you can offer your nobles."), UI.icons().s.noble.Twin(UI.icons().s.chevron(DIR.N).CreateColored(COLOR.ORANGE100), DIR.C, 0), BOOSTABLES.CIVICS());
            boosters = new BoostSpecs(HCLASSES.NOBLE().names, UI.icons().s.noble, true);
            nameRanks = new Json(PATHS.PLAYER().folder("noble").text.gets("_RANKS")).texts("RANKS");
            while (all.HasRoom())
                new Noble(all);

            upper = new IUpdater(all.Size, 10)
            {
                protected override void Update(int i, double timeSinceLast)
                {
                    all.Get(i).Update(timeSinceLast);
                }
            };

            bos = new BoostCompound<NobleOffice>(boosters, OFFICES)
            {
                protected override BoostSpecs Bos(NobleOffice t)
                {
                    return t.boosts;
                }

                protected override double Get(Boostable bo, FactionNPC f, bool isMul)
                {
                    return 0;
                }

                protected override double GetValue(NobleOffice t)
                {
                    return t.value(Allocations(t));
                }
            };

            IDebugPanel.Add("noble galore", new ACTION()
            {
                public override void Exe()
                {
                    new BoosterValue(BValue.VALUE1, new BSourceInfo("cheat", UI.icons().s.cancel), 10, false).Add(MAX);
                    new BoosterValue(BValue.VALUE1, new BSourceInfo("cheat", UI.icons().s.cancel), 10, false).Add(MAX_RANKS);
                }
            });
        }

        protected override void Save(FilePutter file)
        {
            foreach (Noble n in all)
                n.saver.Save(file);
            upper.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            foreach (Noble n in all)
                n.saver.Load(file);
            upper.Load(file);
            bos.ClearChache();
            ri = -1;
            active.Clear();
            foreach (Noble n in all)
            {
                if (n.subject() != null)
                    active.Add(n);
            }
        }

        protected override void Update(double ds, Profiler prof)
        {
            prof.LogStart(NOBLES.class);
            upper.Update(ds);
            prof.LogEnd(NOBLES.class);
        }

        public LIST<Noble> ALL()
        {
            return all;
        }

        public int maxRanks()
        {
            return nameRanks.Length;
        }

        private void Cache()
        {
            if (ri != active.Size)
            {
                ri = active.Size;
                ranksAllocated = 0;
                allocations = new int[OFFICES.Size];
                for (int ni = 0; ni < active.Size; ni++)
                {
                    ranksAllocated += active.Get(ni).rank();
                    NobleOffice n = active.Get(ni).office();
                    if (n != null)
                        allocations[n.index] += 1 + RANK_INCREASE * active.Get(ni).rank();
                }
            }
        }

        public int ranksAllocated()
        {
            Cache();
            return ranksAllocated;
        }

        public int allocations(NobleOffice o)
        {
            Cache();
            return allocations[o.index];
        }

        public void ranksAllocate(Noble n)
        {
            if (ranksAllocated() < (int)MAX_RANKS.Get(HCLASS_RACE.clP()))
            {
                n.rankInc();
                ri = -1;
            }
        }

        public short assignOnlyCallFromHumanoid(Humanoid h)
        {
            if (!active.HasRoom())
                return -1;
            foreach (Noble n in all)
            {
                if (n.subject() == null)
                {
                    n.assign(h);
                    bos.ClearChache();
                    ri = -1;
                    if (active.Contains(n))
                        throw new RuntimeException();
                    active.Add(n);
                    return (short)n.index;
                }
            }
            throw new RuntimeException();
        }

        public void vacateOnlyCallFromHumanoid(Humanoid h, short pos)
        {
            Noble e = all.Get(pos);
            if (e.subject() != h)
                throw new RuntimeException();
            DeathMess m = new DeathMess(h, e);
            active.Remove(e);

            e.saver.Clear();
            ri = -1;
            bos.ClearChache();
            m.Send();
        }

        public void setOffice(Noble n, NobleOffice office)
        {
            n.setOffice(office);
            ri = -1;
        }

        public Noble get(short index)
        {
            return all.Get(index);
        }

        public LIST<NobleOffice> OFFICES => OFFICES;

        public LIST<Noble> active => active;

        public LIST<Noble> all => all;

        public IUpdater upper => upper;

        public BoostSpecs boosters => boosters;

        public Boostable MAX => MAX;

        public Boostable MAX_RANKS => MAX_RANKS;

        public BoostCompound<NobleOffice> bos => bos;

        private static CharSequence[] nameRanks => nameRanks;

        private static int RANK_INCREASE => RANK_INCREASE;

        private static int ranksAllocated => ranksAllocated;

        private static int[] allocations => allocations;

        private static int ri => ri;

        public static void Main(string[] args)
        {
            // Main method for testing or running the game
        }
    }
}