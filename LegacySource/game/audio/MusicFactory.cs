using System.Collections.Generic;
using System.IO;
using Snake2D;
using Init.Paths;

namespace Game.Audio
{
    public sealed class MusicFactory : AudioFactory<SoundStream>
    {
        public MusicFactory() : base("MUSIC", PATHS.AUDIO().music, new SoundStream.Dummy())
        {
        }

        protected override SoundStream Create(LinkedList<SoundStream> all, Path p, string key)
        {
            return CORE.GetSoundCore().GetStream(p, true);
        }
    }
}