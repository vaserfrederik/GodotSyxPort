using System;
using System.Collections.Generic;
using Json = Newtonsoft.Json.Linq.JObject;
using snake2d.util.file;
using snake2d.util.sets;
using game.event.engine;
using init.type;

namespace game.event.actions
{
    final class _OUTBREAK : EventActionConstructor
    {
        _OUTBREAK()
        {
            base("OUTBREAK");
        }

        public override EventAction action(Data data)
        {
            return new Imp(key, data.json, data.all);
        }

        public final class Imp : EventAction
        {
            private readonly double amount;
            public readonly DISEASE disease;
            private CInt am = new CInt("AFFLICTED");

            Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                amount = data.Value<double>("AMOUNT", 0, 1);
                disease = DISEASES.map().readTry(data);
                data.checkUnused();
            }

            public override void exe(Event eventObj, EContext data)
            {
                if (disease != null)
                {
                    int am = STATS.DISEASE().incubating().data().get(null) + STATS.DISEASE().sick().data().get(null);
                    STATS.DISEASE().outbreak(amount, disease);
                    am = STATS.DISEASE().incubating().data().get(null) + STATS.DISEASE().sick().data().get(null) - am;
                    this.am.set(eventObj, data, am);
                }
            }
        }
    }
}