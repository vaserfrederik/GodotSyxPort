using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.misc;
using snake2d.util.sets;

namespace game.audio
{
    public sealed class AmbianceUpdater
    {
        private readonly Channel[] channels = new Channel[] { new Channel(), new Channel(), new Channel(), new Channel(), new Channel() };
        private Tree<Ambiance> aSort;
        private readonly ArrayList<Channel> cfree = new ArrayList<Channel>(channels.Length);
        private readonly ArrayList<Ambiance> toPlay = new ArrayList<Ambiance>(channels.Length);
        private readonly Ambiances aaa;

        public AmbianceUpdater(Ambiances aaa)
        {
            this.aaa = aaa;
        }

        private double last = -100;

        public double[] debugPrio;
        public double[] debugGain;

        public void Update()
        {
            double ds = VIEW.renderSecond() - last;
            if (ds < 0.1)
                return;
            last = VIEW.renderSecond();

            if (debugPrio != null)
            {
                foreach (Ambiance a in aaa.all())
                {
                    a.priority = debugPrio[a.index()];
                    if (a.priority > 0)
                    {
                        a.gainSet(debugGain[a.index()]);
                    }
                }
                debugPrio = null;
                debugGain = null;
            }
            if (aSort == null || aSort.capacity() != aaa.all().size())
            {
                aSort = new Tree<Ambiance>(aaa.all().size())
                {
                    protected override bool isGreaterThan(Ambiance current, Ambiance cmp)
                    {
                        return current.priority > cmp.priority;
                    }
                };
            }

            aSort.clear();
            foreach (Ambiance a in aaa.all())
            {
                if (a.priority > 0)
                {
                    aSort.add(a);
                }
            }

            toPlay.clearSloppy();

            int ci = 0;
            while (aSort.hasMore())
            {
                Ambiance a = aSort.pollGreatest();
                if (ci < channels.Length)
                {
                    if (a.channel == null)
                        toPlay.add(a);
                }
                else
                {
                    a.priority = 0;
                }
                ci++;
            }

            cfree.clearSloppy();
            foreach (Channel c in channels)
            {
                c.update(ds);
                if (c.current == null)
                {
                    cfree.add(c);
                }
            }

            for (int i = 0; i < toPlay.size() && i < cfree.size(); i++)
                cfree.get(i).init(toPlay.get(i));
        }

        private sealed class Channel
        {
            private Ambiance current;
            private SoundStream stream;
            private double gain;

            public void update(double ds)
            {
                if (current == null)
                    return;

                if (!stream.isPlaying())
                {
                    if (current.priority > 0)
                    {
                        init(current);
                    }
                    else
                    {
                        current.channel = null;
                        current = null;
                    }
                    return;
                }

                if (current.priority <= 0)
                {
                    gain -= ds;
                    if (gain <= 0)
                    {
                        gain = 0;
                        stream.stop();
                    }
                }
                else
                {
                    double g = current.gain();
                    if (gain < g)
                    {
                        gain += ds;
                        if (gain > g)
                            gain = g;
                    }
                    else if (gain > g)
                    {
                        gain -= ds;
                        if (gain < g)
                            gain = g;
                    }
                }

                gain = CLAMP.d(gain, 0, 1);
                stream.setGain(gain);
            }

            public void init(Ambiance c)
            {
                c.channel = this;
                current = c;
                stream = c.streams.rnd();
                stream.setLooping(false);
                gain = 0;
                stream.setGain(gain);
                stream.play();
            }
        }
    }
}