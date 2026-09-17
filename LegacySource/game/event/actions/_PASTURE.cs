using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Game.Event.Actions
{
    public class _PASTURE : EventActionConstructor
    {
        public _PASTURE()
            : base("PASTURE")
        {
        }

        public override EventAction Action(Data data)
        {
            return new Imp(Key, data.Json, data.All);
        }

        public sealed class Imp : EventAction
        {
            private readonly ROOM_PASTURE room;
            private readonly double amount;

            public Imp(string key, JObject data, List<EventAction> all)
                : base(key, all)
            {
                RoomBlueprint b = SETT.ROOMS().collection.GetWarn(data.Value<string>("ROOM"), data);

                if (b != null && b is ROOM_PASTURE)
                    this.room = (ROOM_PASTURE)b;
                else
                {
                    this.room = null;
                    GAME.Warn(data.ErrorGet("no pasture named: " + data.Value<string>("ROOM"), "ROOM"));
                }
                amount = data.Value<double>("ANIMALS_KILLED", 0, 1);
                data.CheckUnused();
            }

            public override void Exe(Event evt, EContext data)
            {
                if (room == null)
                    return;

                ROOM_PASTURE p = room;

                if (p.InstancesSize() == 0)
                    return;

                double death = amount;

                double am = 0;
                int tot = 0;

                for (int i = 0; i < p.InstancesSize(); i++)
                {
                    PastureInstance ins = p.GetInstance(i);
                    data.Coo.Set(ins.Body().CX(), ins.Body().CY());
                    tot += ins.AnimalsCurrent();
                    int d = (int)Math.Ceiling(Math.Ceiling(ins.AnimalsCurrent() * death));
                    ins.Kill(d);
                    am += d;
                }

                double per = am / tot;
                data.ActionAmount = per;
            }
        }
    }
}