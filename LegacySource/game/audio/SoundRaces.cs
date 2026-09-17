using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using util.gui.misc;
using util.keymap;

namespace game.audio
{
    final class SoundRaces
    {
        private readonly RMAP<SoundRace> rmap;
        private bool debugged = false;
        private readonly SoundRace RDUMMY;

        SoundRaces(SoundFactory factory)
        {
            var all = new LinkedList<SoundRace>();

            PATH p = PATHS.AUDIO().config.getFolder("mono");
            foreach (var file in p.getFiles())
            {
                Json json = new Json(p.gets(file));
                LIST<String> keys = json.keys();

                foreach (var k in keys)
                {
                    all.add(new SoundRace(all.size(), k, factory.read(k, json)));
                }
            }

            rmap = new RMAP<SoundRace>("SOUND", all);
            RDUMMY = new SoundRace(0, "DUMMY", new Sound(factory.factory.LDUMMY()));

            GButt.defaultHoverSound = get("UI_HOVER");
            GButt.defaultClickSound = get("UI_CLICK");
        }

        public SoundRace get(String key)
        {
            if (rmap.tryGet(key) == null)
            {
                if (!debugged)
                {
                    string a = "Available " + Environment.NewLine;
                    foreach (var s in rmap.available())
                    {
                        a += s + Environment.NewLine;
                    }

                    GAME.Warn("no race sound by the key of: " + key + Environment.NewLine + a);
                    debugged = true;
                }
                else
                {
                    Console.Error.WriteLine("no race sound by the key of: " + key);
                }

                return RDUMMY;
            }
            return rmap.tryGet(key);
        }
    }
}