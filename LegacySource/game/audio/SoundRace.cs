using System;
using game.faction;
using init.race;
using settlement.entity.humanoid;
using snake2d;
using util.keymap;

namespace game.audio
{
    public sealed class SoundRace : MAPPED, SoundSimple
    {
        private Sound[] all;
        private readonly int index;
        private readonly string key;

        public SoundRace(int index, string key, Sound sound)
        {
            all = new Sound[RACES.All().Count];
            Set(sound);

            this.key = key;
            this.index = index;
        }

        public void Set(Sound monos)
        {
            for (int ri = 0; ri < all.Length; ri++)
            {
                Set(ri, monos);
            }
        }

        public void Set(int ri, Sound monos)
        {
            all[ri] = monos;
        }

        public int Index()
        {
            return index;
        }

        public string Key()
        {
            return key;
        }

        public void Rnd(Humanoid a)
        {
            Rnd(a.Race()).Rnd(a.Body());
        }

        private Sound Rnd(Race race)
        {
            return all[race.Index];
        }

        public void Rnd(Race race, RECTANGLE body)
        {
            all[race.Index].Rnd(body);
        }

        public void Play(Race race, int cx, int cy)
        {
            SoundFile f = all[FACTIONS.Player().Race().Index].All.Rnd();
            f.Sound.Play(cx, cy, 1.0f, (float)f.Gain, false);
        }

        public bool Play(bool priority)
        {
            SoundFile f = all[FACTIONS.Player().Race().Index].All.Rnd();
            return f.Sound.Play(1.0f, (float)f.Gain, false);
        }

        public void Rnd(RECTANGLE body)
        {
            Rnd(body, 0.8f + RND.rFloat(0.2));
        }

        public void Rnd(RECTANGLE body, double gain)
        {
            Rnd(body.CX(), body.CY(), gain);
        }

        public void Rnd(int x, int y, double gain)
        {
            Rnd(FACTIONS.Player().Race()).Rnd(x, y, gain * AUDIO.Mono().SGain);
        }
    }
}