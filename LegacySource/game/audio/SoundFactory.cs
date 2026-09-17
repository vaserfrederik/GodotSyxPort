using System.Collections.Generic;
using System.IO;
using System.Numerics;
using game;
using init.paths;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.audio
{
    final class SoundFactory
    {
        double sGain;

        private readonly KeyMap<Sound> map = new KeyMap<Sound>();

        public readonly AudioFactory<SoundFile> factory = new Factory();
        public readonly Sound DUMMY = new Sound(new ArrayList<SoundFile>(factory.DUMMY()));

        SoundFactory()
        {
        }

        public Sound get(string key)
        {
            if (!map.ContainsKey(key))
            {
                GAME.Warn("no sound by the key of: " + key);
                return new Sound(factory.LDUMMY());
            }
            return map[key];
        }

        public Sound read(Json json)
        {
            return read("SOUND", json);
        }

        public Sound read(string key, Json json)
        {
            LIST<SoundFile> ss = factory.read(key, json);
            return new Sound(ss);
        }

        public void settGain(double gain)
        {
            this.sGain = gain;
        }

        private class Factory : AudioFactory<SoundFile>
        {
            public Factory() : base("SOUND", PATHS.AUDIO().mono, new SoundFile(new LinkedList<SoundFile>(), new SoundEffect.Dummy(), "DUMMY"))
            {
            }

            protected override SoundFile create(LinkedList<SoundFile> all, Path p, string key)
            {
                return new SoundFile(all, p, key);
            }
        }
    }
}