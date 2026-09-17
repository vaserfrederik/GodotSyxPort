using System;
using System.IO;
using System.Linq;

namespace Game.Battle.Thread.Trajectory
{
    public class Request : ISavable
    {
        private short ammo = -1;
        private readonly float[] refs;
        private readonly int[] pixels;
        private readonly byte[] counts;

        public Request()
        {
            refs = Enumerable.Repeat(float.NaN, Config.Battle().MenPerDivision).ToArray();
            pixels = Enumerable.Repeat(-1, Config.Battle().MenPerDivision * 2).ToArray();
            counts = Enumerable.Repeat((byte)121, Config.Battle().MenPerDivision).ToArray();
        }

        public bool Request(int pos, Humanoid h, Div div)
        {
            EquipRange a = div.Settings().Ammo();
            if (a == null)
            {
                ammo = -1;
                return false;
            }

            counts[pos] = 0;

            if (a.TIndex != ammo)
            {
                ammo = a.TIndex;
                Set(pos, (float)a.Ref(h.Indu()), h);
                return false;
            }

            int pi = pos * 2;
            if (pixels[pi] != h.Body().CX() || pixels[pi + 1] != h.Body().CY())
            {
                Set(pos, (float)a.Ref(h.Indu()), h);
                return false;
            }

            float @ref = (float)a.Ref(h.Indu());

            if (@ref != refs[pos])
            {
                Set(pos, (float)a.Ref(h.Indu()), h);
                return false;
            }

            return true;
        }

        private void Set(int pos, float @ref, Humanoid h)
        {
            refs[pos] = (float)@ref;
            int pi = pos * 2;
            pixels[pi] = h.Body().CX();
            pixels[pi + 1] = h.Body().CY();
        }

        public void Save(FilePutter file)
        {
            file.S(ammo);
            file.Fs(refs);
            file.Is(pixels);
            file.Bs(counts);
        }

        public void Load(FileGetter file)
        {
            ammo = file.S();
            file.Fs(refs);
            file.Is(pixels);
            file.Bs(counts);
        }

        public void Clear()
        {
            ammo = -1;
            refs.AsSpan().Fill(0);
            pixels.AsSpan().Fill(0);
            counts.AsSpan().Fill(121);
        }

        public float Ref(int i)
        {
            return refs[i];
        }

        public int X(int i)
        {
            return pixels[i * 2];
        }

        public int Y(int i)
        {
            return pixels[i * 2 + 1];
        }

        public EquipRange Ammo()
        {
            if (ammo == -1)
                return null;
            return STATS.EQUIP().RANGED().Get(ammo);
        }

        public bool Count(int i)
        {
            if (counts[i] > 120)
                return false;
            counts[i]++;
            return true;
        }
    }
}