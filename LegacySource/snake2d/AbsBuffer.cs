using System;

namespace snake2d
{
    using snake2d.SOUND_CORE;

    abstract class AbsBuffer
    {
        public AbsBuffer()
        {
        }

        public abstract void reclaimSource(Source source);
        public abstract bool refillBuffers(Source source);
        public abstract void dis();
        public abstract void setBuffer(Source source);
        public abstract AUDIO_GAIN_TYPE type();
    }
}