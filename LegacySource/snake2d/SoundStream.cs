using System;
using System.Numerics;
using System.IO;
using OpenAL;

namespace Snake2D
{
    public interface SoundStream
    {
        bool Play();
        void PlayOnce();
        void SetGain(double gain);
        void Stop();
        void Resume();
        void SetLooping(bool yes);
        double GetProgress();
        bool IsPlaying();
        double GetLengthInSeconds();
    }

    public class Dummy : SoundStream
    {
        private long millis = 0;
        private bool looping = false;

        public bool Play()
        {
            millis = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + GetLengthInSeconds() * 1000);
            return true;
        }

        public void SetGain(double gain)
        {
        }

        public void Stop()
        {
            millis = 0;
        }

        public void Resume()
        {
            Play();
        }

        public void SetLooping(bool yes)
        {
            this.looping = yes;
        }

        public double GetProgress()
        {
            double p = 1.0 - (millis - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / (1000.0 + GetLengthInSeconds());
            if (looping && millis != 0)
                p %= 1.0;
            return Math.Clamp(p, 0, 1);
        }

        public bool IsPlaying()
        {
            if (looping && millis != 0)
                return true;
            return GetProgress() < 1;
        }

        public double GetLengthInSeconds()
        {
            return 60;
        }

        public void PlayOnce()
        {
            Play();
        }
    }

    public abstract class AbsBuffer
    {
        protected abstract void setBuffer(Source source);
        protected abstract void reclaimSource(Source source);
        protected abstract bool refillBuffers(Source source);
        protected abstract AUDIO_GAIN_TYPE type();
        public abstract void dis();
    }

    public class SoundStreamImp : AbsBuffer, SoundStream
    {
        protected readonly int[] bufferIDs;
        private readonly DATA_STREAM data;
        private bool looping;
        private volatile bool playing = false;
        private bool wantsToStop = false;
        private bool gainChanged = false;
        protected float gain = 1f;
        protected float bufferOffset = 0;
        private readonly AUDIO_GAIN_TYPE type;

        public SoundStreamImp(string path, bool music)
        {
            type = music ? AUDIO_GAIN_TYPE.MUSIC : AUDIO_GAIN_TYPE.AMBIENCE;
            this.data = DATA_STREAM.GetStream(path);
            const int nrOfBuffers = 3;
            bufferIDs = new int[nrOfBuffers];
            for (int i = 0; i < bufferIDs.Length; i++)
            {
                bufferIDs[i] = AL.GenBuffer();
            }
        }

        public bool Play()
        {
            wantsToStop = false;
            if (playing)
                return true;
            return Play(0);
        }

        private bool Play(float off)
        {
            bufferOffset = off;
            playing = CORE.GetSoundCore().RequestStereo(this);
            return playing;
        }

        public void PlayOnce()
        {
            if (playing)
            {
                wantsToStop = true;
                return;
            }
            bufferOffset = 0;
            playing = CORE.GetSoundCore().RequestMono(this, true, gain, 1);
        }

        protected override void setBuffer(Source source)
        {
            Reset();
            for (int i = 0; i < bufferIDs.Length; i++)
            {
                source.EnqueueBuffer(bufferIDs[i]);
            }
            source.SetPitch(1f);
            source.SetGain(gain);
            Set(source);
            source.Play();
            playing = true;
            gainChanged = false;
            wantsToStop = false;
        }

        protected virtual void Set(Source source)
        {
        }

        public void SetGain(double gain)
        {
            if (gain < 0 || gain > 1)
                throw new Exception(gain.ToString());
            if (gain != this.gain)
            {
                this.gain = (float)gain;
                gainChanged = true;
            }
        }

        public void Stop()
        {
            wantsToStop = true;
        }

        public void Resume()
        {
            if (playing)
                return;
            Play(bufferOffset);
        }

        public void SetLooping(bool yes)
        {
            looping = yes;
        }

        public double GetLengthInSeconds()
        {
            return data.GetLengthInSeconds();
        }

        public double GetProgress()
        {
            if (playing)
            {
                return data.GetProgress();
            }
            return 0;
        }

        public bool IsPlaying()
        {
            return playing;
        }

        private void Reset()
        {
            data.Rewind();
            for (int i = 0; i < bufferIDs.Length; i++)
            {
                data.SetNext(bufferIDs[i]);
            }
        }

        protected override void reclaimSource(Source source)
        {
            bufferOffset = source.GetOffset();
            playing = false;
            source = null;
        }

        public override void dis()
        {
            data.Dispose();
            foreach (var id in bufferIDs)
            {
                AL.DeleteBuffer(id);
            }
        }

        protected override bool refillBuffers(Source source)
        {
            if (wantsToStop)
            {
                return false;
            }

            while (source.HasProcessedBuffer())
            {
                if (data.HasMoreBuffers())
                {
                    for (int i = 0; i < bufferIDs.Length - 1; i++)
                        bufferIDs[i] = bufferIDs[i + 1];
                    int buff = source.GetProcessedBuffers();
                    data.SetNext(buff);
                    source.EnqueueBuffer(buff);
                    bufferIDs[bufferIDs.Length - 1] = buff;
                    if (!source.IsPlaying())
                        source.Play();
                }
                else if (looping)
                {
                    data.Rewind();
                    refillBuffers(source);
                }
                else
                {
                    source.GetProcessedBuffers();
                    return false;
                }
            }

            if (gainChanged)
            {
                source.SetGain(gain);
                gainChanged = false;
            }

            return true;
        }

        protected override AUDIO_GAIN_TYPE type()
        {
            return type;
        }
    }
}