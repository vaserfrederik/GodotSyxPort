using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game;
using game.boosting;
using game.faction;
using game.faction.npc;
using game.faction.player;
using init.paths;
using init.sprite;
using init.value;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using world.map.regions;

namespace world.region.building
{
    public sealed class RDBuildPoints
    {
        public readonly LIST<RDBuildPoint> ALL;
        private readonly LIST<RDBuildPoint> all;
        public readonly RDBuildPoint GOV;
        // private bool dirty = true;

        public RDBuildPoints()
        {
            ArrayListGrower<RDBuildPoint> all = new ArrayListGrower<RDBuildPoint>();
            GOV = new RDBuildPoint(all.Size, BOOSTABLES.CIVICS().GOV);
            all.Add(GOV);

            ResFolder f = PATHS.WORLD().folder("point");
            foreach (string key in f.init.GetFiles())
            {
                Json d = new Json(f.init.gets(key));
                Json t = new Json(f.text.gets(key));
                RDBuildPoint p = new RDBuildPoint(all.Size, key, d, t);
                all.Add(p);
            }
            this.ALL = all;
            this.all = new ArrayList<RDBuildPoints.RDBuildPoint>(ALL);
        }

        public void SetDirty()
        {
            upI = -120;
        }

        public RDBuildPoint Get(Boostable bo)
        {
            for (int i = 0; i < all.Size; i++)
            {
                if (all.Get(i).bo == bo)
                    return all.Get(i);
            }
            return null;
        }

        public RDBuildPoint Get(Boostable bo, Booster b)
        {
            if (b.isMul)
            {
                if (b.to() >= 1)
                    return null;
            }
            else if (b.to() > 0)
                return null;
            return Get(bo);
        }

        private bool calcing = false;

        private int upI = -120;

        private bool Dirty()
        {
            return upI != GAME.updateI();
        }

        private void Calc()
        {
            if (calcing)
                return;
            calcing = true;

            upI = GAME.updateI();

            for (int i = 0; i < FACTIONS.player().realm().regions(); i++)
            {
                Region reg = FACTIONS.player().realm().region(i);
                foreach (RDBuildPoint co in all)
                {
                    co.eff[reg.index()] = 1;
                    co.lastValue[reg.index()] = -1;
                    co.consumed[reg.index()] = -1;
                }
            }

            int deathSwitch = 100;
            bool changed = true;
            while (changed && deathSwitch-- > 0)
            {
                changed = false;
                for (int ci = 0; ci < all.Size; ci++)
                {
                    RDBuildPoint co = all.Get(ci);

                    for (int ri = 0; ri < FACTIONS.player().realm().regions(); ri++)
                    {
                        Region r = FACTIONS.player().realm().region(ri);

                        double v = co.bo.get(r);
                        if (v != co.lastValue[r.index()])
                        {
                            co.lastValue[r.index()] = v;
                            changed = true;
                            co.consumed[r.index()] = 0;
                            for (int bi = 0; bi < co.bo.all().Size; bi++)
                            {
                                Booster b = co.bo.all().Get(bi);
                                double bv = b.get(r);
                                if (!b.isMul && bv < 0)
                                    co.consumed[r.index()] += bv;
                            }

                            if (v >= 0)
                                co.eff[r.index()] = 1;
                            else
                            {
                                double add = co.bo.baseValue;
                                double mul = 1;
                                for (int bi = 0; bi < co.bo.all().Size; bi++)
                                {
                                    Booster b = co.bo.all().Get(bi);
                                    double bv = b.get(r);
                                    if (b.isMul)
                                        mul *= bv;
                                    else if (bv > 0)
                                        add += bv;
                                }
                                if (mul > 1)
                                    add *= mul;
                                co.eff[r.index()] = CLAMP.d((add + v) / add, 0, 1);
                            }
                        }
                    }
                }
            }
            calcing = false;
        }

        public sealed class RDBuildPoint
        {
            public readonly int index;
            public readonly SPRITE icon;
            public readonly INFO info;
            public readonly Boostable bo;
            private readonly double[] lastValue = new double[WREGIONS.MAX];
            private readonly int[] consumed = Alloc.ii(WREGIONS.MAX);
            private readonly double[] eff = new double[WREGIONS.MAX];
            private readonly double BI = 1.0 / 1000000.0;

            public RDBuildPoint(int index, string key, Json data, Json text) : this(index, BOOSTING.push("POINT_" + key, 0, text.get("name"), text.get("desc"), SPRITES.icons().get(data), BoostableCat.ALL().WORLD))
            {
                GVALUES.REGION.push("EFFICIENCY_" + key, text.get("name"), SPRITES.icons().get(data), new DOUBLE_O<Region>()
                {
                    public double getD(Region t) => eff(t)
                });

                new BoostSpecs(text.get("name"), SPRITES.icons().get(data), true).read(data, new BValuePlayerOnly()
                {
                    public double vGet(Player f)
                    {
                        int am = 0;
                        for (int i = 0; i < f.realm().regions(); i++)
                            am += lastValue[f.realm().region(i).index()];
                        return am;
                    }

                    public double vGet(Region reg)
                    {
                        Calc();
                        return ((int)lastValue[reg.index()]) * BI;
                    }

                    public double vGet(FactionNPC f) => 0;
                });
            }

            public RDBuildPoint(int index, Boostable bo)
            {
                this.index = index;
                icon = bo.icon.medium;
                info = new INFO(bo.name, bo.desc);
                this.bo = bo;

                GVALUES.REGION.push("EFFICIENCY_" + bo.key, info.name, icon, new DOUBLE_O<Region>()
                {
                    public double getD(Region t) => eff(t)
                });
            }

            public double eff(Region reg)
            {
                if (reg.faction() != FACTIONS.player())
                    return 1;

                if (Dirty() || lastValue[reg.index()] != bo.get(reg))
                {
                    Calc();
                }
                return eff[reg.index()];
            }

            public int consumed(Region reg)
            {
                Calc();
                return consumed[reg.index()];
            }

            public int consumed(Faction f)
            {
                Calc();
                int am = 0;
                for (int ri = 0; ri < f.realm().regions(); ri++)
                {
                    Region r = f.realm().region(ri);
                    am += consumed[r.index()];
                }
                return -am;
            }

            public void hover(GUI_BOX box, Region reg)
            {
                GBox b = (GBox)box;
                box.title(bo.name);
                box.text(bo.desc);
                box.NL();

                bo.hoverDetailed(box, reg, Dic.¤¤Produced, true);
                b.NL();

                // b.textLL(¤¤allocated);
                // b.tab(6);
                // b.add(GFORMAT.iIncr(b.text(), -allocated));
                // b.NL();
                //
                // b.textLL(¤¤frozen);
                // b.tab(6);
                // b.add(GFORMAT.iIncr(b.text(), -frozen()));
                // b.NL();
                //
                // b.sep();
                //
                // b.textLL(¤¤available);
                // b.tab(6);
                // b.add(GFORMAT.iIncr(b.text(), available()));
                // b.NL();
                //
                // b.textLL(¤¤penalty);
                // b.tab(6);
                // b.add(GFORMAT.percInv(b.text(), penalty));
                // b.NL();
            }
        }
    }
}