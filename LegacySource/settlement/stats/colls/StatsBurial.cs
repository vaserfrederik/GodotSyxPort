using System;
using System.Collections.Generic;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.main;
using settlement.room.spirit.grave;
using settlement.stats;
using settlement.stats.standing;
using settlement.stats.stat;
using settlement.stats.util;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.keymap;
using util.text;

namespace settlement.stats.colls
{
    public class StatsBurial : StatCollection
    {
        private readonly LIST<StatGrave> graves;
        public STAT DESECRATION;

        private readonly LIST<STAT> others;

        private static readonly CharSequence ¤¤more = "¤We want better prospects of being buried in a {0}.";
        private static readonly CharSequence ¤¤less = "¤We do not wish to be buried in a {0}.";
        private static readonly CharSequence ¤¤name = "Burial";
        private static readonly CharSequence ¤¤desc = "Stats related to afterlife.";

        static
        {
            D.ts(typeof(StatsBurial));
        }

        public StatsBurial(StatsInit init) : base(init, "BURIAL", ¤¤name, ¤¤desc)
        {
            ArrayList<StatGrave> graves = new ArrayList<StatGrave>(SETT.ROOMS().GRAVES.size());
            foreach (GraveData.GRAVE_DATA_HOLDER h in SETT.ROOMS().GRAVES)
            {
                StatInfo inInfo = new StatInfo(h.graveData().blueprint().info.names, h.graveData().blueprint().info.desc);
                inInfo.setOpinion(¤¤more, ¤¤less);

                graves.add(new StatGrave(h.graveData(), init, inInfo));
            }
            this.graves = graves;

            DESECRATION = new STATFakeRace("DESECRATION", init)
            {
                protected override double getDD(Race r)
                {
                    double am = 0;
                    foreach (StatGrave g in graves)
                        am += g.grave().disturbance.getD();
                    return am;
                }
            };

            ArrayList<STAT> others = new ArrayList<STAT>(all().size());
            foreach (STAT s in all())
            {
                if (s is StatGrave)
                    continue;
                others.add(s);
            }
            this.others = new ArrayList<STAT>(others);

            init.upers.add(new GraveUpdater());
        }

        public LIST<STAT> others()
        {
            return others;
        }

        public LIST<StatGrave> graves()
        {
            return graves;
        }

        private class GraveUpdater : StatUpdatable
        {
            private int[] available;
            private int[] needed;

            public GraveUpdater()
            {
                available = Alloc.ii(graves.size());
                needed = Alloc.ii(graves.size());
            }

            public override void update(double ds)
            {
                foreach (StatGrave gr in graves)
                {
                    available[gr.gIndex()] = gr.grave().total.get(null) * 100;
                }

                foreach (HCLASS c in HCLASSES.ALL())
                {
                    if (!c.player)
                        continue;

                    foreach (StatGrave gr in graves)
                    {
                        needed[gr.gIndex()] = 0;
                    }

                    foreach (Race r in RACES.all())
                    {
                        foreach (StatGrave gr in r.service().GRAVES.get(c.index()))
                        {
                            if (gr.grave().permission().get(c, r))
                            {
                                needed[gr.gIndex()] += POP.tot(c, r);
                            }
                        }
                    }

                    foreach (Race r in RACES.all())
                    {
                        foreach (StatGrave gr in r.service().GRAVES.get(c.index()))
                        {
                            if (gr.grave().permission().get(c, r))
                            {
                                gr.access.get(c).set(r, 0);

                                int av = available[gr.gIndex()];
                                int neededValue = this.needed[gr.gIndex()];
                                if (av == 0)
                                {
                                    gr.access.get(c).setD(r, 0);
                                }
                                else
                                {
                                    double d = av / (double)(neededValue + 1);
                                    gr.access.get(c).setD(r, CLAMP.d(d, 0, 1));
                                }
                            }
                            else
                            {
                                gr.access.get(c).setD(r, 0);
                            }
                        }
                    }
                }
            }
        }

        public class StatGrave : STATFakeData
        {
            private GraveData h;
            private RMapIntTwo<HCLASS, Race> access = new RMapIntTwo<HCLASS, Race>(HCLASSES.MAP(), RACES.map());

            public StatGrave(GraveData h, StatsInit init, StatInfo info) : base(h.blueprint().key, "BURR_" + h.blueprint().key, init, info)
            {
                standing = new StatStanding(this, 0, h.standingDef());
                this.h = h;

                init.savers.put("BURR_ACCESS_" + h.blueprint().key, access);
                info().icon = h.blueprint().icon.resized(Icon.S);
            }

            protected override double getDD(HCLASS cl, Race r)
            {
                return access.get(cl).getD(r) * h.get(cl).value.getD(r);
            }

            public GraveData grave()
            {
                return h;
            }

            int gIndex()
            {
                return index() - STATS.BURIAL().graves.get(0).index();
            }

            public override void hover(GUI_BOX text, HCLASS cl, Race type)
            {
                GBox b = (GBox)text;
                b.title(info().name);
                GraveData da = grave();

                b.textLL(Dic.¤¤Access);
                b.add(GFORMAT.perc(b.text(), access.get(cl).getD(type)));

                b.textLL(da.respect.info().name);
                b.add(GFORMAT.perc(b.text(), da.respect.getD(null)));
                b.NL().text(da.respect.info().desc);
                b.NL(4);
                b.textLL(da.get(cl).buried.info().name);
                b.add(GFORMAT.iofkInv(b.text(), (int)da.get(cl).buried.getD(type), (int)da.get(cl).buried.getD(type) + (int)da.get(cl).failed.getD(type)));

                b.NL().text(da.get(cl).buried.info().desc);
                b.sep();
                StatHoverer.hover(text, this, cl, type);
            }

            public override void hover(GUI_BOX text, Induvidual indu)
            {
                hover(text, indu.clas(), indu.race());
            }
        }
    }
}