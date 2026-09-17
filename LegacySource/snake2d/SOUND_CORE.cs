using System;
using System.Collections.Generic;
using System.IO;

namespace snake2d
{
    public abstract class SOUND_CORE
    {
        public enum AUDIO_GAIN_TYPE
        {
            MASTER,
            MUSIC,
            AMBIENCE,
            EFFECT,
        }

        public abstract void Set(int cX, int cY);

        public abstract void StopAllSounds();

        abstract void Dis();

        abstract bool RequestMono(AbsBuffer buff, int x, int y, bool prio, float gain, float pitch);

        abstract bool RequestMono(AbsBuffer buff, bool prio, float gain, float pitch);

        abstract bool RequestStereo(AbsBuffer buff);

        public abstract void SetGain(double gain, AUDIO_GAIN_TYPE type);
        public abstract void SetMuteOnFocus(bool muteOnFocus);

        static SOUND_CORE Create(SETTINGS s)
        {
            if (s.OpenALDevice() != null)
            {
                SoundDevices.Refresh();
                foreach (string ss in SoundDevices.Get())
                    if (ss.Equals(s.OpenALDevice(), StringComparison.OrdinalIgnoreCase))
                        return new SoundCore(ss, s);
                if (SoundDevices.Get().Count > 0)
                    return new SoundCore(SoundDevices.Get()[0], s);
            }
            return new SoundCoreDummy();
        }

        public abstract SoundEffect GetEffect(Path path);

        public abstract SoundStream GetStream(Path path, bool music);

        public abstract SoundEffectStream GetStreamMono(Path path);

        public abstract void DisposeSounds();
    }
}