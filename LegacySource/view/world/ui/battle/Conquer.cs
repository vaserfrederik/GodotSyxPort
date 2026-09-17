using System;
using System.Collections.Generic;
using game.faction;
using game.faction.royalty.opinion;
using init.race;
using init.sprite;
using init.sprite.UI;
using init.trade;
using init.type;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.colors;
using util.gui.misc;
using util.gui.table;

namespace view.world.ui.battle
{
    class Conquer : GuiSection
    {
        private static readonly CharSequence ¤¤name = "¤Region Captured";
        private static readonly CharSequence ¤¤desc = "¤The city of {0} is captured my lord. What shall be the fate of its citizens?";

        private static readonly CharSequence ¤¤Enslave = "¤Enslave";
        private static readonly CharSequence ¤¤EnslaveD = "¤Line the population up and ship the healthiest specimen to the capitol as slaves";
        private static readonly CharSequence ¤¤Plunder = "¤Plunder";
        private static readonly CharSequence ¤¤Sack = "¤Loot";
        private static readonly CharSequence ¤¤sackD = "¤Grab what we can of valuables.";
        private static readonly CharSequence ¤¤Raze = "¤Raze";
        private static readonly CharSequence ¤¤RazeD = "¤Let your men blow off some steam after a tough siege. Spare none, leave no stone unturned and teach this settlement a lesson that will be remembered for generations.";

        private static readonly CharSequence ¤¤Occupy = "¤Occupy";
        private static readonly CharSequence ¤¤Abandon = "¤Abandon";
        private static readonly CharSequence ¤¤Puppet = "¤Puppet";

        private static readonly CharSequence ¤¤OccupyD = "¤Take full control of this region.";
        private static readonly CharSequence ¤¤AbandonD = "¤Let this settlement find its future on its own.";
        private static readonly CharSequence ¤¤PuppetP = "¤Currently, there are no nobles available that can take on the job of governing the region.";
        private static readonly CharSequence ¤¤PuppetD = "¤Install a puppet regime. A new faction will be created, which will be long indebted to you.";

        static Conquer()
        {
            D.ts(Conquer.class);
        }

        public static readonly int width = 600;

        private bool enslave;
        private bool loot;
        private bool raze;

        private readonly WBattleSiege.Result result;

        public Conquer(ACTION close, WBattleSiege.Result result)
        {
            this.result = result;
            CharSequence[] descs = UI.FONT().M.getRows(Str.TMP.clear().add(¤¤desc).insert(0, result.besiged.info.name()), width);

            foreach (CharSequence d in descs)
            {
                GText t = new GText(UI.FONT().M, d);
                t.warnify();
                addRelBody(4, DIR.S, t);
            }

            {
                GuiSection ss = new GuiSection();

                ss.add(new GButt.ButtPanel(¤¤Enslave)
                {
                    protected override void clickA()
                    {
                        enslave = !enslave;
                    }

                    protected override void renAction()
                    {
                        selectedSet(enslave);
                    }
                }.setDim(150, 30).hoverInfoSet(¤¤EnslaveD));

                ss.addRightC(2, new GButt.ButtPanel(¤¤Sack)
                {
                    protected override void clickA()
                    {
                        loot = !loot;
                    }

                    protected override void renAction()
                    {
                        selectedSet(loot);
                    }
                }.setDim(150, 30).hoverInfoSet(¤¤sackD));

                ss.addRightC(2, new GButt.ButtPanel(¤¤Raze)
                {
                    protected override void clickA()
                    {
                        raze = !raze;
                    }

                    protected override void renAction()
                    {
                        selectedSet(raze);
                    }
                }.setDim(150, 30).hoverInfoSet(¤¤RazeD));

                addRelBody(16, DIR.S, ss);
            }

            Slaves slaves = new Slaves();
            addRelBody(16, DIR.S, slaves);

            Spoils spoils = new Spoils();
            addRelBody(16, DIR.S, spoils);

            {
                GuiSection stats = new GuiSection();

                stats.addRightC(64, new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.percInv(text, deva());
                    }
                }.hh(UI.icons().m.repair).hoverTitleSet(RD.DEVASTATION().current.info().name).hoverInfoSet(RD.DEVASTATION().current.info().desc));

