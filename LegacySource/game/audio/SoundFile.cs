using System;
using System.Numerics;
using System.IO;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.keymap;

namespace game.audio
{
    public sealed class SoundFile : MAPPED
    {
        private readonly int index;
        private readonly string key;
        public readonly SoundEffect sound;
        public double gain = 1.0;
        public double pitch = 0.3;

        public SoundFile(LISTE<SoundFile> all, Path p, string key)
        {
            sound = CORE.getSoundCore().getEffect(p);
            index = all.add(this);
            this.key = key;
        }

        public SoundFile(LISTE<SoundFile> all, SoundEffect p, string key)
        {
            sound = p;
            index = all.add(this);
            this.key = key;
        }

        public override int index()
        {
            return index;
        }

        public override string key()
        {
            return key;
        }

        public void rnd(RECTANGLE body)
        {
            rnd(body, 0.8f + RND.rFloat(0.2));
        }

        public void rnd(RECTANGLE body, double gain)
        {
            rnd(body.cX(), body.cY(), gain);
        }

        public void rnd(int x, int y, double gain)
        {
            gain *= this.gain;
            if (gain <= 0)
                return;
            float pitch = RND.rFloat1((float)this.pitch);
            sound.play(x, y, pitch, (float)gain, false);
        }
    }
}