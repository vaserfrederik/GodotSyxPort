using game;
using snake2d;

namespace game.audio
{
    public class AUDIO
    {
        private static AUDIO s;

        private SoundRaces races;
        private readonly Music music;
        private readonly SoundFactory mono;
        private readonly Ambiances ambiences;
        private readonly AmbianceUpdater aUpdater;

        public AUDIO(GAME game)
        {
            CORE.getSoundCore().disposeSounds();
            s = this;
            mono = new SoundFactory();
            music = new Music();
            ambiences = new Ambiances();
            aUpdater = new AmbianceUpdater(ambiences);
        }

        public void Update(double ds)
        {
            music.Update(ds);
            aUpdater.Update();
        }

        public static void SetSettGain(double gain)
        {
            s.mono.SettGain(gain);
        }

        public static SoundRace Race(string key)
        {
            return s.races.Get(key);
        }

        static SoundFactory Mono()
        {
            return s.mono;
        }

        public static Music Music()
        {
            return s.music;
        }

        public static Ambiances AMBI()
        {
            return s.ambiences;
        }

        public static AmbianceUpdater AMBI_UP()
        {
            return s.aUpdater;
        }

        public void Init()
        {
            new Debug();
            races = new SoundRaces(mono);
        }
    }
}