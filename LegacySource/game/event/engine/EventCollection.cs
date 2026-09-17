using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game;
using game.event.actions;
using init.paths.PATHS;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.event.engine
{
    public class EventCollection
    {
        private readonly KeyMap<Event> map = new KeyMap<Event>();
        public readonly ArrayListGrower<Event> all = new ArrayListGrower<Event>();

        public EventCollection(ResFolder fo) throws IOException
        {
            if (GAME.EVENT() != null)
                throw new Errors.DataError("This must be done before events are setup");

            KeyMap<int> occMap = new KeyMap<int>();
            foreach (string file in fo.init.getFiles())
            {
                Json jfile = new Json(fo.init.gets(file));
                Json jtext = fo.text.exists(file) ? new Json(fo.text.gets(file)) : null;
                foreach (string pkey in jfile.keys())
                {
                    string key = file + "_" + pkey;

                    Json text = jtext != null && jtext.has(pkey) ? jtext.json(pkey) : null;
                    Json d = jfile.json(pkey);
                    map.put(key, new Event(all, key, d, text));

                    if (d.has("OCCURENCE"))
                    {
                        d = d.json("OCCURENCE");
                        if (d.has("TYPE"))
                        {
                            int i = 0;
                            string t = d.value("TYPE");
                            if (occMap.containsKey(t))
                            {
                                i += occMap.get(t);
                            }
                            occMap.putReplace(t, i);
                        }
                    }
                }
            }

            EventActions actions = new EventActions(this);

            foreach (string file in fo.init.getFiles())
            {
                Json jfile = new Json(fo.init.gets(file));
                Json jtext = fo.text.exists(file) ? new Json(fo.text.gets(file)) : null;
                foreach (string pkey in jfile.keys())
                {
                    string key = file + "_" + pkey;
                    Json d = jfile.json(pkey);
                    Json text = jtext != null && jtext.has(pkey) ? jtext.json(pkey) : null;
                    map.get(key).read(d, text, actions, this);
                    if (d.has("OCCURENCE"))
                    {
                        d = d.json("OCCURENCE");
                        if (d.has("TYPE"))
                        {
                            int i = occMap.get(d.value("TYPE"));
                            for (int di = 0; di < map.get(key).occurence.coccurence.Length; di++)
                            {
                                map.get(key).occurence.coccurence[di] /= i;
                            }
                        }
                    }
                }
            }
            actions.init();
        }

        private bool hasError = false;

        public Event read(Event parent, string k, Json error, string kk)
        {
            Event e = map.get(k);
            if (e == null)
            {
                string f = parent.key.Split("_")[0];
                e = map.get(f + "_" + k);
            }
            if (e == null)
            {
                string ee = error.errorGet("no event named: " + k + (k.EndsWith(" ") ? "It ends with space!" : "") + k, kk);

                if (!hasError)
                {
                    string av = "   Available: " + Environment.NewLine;
                    av += map.keysString();
                    ee += Environment.NewLine + av;
                    hasError = true;
                }

                LOG.err(ee);
                return null;
            }
            return e;
        }
    }
}