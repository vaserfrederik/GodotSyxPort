using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Water
{
    public abstract class RoomIrrigated
    {
        public readonly double From;
        public readonly double To;

        public RoomIrrigated(ROOM_IRRIGATED blue, Boostable bo, double from, double to)
        {
            From = from;
            To = to;
            BSourceInfo inInfo = new BSourceInfo(Ground.Moisture, UI.Icons().s.drop.CreateColored(COLOR.Blueish));
            Booster bos = new BoosterImp(inInfo, from, to, true)
            {
                vGet = (Induvidual indu) =>
                {
                    RoomInstance ins = STATS.Work().Employed.Get(indu);
                    if (ins != null && ins.Blueprint() == blue)
                    {
                        return CLAMP.d(irrigation(ins) / needed(ins), 0, 1);
                    }
                    return 1;
                },

                vGetPlayer = (Player f) => vGet(HCLASS_RACE.ClP()),

                vGetHCLASS_RACE = (HCLASS_RACE popTime) =>
                {
                    if (blue is RoomBlueprintIns<?>)
                    {
                        RoomBlueprintIns<?> p = (RoomBlueprintIns<?>)blue;
                        if (Math.Abs(GAME.UpdateI() - ci) >= 120)
                        {
                            ci = GAME.UpdateI();
                            c = 0;
                            int am = 0;
                            for (int i = 0; i < p.InstancesSize(); i++)
                            {
                                RoomInstance ins = p.GetInstance(i);
                                int e = ins.Employees().Employed();
                                c += e * CLAMP.d(irrigation(ins) / needed(ins), 0, 1);
                                am += e;
                            }

                            if (am != 0)
                            {
                                c /= am;
                            }
                            else
                            {
                                c = 1.0;
                            }
                        }
                    }
                    return c;
                },

                vGetFactionNPC = (FactionNPC f) => 1.0,

                vGetFaction = (Faction f) => 0,

                Get = (BOOSTABLE_O o) =>
                {
                    if (o is FactionNPC)
                        return 1.0;
                    return base.Get(o);
                }
            };
            bos.Add(bo);
        }

        protected abstract double irrigation(RoomInstance ins);

        public interface ROOM_IRRIGATED
        {
            RoomIrrigated Irrigation();
        }

        public double ProspectFlat(AREA area)
        {
            double n = needed(area);
            if (n == 0)
                return 0;

            double w = 0;

            foreach (COORDINATE c in area.Body())
            {
                if (area.Is(c))
                {
                    w += SETT.Ground().MoistureTot.Get(c);
                }
            }
            return w / n;
        }

        public static double RawValue(AREA area)
        {
            double w = 0;

            foreach (COORDINATE c in area.Body())
            {
                if (area.Is(c))
                {
                    w += SETT.Ground().MoistureTot.Get(c);
                }
            }
            return w / area.Area();
        }

        public double ValueProspect(AREA area)
        {
            double n = ProspectFlat(area);
            return CLAMP.d(From + (To - From) * n, 0, 1);
        }

        public double Needed(AREA area)
        {
            return area.Area();
        }

        public double Current(RoomInstance ins)
        {
            return CLAMP.d(irrigation(ins) / needed(ins), 0, 1);
        }
    }
}