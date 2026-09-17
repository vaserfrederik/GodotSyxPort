using System;

namespace Game.Audio
{
    internal class AmbianceChannel
    {
        private Ambiance current;
        private SoundStream stream;
        private double gain;
        private bool play;

        public void Update(double ds)
        {
            if (current == null)
                return;
            if (!stream.IsPlaying())
            {
                current = null;
                return;
            }
            if (!play || current.Priority <= 0)
            {
                gain -= ds * 2;
                if (gain <= 0)
                {
                    gain = 0;
                    stream.Stop();
                    play = false;
                }
            }

            if (gain < current.Gain())
            {
                gain += ds * 2;
                if (gain > current.Gain())
                    gain = current.Gain();
            }
            else if (gain > current.Gain())
            {
                gain -= ds * 2;
                if (gain < current.Gain())
                    gain = current.Gain();
            }
            gain = CLAMP.d(gain, 0, 1);
            stream.SetGain(gain);
        }

        public void Init(Ambiance c)
        {
            current = c;
            stream = c.Streams.Rnd();
            stream.SetLooping(false);
            gain = 0;
            stream.SetGain(gain);
            stream.Play();
            play = true;
        }
    }
}