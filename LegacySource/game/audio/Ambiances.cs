using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game;
using init.paths;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;
using util.keymap;

namespace game.audio
{
    public sealed class Ambiances
    {
        private readonly RMAP<Ambiance> rmap;
        //private readonly ArrayListGrower<Ambiance> all = new ArrayListGrower<>();

        public readonly Ambiance nature;
        public readonly Ambiance wind;
        public readonly Ambiance night;
        public readonly Ambiance water;
        public readonly Ambiance rain;
        public readonly Ambiance windTrees;
        public readonly Ambiance windhowl;
        public readonly Ambiance thunder;
        private readonly AudioFactory<SoundStream> factory;
        private bool debugged = false;

        public Ambiances()
        {
            factory = new AudioFactory<SoundStream>("AMBIANCE", PATHS.AUDIO().ambience, new SoundStream.Dummy())
            {
                Create = (LinkedList<SoundStream> all, Path p, string key) =>
                {
                    return CORE.GetSoundCore().GetStream(p, false);
                }
            };

            var all = new LinkedList<Ambiance>();

            PATH p = PATHS.AUDIO().config.GetFolder("ambience");
            foreach (string file in p.GetFiles())
            {
                Json json = new Json(p.GetS(file));
                LIST<string> keys = json.Keys();

                foreach (string k in keys)
                {
                    new Ambiance(k, all, factory.Read(k, json));
                }
            }

            rmap = new RMAP<Ambiance>("AMBIENCE", all);

            nature = Get("NATURE");
            wind = Get("WIND");
            night = Get("NIGHT");
            water = Get("WATER");
            rain = Get("RAIN");
            windTrees = Get("WIND_TREES");
            windhowl = Get("CAVE");
            thunder = Get("THUNDER");
        }

        public Ambiance Get(string key)
        {
            if (rmap.TryGet(key) == null)
            {
                if (!debugged)
                {
                    string a = "Available " + Environment.NewLine;
                    foreach (string s in rmap.Available())
                    {
                        a += s + Environment.NewLine;
                    }

                    GAME.Warn("no ambiance sound by the key of: " + key + Environment.NewLine + a);
                    debugged = true;
                }
                else
                {
                    Console.Error.WriteLine("no ambiance sound by the key of: " + key);
                }

                return null;
            }
            return rmap.TryGet(key);
        }

        public LIST<Ambiance> All()
        {
            return rmap.All();
        }
    }
}