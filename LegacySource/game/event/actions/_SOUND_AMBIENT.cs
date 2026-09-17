using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;
using view.main;

namespace game.event.actions
{
    final class _SOUND_AMBIENT : EventActionConstructor
    {
        _SOUND_AMBIENT() : base("SOUND_AMBIENT") { }

        public override EventAction action(Data data)
        {
            return new Imp(key, data.json, data.all);
        }

        public final class Imp : EventAction
        {
            private readonly LIST<SoundStream> stream;
            private readonly bool city;
            private readonly bool world;

            Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                stream = AUDIO.AMBI().factory.read(data);
                city = data.bool("CITY", true);
                world = data.bool("WORLD", false);
                data.checkUnused();
            }

            public override void update(Event event, EContext e, double ds, double second)
            {
                if (!city && VIEW.s().isActive() || VIEW.s().battle.isActive())
                    return;

                if (!world && VIEW.world().isActive())
                    return;

                foreach (SoundStream s in stream)
                    s.play();

                base.update(event, e, ds, second);
            }
        }
    }
}