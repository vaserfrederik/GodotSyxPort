using System;
using System.IO;

namespace Snake2D
{
    sealed class SoundCoreDummy : SOUND_CORE
    {
        public SoundCoreDummy()
        {
        }

        public override void Set(int cX, int cY)
        {
        }

        public override void StopAllSounds()
        {
        }

        internal override void Dis()
        {
        }

        public override bool RequestMono(AbsBuffer buff, int x, int y, bool prio, float gain, float pitch)
        {
            return false;
        }

        public override bool RequestMono(AbsBuffer buff, bool prio, float gain, float pitch)
        {
            return false;
        }

        public override bool RequestStereo(AbsBuffer buff)
        {
            return false;
        }

        public override SoundEffect GetEffect(Path path)
        {
            return new SoundEffect.Dummy();
        }

        public override SoundStream GetStream(Path path, bool music)
        {
            return new SoundStream.Dummy();
        }

        public override SoundEffectStream GetStreamMono(Path path)
        {
            return new SoundEffectStream.Dummy();
        }

        public override void DisposeSounds()
        {
        }

        public override void SetGain(double gain, AUDIO_GAIN_TYPE type)
        {
            // TODO Auto-generated method stub
        }

        public override void SetMuteOnFocus(bool muteOnFocus)
        {
            // TODO Auto-generated method stub
        }
    }
}