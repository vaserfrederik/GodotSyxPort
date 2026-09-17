using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.event.actions
{
    using game.event.engine;
    using snake2d.util.file;
    using snake2d.util.rnd;
    using snake2d.util.sets;
    using view.main;

    internal sealed class _EARTHQUAKE : EventActionConstructor
    {
        _EARTHQUAKE() : base("EARTHQUAKE") { }

        public override EventAction action(Data data)
        {
            return new Imp(key, data.json, data.all);
        }

        public sealed class Imp : EventAction
        {
            double acc;
            double vtime = 0;

            public Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                data.checkUnused();
            }

            public override void setContext(Event eventObj, EContext data)
            {
                acc = 0;
                vtime = 0;
            }

            public override void update(Event eventObj, EContext e, double ds, double second)
            {
                if (VIEW.s().isActive() && VIEW.renderSecond() > vtime)
                {
                    vtime = VIEW.renderSecond() + RND.rFloat() * 0.2;
                    if (ds > 0)
                        VIEW.s().getWindow().centerer.set(VIEW.s().getWindow().pixels().cX() + RND.rInt0(20), VIEW.s().getWindow().pixels().cY() + RND.rInt0(20));
                }
            }
        }
    }
}