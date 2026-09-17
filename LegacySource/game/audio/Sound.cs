using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sets;
using view.main;

namespace game.audio
{
    internal sealed class Sound
    {
        private static double playCount;
        public readonly LIST<SoundFile> all;

        public Sound(LIST<SoundFile> all)
        {
            this.all = all;
        }

        public void rnd(RECTANGLE body)
        {
            rnd(body, 0.8f + RND.rFloat(0.2f));
        }

        public void rnd(RECTANGLE body, double gain)
        {
            rnd(body.cX(), body.cY(), gain);
        }

        public void rnd(int cx, int cy)
        {
            rnd(cx, cy, 0.8f + RND.rFloat(0.2f));
        }

        public void rnd(int x, int y, double gain)
        {
            playCount += GAME.SPEED.speedI();
            if (playCount >= 1)
            {
                playCount -= 1;
            }
            else
            {
                return;
            }

            if (VIEW.world().isActive())
            {
                return;
            }
            all.rnd().rnd(x, y, gain * AUDIO.mono().sGain);
        }
    }
}