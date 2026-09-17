using System;
using System.Collections.Generic;
using System.IO;

namespace game.audio
{
    public sealed class Music
    {
        private readonly MusicFactory factory;

        public AudioFactory<SoundStream> Factory()
        {
            return factory;
        }

        private SoundStream current;
        private SoundStream[] currentA;

        private readonly SoundStream[] normal;
        private readonly SoundStream[] battle;
        private int r = 0;

        private double fade = 0;
        private double timeout = 0;
        private bool shuffle = false;

        public Music()
        {
            factory = new MusicFactory();

            Json json = new Json(PATHS.AUDIO().config.Get("Music"));

            normal = Get(factory.Read("NORMAL", json));
            battle = Get(factory.Read("BATTLE", json));

            currentA = normal;
            current = currentA[0];
            current.SetGain(1f);
            fade = 1f;
            current.Play();
        }

        public void Update(double ds)
        {
            if (GAME.ARMIES().enemy().Men() > 0)
            {
                if (shuffle || currentA != battle)
                {
                    if (fade < 0)
                    {
                        current.Stop();
                        currentA = battle;
                        shuffle = false;
                        fade = 1;
                    }
                    else
                    {
                        current.SetGain(fade);
                    }
                    fade -= ds;
                }
                else if (!current.IsPlaying())
                {
                    fade = CLAMP.D(fade + ds, 0, 1);
                    r++;
                    r %= currentA.Length;
                    current = currentA[r];
                    current.SetGain(fade);
                    current.Play();
                }
            }
            else if (TIME.light().DayIs())
            {
                if (shuffle || currentA != normal)
                {
                    if (fade < 0)
                    {
                        current.Stop();
                        currentA = normal;
                        shuffle = false;
                        fade = 1;
                        timeout = 2 + RND.RInt(10);
                    }
                    else
                    {
                        current.SetGain(fade);
                    }
                    fade -= ds;
                }
                else if (!current.IsPlaying())
                {
                    if (timeout > 0)
                    {
                        timeout -= ds;
                    }
                    else
                    {
                        fade = CLAMP.D(fade + ds, 0, 1);
                        r++;
                        r %= currentA.Length;
                        current = currentA[r];
                        current.SetGain(fade);
                        current.Play();
                    }
                }
            }
            else
            {
                if (currentA == battle)
                {
                    if (fade < 0)
                    {
                        current.Stop();
                    }
                    fade -= ds;
                }
            }
        }

        private SoundStream[] Get(List<SoundStream> streams)
        {
            SoundStream[] res = new SoundStream[streams.Count];
            int i = 0;
            foreach (SoundStream s in streams)
            {
                res[i++] = s;
            }
            Random ran = new Random();
            ran.Seed = (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            for (int k = 0; k < res.Length * 4; k++)
            {
                int i1 = ran.Next(res.Length);
                int i2 = ran.Next(res.Length);
                SoundStream s = res[i1];
                res[i1] = res[i2];
                res[i2] = s;
            }

            return res;
        }

        public void Next()
        {
            shuffle = true;
        }
    }
}