using System;
using System.Collections.Generic;
using game;
using game.boosting;
using game.faction;
using game.time;
using init.race;
using init.resources;
using init.sprite.UI;
using init.trade;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.industry.module.consumption;
using settlement.room.main;
using settlement.room.spirit.temple;
using settlement.stats.equip;
using snake2d.util.misc;
using snake2d.util.sets;
using util.text;
using world.regions;

namespace settlement.room.industry
{
    public class RoomProduction
    {
        private List<Res> producers;
        private List<Res> consumers;
        private List<Res> eaters;

        private int ui = 0;
        private int upI = 0;

        public RoomProduction()
        {
            producers = new List<Res>();
            consumers = new List<Res>();
            eaters = new List<Res>();

            for (int i = 0; i < RESOURCE.Count; i++)
            {
                producers.Add(new Res(RESOURCE.Get(i)));
                consumers.Add(new Res(RESOURCE.Get(i)));
                eaters.Add(new Res(RESOURCE.Get(i)));
            }

            GAME.UpdateIChanged += Update;
        }

        private void Update(int ticks)
        {
            upI = GAME.updateI();
            Entity[] es = SETT.ENTITIES().getAllEnts();

            int tott = ticks * 200;
            if (tott < 0 || tott > es.Length)
                tott = es.Length;

            for (int i = 0; i < tott; i++)
            {
                if (ui >= es.Length)
                {
                    for (int ii = 0; ii < producers.Count; ii++)
                    {
                        Res r = producers[ii];
                        double tot = 0;
                        foreach (SourceR in in r.ins)
                        {
                            in.am = in.old;
                            in.old = 0;
                            tot += in.am;
                        }
                        foreach (Source in in r.all)
                        {
                            if (in is SourceReg)
                                tot += in.am();
                        }
                        r.am = tot;
                    }
                    for (int ii = 0; ii < consumers.Count; ii++)
                    {
                        Res r = consumers[ii];
                        double tot = 0;
                        foreach (SourceR in in r.ins)
                        {
                            in.am = in.old;
                            in.old = 0;
                            tot += in.am;
                        }

                        r.am = tot;
                    }

                    ui = 0;
                    break;
                }

                Entity e = es[ui];
                ui++;
                if (e != null && e is Humanoid)
                {
                    Humanoid h = e as Humanoid;
                    RoomInstance ins = STATS.WORK().EMPLOYED.get(h);
                    if (ins == null)
                        continue;

                    if (ins is ROOM_PRODUCER_INSTANCE)
                    {
                        ROOM_PRODUCER_INSTANCE p = ins as ROOM_PRODUCER_INSTANCE;

                        Industry in = p.industry();
                        foreach (IndustryResource oo in in.outs())
                        {
                            RESOURCE res = oo.resource;
                            double d = p.productionRate(ins, h, in, oo);
                            for (int ri = 0; ri < producers[res.index()].ins.Count; ri++)
                            {
                                SourceR ii = producers[res.index()].ins[ri];
                                if (ii.blue == in.blue && ii.ins == in)
                                {
                                    ii.old += d;
                                }
                            }
                        }
                        foreach (IndustryResource oo in in.ins())
                        {
                            RESOURCE res = oo.resource;
                            double d = in.consumptionRate(ins, h, oo);
                            for (int ri = 0; ri < consumers[res.index()].ins.Count; ri++)
                            {
                                SourceR ii = consumers[res.index()].ins[ri];
                                if (ii.blue == in.blue && ii.ins == in)
                                {
                                    ii.old += d;
                                }
                            }
                        }
                    }

                    if (ins.blueprintI() is ROOM_CONSUMPTION_HASER)
                    {
                        RoomConsumptionAbs in = (ROOM_CONSUMPTION_HASER)ins.blueprintI()).consumption();
                        for (int rii = 0; rii < in.ins().Count; rii++)
                        {
                            IndustryResource oo = in.ins()[rii];
                            RESOURCE res = oo.resource;
                            double d = in.consumptionRate(ins, h, oo);
                            for (int ri = 0; ri < consumers[res.index()].ins.Count; ri++)
                            {
                                SourceR ii = consumers[res.index()].ins[ri];
                                if (ii.blue == in.blue)
                                {
                                    ii.old += d;
                                }
                            }
                        }

                    }
                }
            }
        }

        public double Produced(RESOURCE res)
        {
            if (Math.Abs(upI - GAME.updateI()) > 1)
            {
                Update(Math.Abs(upI - GAME.updateI()));
            }
            return producers[res.index()].am;
        }

        public double Consumed(RESOURCE res)
        {
            if (Math.Abs(upI - GAME.updateI()) > 1)
            {
                Update(Math.Abs(upI - GAME.updateI()));
            }
            return consumers[res.index()].am;
        }

        public List<Source> Producers(RESOURCE res)
        {
            return producers[res.index()].all;
        }

        public List<Source> Consumers(RESOURCE res)
        {
            return consumers[res.index()].all;
        }

        public List<Source> Eaters(RESOURCE res)
        {
            return eaters[res.index()].all;
        }

        private class Res
        {
            private List<Source> all = new List<Source>();
            private List<SourceR> ins = new List<SourceR>();
            private double am;

            public Res(RESOURCE res)
            {
            }

            private void Init()
            {
                for (int i1 = 0; i1 < ins.Count; i1++)
                {
                    for (int i2 = 0; i2 < ins.Count; i2++)
                    {
                        if (i2 == i1)
                            continue;
                        if (ins[i1].blue == ins[i2].blue)
                        {
                            ins[i1].multiple = true;
                            ins[i2].multiple = true;
                        }

                    }

                }
            }
        }

        public abstract class Source
        {
            public readonly RESOURCE res;

            public Source(RESOURCE res)
            {
                this.res = res;
            }

            public abstract double am();

            public virtual Industry ThereAreMultipleIns()
            {
                return null;
            }

            public abstract SPRITE Icon();

            public abstract string Name();
        }

        public class SourceReg : Source
        {
            public SourceReg(RESOURCE res) : base(res)
            {
            }

            public override double am()
            {
                int am = 0;
                for (int i = 0; i < FACTIONS.player().realm().regions(); i++)
                {
                    int aa = RD.OUTPUT().get(TR.get(res)).getDelivery(FACTIONS.player().realm().region(i));
                    am += aa;

                }
                return am;
            }

            public override Industry ThereAreMultipleIns()
            {
                return null;
            }

            public override SPRITE Icon()
            {
                return UI.icons().s.money;
            }

            public override string Name()
            {
                return Dic.¤¤Taxes;
            }
        }

        public class SourceR : Source
        {
            private readonly RoomBlueprintImp blue;
            private readonly Industry ins;
            private double old;
            private double am;
            private bool multiple = false;

            public SourceR(RESOURCE res, RoomBlueprintImp blue, Industry ins) : base(res)
            {
                this.blue = blue;
                this.ins = ins;
            }

            public override double am()
            {
                if (Math.Abs(upI - GAME.updateI()) > 1)
                {
                    Update(Math.Abs(upI - GAME.updateI()));

                }
                return am;
            }

            public override Industry ThereAreMultipleIns()
            {
                return multiple ? ins : null;
            }

            public override SPRITE Icon()
            {
                return blue.icon.small;
            }

            public override string Name()
            {
                return blue.info.names;
            }
        }

        public class SourceRC : SourceR
        {
            public SourceRC(RESOURCE res, RoomBlueprintImp blue) : base(res, blue, null)
            {
            }
        }
    }
}