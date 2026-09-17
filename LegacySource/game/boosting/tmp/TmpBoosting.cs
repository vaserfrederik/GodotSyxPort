using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.boosting;
using game.debug;
using game.faction;
using init.type;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;
using world.map.regions;

namespace game.boosting.tmp
{
    public class TmpBoosting : GameResource
    {
        private static readonly CharSequence ¤¤regs = "Affected Regions";
        private static readonly CharSequence ¤¤groups = "Affected Groups";

        static TmpBoosting()
        {
            D.ts(typeof(TmpBoosting));
        }

        static ArrayListGrower<TmpBoostSpec> allTmp;
        static KeyMap<TmpBoostSpec> allMap;

        private readonly ArrayListGrower<TmpBoostable<object>> all = new ArrayListGrower<TmpBoostable<object>>();
        public readonly TmpBoostable<Region> regions = new TmpBoostable<Region>(all, WREGIONS.MAX, this);
        public readonly TmpBoostable<HCLASS_RACE> popcl = new TmpBoostable<HCLASS_RACE>(all, HCLASS_RACE.ALL().Count, this);
        public readonly TmpBoostable<Faction> factions = new TmpBoostable<Faction>(all, FACTIONS.MAX(), this);

        private readonly ArrayListGrower<TmpBoostSpec> specs;
        readonly Data[] datas;

        public TmpBoosting(GAME game) : base("TEMP_EVENTS")
        {
            allTmp = new ArrayListGrower<TmpBoostSpec>();
            allMap = new KeyMap<TmpBoostSpec>();
            specs = allTmp;
            GAME.addOnViewInit(new ACTION
            {
                public void exe()
                {
                    allTmp = null;

                    for (int i = 0; i < datas.Length; i++)
                    {
                        datas[i] = new Data(specs.Size());
                    }

                    KeyMap<ArrayListGrower<BoostSpec>> map = new KeyMap<ArrayListGrower<BoostSpec>>();

                    foreach (TmpBoostSpec ts in specs)
                    {
                        foreach (BoostSpec s in ts.spec.all())
                        {
                            string k = s.boostable.key + s.booster.isMul;
                            if (!map.ContainsKey(k))
                            {
                                map.Put(k, new ArrayListGrower<BoostSpec>());
                            }
                            map.Get(k).Add(s);
                        }
                    }

                    foreach (LIST<BoostSpec> bos in map.all())
                    {
                        bool isMul = bos.Get(0).booster.isMul;
                        double min = isMul ? 1 : 0;
                        double max = min;

                        foreach (BoostSpec s in bos)
                        {
                            Booster b = s.booster;
                            if (isMul)
                            {
                                if (b.to() < 1)
                                    min *= b.to();
                                else
                                    max *= b.to();
                            }
                            else
                            {
                                if (b.to() < 0)
                                    min += b.to();
                                else
                                    max += b.to();
                            }
                        }

                        new TBooster(bos.Get(0).boostable, min, max, isMul);
                    }
                }
            });

        }

        protected override void update(double ds, Profiler prof)
        {
            // TODO Auto-generated method stub

        }

        protected override void save(FilePutter file)
        {
            file.i(specs.Size());
            foreach (TmpBoostSpec s in specs)
            {
                file.chars(s.key);
            }

            file.i(datas.Length);
            foreach (Data d in datas)
                d.save(file);
        }

        protected override void load(FileGetter file)
        {
            int am = file.i();
            int[] look = Alloc.ii(am);
            Array.Fill(look, -1);
            for (int i = 0; i < am; i++)
            {
                string k = file.chars();
                if (allMap.ContainsKey(k))
                {
                    look[i] = allMap.Get(k).index;
                }
            }
            am = file.i();
            for (int i = 0; i < am; i++)
            {
                if (i < datas.Length)
                    datas[i].load(file, look);
                else
                    new Data(specs.Size()).load(file, look);
            }
        }

        public LIST<TmpBoostSpec> specs()
        {
            return specs;
        }

        public void hover(GBox b, Faction f)
        {
            foreach (TmpBoostSpec s in GAME.BOOST().specs())
            {
                bool faction = factions.is(f, s);
                int regs = 0;
                for (int ri = 0; ri < f.realm().regions(); ri++)
                {
                    if (regions.is(f.realm().region(ri), s))
                        regs++;
                }
                bool pops = false;
                if (f == FACTIONS.player())
                {
                    foreach (HCLASS_RACE cl in HCLASS_RACE.ALL())
                    {
                        if (popcl.is(cl, s))
                        {
                            pops = true;
                            break;
                        }
                    }
                }

                if (!faction && regs == 0 && !pops)
                    continue;

                b.add(s.icon);
                b.textLL(s.name);
                b.NL();
                b.text(s.desc);
                b.NL(8);

                foreach (BoostSpec ss in s.spec.all())
                {
                    b.add(ss.boostable.icon);
                    b.textL(ss.boostable.name);
                    b.tab(7);
                    GText t = b.text();
                    if (ss.booster.isMul)
                    {
                        t.add('*');
                        GFORMAT.f1(t, ss.booster.to());
                    }
                    else
                    {
                        GFORMAT.f0(t, ss.booster.to());
                    }
                    b.add(t);
                    b.NL();
                }

                b.NL(4);

                if (regs > 0)
                {
                    b.textLL(¤¤regs);
                    b.add(GFORMAT.i(b.text(), regs));
                    b.NL();
                }

                if (pops)
                {
                    b.textLL(¤¤groups);
                    foreach (HCLASS_RACE cl in HCLASS_RACE.ALL())
                    {
                        if (cl.race != null && cl.cl != null && popcl.is(cl, s))
                        {
                            b.add(cl.race.appearance().iconBig);
                            b.rewind(8);
                            if (cl.cl.iconSmall() == null)
                                continue;
                            b.add(cl.cl.iconSmall());
                            b.space();
                        }
                    }
                }

                b.sep();
            }
        }
    }
}