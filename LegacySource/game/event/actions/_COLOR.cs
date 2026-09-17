using System.Collections.Generic;
using game.event.engine;
using snake2d.util.color;
using snake2d.util.file;

namespace game.event.actions
{
    internal class _COLOR : EventActionConstructor
    {
        public _COLOR()
            : base("COLOR")
        {
        }

        public override EventAction action(Data data)
        {
            return new Imp(key, data.json, data.all);
        }

        public class Imp : EventAction
        {
            public readonly ColorImp color;
            private readonly bool useSelection;

            public Imp(string key, Json data, List<EventAction> all)
                : base(key, all)
            {
                color = new ColorImp(data);
                useSelection = data.boolValue("USE_SELECTION", false);
                data.checkUnused();
            }

            public override void exe(Event eventObj, EContext data)
            {
                if (useSelection)
                    data.colorIndu = color;
                else
                    data.colorinduAll = color;
            }
        }
    }
}