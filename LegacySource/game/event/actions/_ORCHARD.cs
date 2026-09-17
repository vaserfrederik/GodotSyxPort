using System;
using System.Collections.Generic;
using game;
using game.event.engine;
using settlement.main;
using settlement.room.food.orchard;
using settlement.room.main;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;

namespace game.event.actions
{
    internal class _ORCHARD : EventActionConstructor
    {
        public _ORCHARD()
            : base("ORCHARD")
        {
        }

        public override EventAction Action(Data data)
        {
            return new Imp(Key, data.Json, data.All);
        }

        public sealed class Imp : EventAction
        {
            private readonly ROOM_ORCHARD room;
            private readonly double amount;

            public Imp(string key, Json data, LISTE<EventAction> all)
                : base(key, all)
            {
                RoomBlueprint b = SETT.ROOMS().Collection.GetWarn(data.Value("ROOM"), data);

                if (b != null && b is ROOM_ORCHARD)
                    this.room = (ROOM_ORCHARD)b;
                else
                {
                    this.room = null;
                    GAME.Warn(data.ErrorGet("no orchard room named: " + data.Value("ROOM"), "ROOM"));
                }
                amount = data.D("AREA_AFFECTED", 0, 1);
                data.CheckUnused();
            }

            public override void Exe(Event e, EContext data)
            {
                int area = (int)(this.amount * room.TotalArea());
                double aa = 0;
                int r = RND.rInt() & int.MaxValue;
                bool first = true;

                for (int i = 0; i < room.InstancesSize(); i++)
                {
                    RoomInstance ro = room.GetInstance((i + r) % room.InstancesSize());
                    if (first || ro.Area() <= area)
                    {
                        room.Event(ro.Mx(), ro.My(), 1.0);
                        data.Coo.Set(ro.Body().Cx(), ro.Body().Cy());
                        area -= ro.Area();
                        first = false;
                        aa += ro.Area();
                    }
                }

                double per = aa / room.TotalArea();
                data.ActionAmount = per;
            }
        }
    }
}