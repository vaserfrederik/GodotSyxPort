using System;
using System.Collections.Generic;
using settlement.room.industry.module.consumption;
using game.boosting;
using init.resources;
using init.sprite;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.industry.module;
using settlement.room.main;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.data;
using util.info;
using util.text;

public class RoomConsumption : RoomConsumptionAbs
{
    public readonly ArrayListGrower<ExtraInfo> extra = new ArrayListGrower<ExtraInfo>();

    public RoomConsumption(RoomBlueprintImp blue, Json json, Boostable bonus) : base(blue, bonus)
    {
        if (json.has("CONSUMPTION"))
        {
            json = json.json("CONSUMPTION");

            foreach (string k in json.keys())
            {
                RESOURCE res = RESOURCES.map().get(k, json);
                Json j = json.json(k);
                double rate = j.d("RATE", 0, 10000);

                new IndustryResourceIn(data, res, rate, rate, rate);
                extra.add(new ExtraInfo(j, data));
            }
        }

        INFO info = new INFO(Dic.¤¤Resources, "");

        double m = 1;
        foreach (IndustryResource r in allIns)
        {
            m += boost(r);
        }
        double max = m;

        roomBoosts.add(new RoomBoost()
        {
            public INFO info() => info;

            public double get(RoomInstance r)
            {
                double m = 1;
                foreach (IndustryResource res in allIns)
                {
                    if (stored(res).get((ROOM_IDATA_INSTANCE)r) > 0)
                        m += boost(res);
                }
                return m;
            }

            public double max() => max;

            public double min() => 1;
        });

        SPRITE icon = SPRITES.icons().l.star.twin(blue.iconBig(), DIR.C, 1);

        conBonus = BOOSTING.push("CONSUMPTION_" + blue.key, 1, Dic.¤¤ConsumptionRate + ": " + blue.info.name, Dic.¤¤ConsumptionRate + ": " + blue.info.name, icon, BOOSTABLES.CONSUMPTION());
    }

    public bool enabled(IndustryResource res, ROOM_IDATA_INSTANCE ins)
    {
        return extra.get(res.index()).enabled.get(ins) == 1;
    }

    public void enabledToggle(IndustryResource res, ROOM_IDATA_INSTANCE ins, RoomInstance i)
    {
        extra.get(res.index()).enabled.set(ins, (extra.get(res.index()).enabled.get(ins) + 1) & 1);
        ins.getWork().resetResourceSearch();
        if (!enabled(res, ins))
        {
            releaseResources(i, ins);
        }
    }

    public double boost(IndustryResource res)
    {
        return extra.get(res.index()).boost;
    }

    public INT_OE<ROOM_IDATA_INSTANCE> stored(IndustryResource res)
    {
        return extra.get(res.index()).amount;
    }

    public INT_OE<ROOM_IDATA_INSTANCE> reseved(IndustryResource res)
    {
        return extra.get(res.index()).reserved;
    }

    public bool shouldFecth(IndustryResource r, ROOM_IDATA_INSTANCE ins, RoomInstance emp)
    {
        if (!enabled(r, ins))
            return false;
        if (!ins.getWork().resourceShouldSearch(r.resource))
            return false;

        double am = extra.get(r.index()).amount.get(ins);
        double res = extra.get(r.index()).reserved.get(ins);
        double min = Math.Ceiling(emp.employees().employed() * r.rate);
        return am + res < min;
    }

    public class ExtraInfo
    {
        public readonly double boost;
        public readonly INT_OE<ROOM_IDATA_INSTANCE> enabled;
        public readonly INT_OE<ROOM_IDATA_INSTANCE> amount;
        public readonly INT_OE<ROOM_IDATA_INSTANCE> reserved;

        public ExtraInfo(Json j, DataOSimple<ROOM_IDATA_INSTANCE> data)
        {
            boost = j.d("BONUS", 0, 1000);
            enabled = data.newDataBit();
            amount = data.newDataInt();
            reserved = data.newDataShort();
        }
    }

    public interface ROOM_CONSUMPTION_HASER
    {
        RoomConsumption consumption();
    }

    public void releaseResources(RoomInstance ins, ROOM_IDATA_INSTANCE insc)
    {
        double stations = 0;

        foreach (COORDINATE c in ins.body())
        {
            if (ins.is(c) && insc.getWork().getJob(c) != null)
            {
                stations++;
            }
        }

        if (stations > 0)
        {
            foreach (IndustryResource res in ins())
            {
                int delta = (int)Math.Ceiling(stored(res).get(insc) / stations);

                foreach (COORDINATE c in ins.body())
                {
                    if (ins.is(c) && insc.getWork().getJob(c) != null)
                    {
                        int am = Math.Min(delta, stored(res).get(insc));
                        if (am > 0)
                        {
                            for (int di = 0; di < DIR.ALL.size(); di++)
                            {
                                int dx = c.x() + DIR.ALL.get(di).x();
                                int dy = c.y() + DIR.ALL.get(di).y();
                                if (!SETT.PATH().solidity.is(dx, dy))
                                {
                                    SETT.THINGS().resources.create(dx, dy, res.resource, am);
                                    stored(res).inc(insc, -am);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        foreach (IndustryResource res in ins())
        {
            int am = stored(res).get(insc);
            if (am > 0)
            {
                SETT.THINGS().resources.create(ins.mX(), ins.mY(), res.resource, am);
            }
            stored(res).set(insc, 0);
        }
    }

    public override double consumptionRate(RoomInstance ins, Humanoid h, IndustryResource res)
    {
        ROOM_IDATA_INSTANCE insi = (ROOM_IDATA_INSTANCE)ins;
        if (stored(res).get(insi) > 0)
        {
            return base.consumptionRate(ins, h, res);
        }
        return 0;
    }
}