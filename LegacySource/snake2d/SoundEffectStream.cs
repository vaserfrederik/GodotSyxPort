using System;
using System.IO;

namespace snake2d
{
    public interface SoundEffectStream : SoundStream
    {
        void SetCoos(int x, int y);
    }

    public class Dummy : SoundEffectStream
    {
        private long _millis = 0;
        private bool _looping = false;

        public bool Play()
        {
            _millis = (long)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + (long)(GetLengthInSeconds() * 1000);
            return true;
        }

        public void SetGain(double gain)
        {
        }

        public void Stop()
        {
            _millis = 0;
        }

        public void Resume()
        {
            Play();
        }

        public void SetLooping(bool yes)
        {
            _looping = yes;
        }

        public double GetProgress()
        {
            double p = 1.0 - (_millis - (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond)) / (1000.0 + GetLengthInSeconds());
            if (_looping && _millis != 0)
                p %= 1.0;
            return CLAMP.D(p, 0, 1);
        }

        public bool IsPlaying()
        {
            if (_looping && _millis != 0)
                return true;
            return GetProgress() < 1;
        }

        public double GetLengthInSeconds()
        {
            return 5;
        }

        public void PlayOnce()
        {
            Play();
        }

        public void SetCoos(int x, int y)
        {
            // TODO Auto-generated method stub
        }
    }

    public class SoundEffectStreamImp : SoundStream.SoundStreamImp, SoundEffectStream
    {
        private int _x = 0;
        private int _y = 0;
        private bool _posChanged = false;

        public SoundEffectStreamImp(Path path, bool music) : base(path, music)
        {
        }

        public void SetCoos(int x, int y)
        {
            _x = x;
            _y = y;
            _posChanged = true;
            if (IsPlaying())
            {
            }
        }

        protected override void Set(Source source)
        {
            source.SetPosition(_x, _y);
        }

        // public void SetPitch(float pitch)
        // {
        //     this.pitch = pitch;
        //     if (IsPlaying())
        //     {
        //         source.SetPitch(pitch);
        //     }
        // }

        protected override bool RefillBuffers(Source source)
        {
            if (base.RefillBuffers(source))
            {
                if (_posChanged)
                {
                    _posChanged = false;
                    source.SetPosition(_x, _y);
                }
                return true;
            }
            return false;
        }
    }
}