                stats.addRightC(64, new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.iIncr(text, -(int)(death() * RD.RACES().population.get(result.besiged)));
                    }
                }.hh(UI.icons().m.skull).hoverInfoSet(Dic.¤¤Deaths));

                stats.addRightC(64, new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.f0(text, mercy());
                    }
                }.hh(UI.icons().m.heart).hoverInfoSet(ROPINION.STANCE().chivalry.info.name));

                addRelBody(16, DIR.S, stats);
            }

            {
                GuiSection butts = new GuiSection();

                butts.add(new GButt.ButtPanel(¤¤Occupy)
                {
                    protected override void clickA()
                    {
                        result.occupy();
                        close.act();
                    }
                }.setDim(150, 30));

                butts.addRightC(2, new GButt.ButtPanel(¤¤Abandon)
                {
                    protected override void clickA()
                    {
                        result.abandon();
                        close.act();
                    }
                }.setDim(150, 30));

                butts.addRightC(2, new GButt.ButtPanel(¤¤Puppet)
                {
                    protected override void clickA()
                    {
                        result.puppet();
                        close.act();
                    }

                    protected override bool disabled()
                    {
                        return !FACTION.noblesAvailable();
                    }
                }.setDim(150, 30).hoverInfoSet(¤¤PuppetP));

                addRelBody(16, DIR.S, butts);
            }
        }

        public override void render(SPR_RENDERER r)
        {
            base.render(r);
        }

        private double deva()
        {
            double d = 0.2;
            if (enslave)
                d += 0.25;
            if (loot)
                d += 0.1;
            if (raze)
                d = 0.95;
            return d;
        }

        private double death()
        {
            double d = 0;
            if (enslave)
                d += 0.25;
            if (loot)
                d += 0.1;
            if (raze)
                d = 0.95;
            return d;
        }

        private double mercy()
        {
            double m = 0;

            if (enslave)
            {
                m -= RD.RACES().population.get(result.besiged) * 0.25;
            }
            if (loot)
            {
                m -= RD.RACES().population.get(result.besiged) * 0.25;
            }
            if (raze)
            {
                m -= RD.RACES().population.get(result.besiged);
            }

            m /= 1 + POP.tot(null);
            m = CLAMP.d(m, -5, 5);
            return m;
        }

        private class Spoils : GuiSection
        {
            public Spoils()
            {
                int am = 4;
                GRows rows = new GRows(am).setMin(100);
                foreach (RDResource res in RD.OUTPUT().RES)
                {
                    if (res.loot(result.besiged) > 0)
                    {
                        rows.add(new GStat()
                        {
                            public override void update(GText text)
                            {
                                GFORMAT.i(text, am(res));
                            }
                        }.hh(res.res.icon()));
                    }
                }

                add(new GScrollRows(rows.rows(), 28 * 3).view());

                addC(GCOLOR.UI().border().makeFrame(body().width() + 8, body().height() + 8, 1), body().cX(), body().cY());

                addRelBody(8, DIR.N, new GHeader(¤¤Plunder));
            }

            public int am(RDResource res)
            {
                return loot ? res.loot(result.besiged) * 8 : 0;
            }

            public int[] accepted()
            {
                int[] accepted = Alloc.ii(TR.ALL().size());
                foreach (RDResource res in RD.OUTPUT().RES)
                {
                    accepted[res.res.index()] += am(res);
                }
                return accepted;
            }
        }

        private class Slaves : GuiSection
        {
            public Slaves()
            {
                int am = 4;
                GRows rows = new GRows(am).setMin(100);

                foreach (RDRace race in RD.RACES().all)
                {
                    if (race.pop.get(result.besiged) <= 0)
                        continue;

                    rows.add(new GStat()
                    {
                        public override void update(GText text)
                        {
                            GFORMAT.i(text, slaves(race));
                        }
                    }.hh(race.race.appearance().icon));
                }

                addRelBody(4, DIR.S, new GScrollRows(rows.rows(), 24 * 2).view());

                addC(GCOLOR.UI().border().makeFrame(body().width() + 8, body().height() + 8, 1), body().cX(), body().cY());

                addRelBody(8, DIR.N, new GHeader(HTYPES.PRISONER().names));
            }

            private int slaves(RDRace race)
            {
                if (enslave)
                    return (int)(race.pop.get(result.besiged) * 0.3);
                return 0;
            }

            public int[] accepted()
            {
                int[] accepted = Alloc.ii(RACES.all().size());

                if (enslave)
                {
                    foreach (RDRace race in RD.RACES().all)
                    {
                        accepted[race.race.index] = slaves(race);
                    }
                }
                return accepted;
            }
        }
    }
}