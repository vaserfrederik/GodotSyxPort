using System;
using System.IO;
using OpenAL.Bindings;

namespace snake2d
{
    public interface SoundEffect : SoundSimple
    {
        bool Play(int x, int y, float pitch, float gain, bool priority);
        bool Play(RECTANGLE rec, float pitch, float gain, bool priority);
        bool Play(int x, int y, bool priority);
        bool Play(float pitch, float gain, bool priority);
        float LengthInSeconds();
    }

    public class SoundEffectImp : AbsBuffer, SoundEffect
    {
        private readonly int ID;
        private readonly Data data;

        public SoundEffectImp(Path path)
        {
            data = new Data(path);
            ID = AL.GenBuffer();
            AL.BufferData(ID, data.alFormat, data.data, data.samplerate);
            data.Dispose();
        }

        protected override void Dis()
        {
            AL.DeleteBuffer(ID);
        }

        public bool Play(int x, int y, float pitch, float gain, bool priority)
        {
            return CORE.GetSoundCore().RequestMono(this, x, y, priority, gain, pitch);
        }

        public bool Play(RECTANGLE rec, float pitch, float gain, bool priority)
        {
            return Play(rec.CX(), rec.CY(), pitch, gain, priority);
        }

        public bool Play(int x, int y, bool priority)
        {
            return Play(x, y, 1f, 1f, priority);
        }

        public bool Play(bool priority)
        {
            return Play(1f, 1f, priority);
        }

        public bool Play(float pitch, float gain, bool priority)
        {
            return CORE.GetSoundCore().RequestMono(this, priority, gain, pitch);
        }

        public float LengthInSeconds()
        {
            return data.length;
        }

        protected override void ReclaimSource(Source source)
        {
            // TODO Auto-generated method stub
        }

        protected override bool RefillBuffers(Source source)
        {
            return false;
        }

        protected override void SetBuffer(Source source)
        {
            source.SetBuffer(ID);
        }

        protected override AUDIO_GAIN_TYPE Type()
        {
            return AUDIO_GAIN_TYPE.EFFECT;
        }
    }

    public class Dummy : SoundEffect
    {
        public bool Play(int x, int y, float pitch, float gain, bool priority)
        {
            return true;
        }

        public bool Play(RECTANGLE rec, float pitch, float gain, bool priority)
        {
            return true;
        }

        public bool Play(int x, int y, bool priority)
        {
            return true;
        }

        public bool Play(bool priority)
        {
            return true;
        }

        public bool Play(float pitch, float gain, bool priority)
        {
            return true;
        }

        public float LengthInSeconds()
        {
            return 5;
        }
    }
}