using System;
using System.IO;
using game.debug;
using settlement.main;
using snake2d.util.file;

namespace settlement.battle
{
    public sealed class SBattle : SettResource
    {
        public readonly BannerRenderer bannerR = new BannerRenderer();
        public readonly ArmyTrainingInfo info;

        public SBattle(SETT sett) : base("battle", false)
        {
            info = new ArmyTrainingInfo();
        }

        protected override void Save(FilePutter file)
        {
            info.saver.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            info.saver.Load(file);
        }

        protected override void Clear()
        {
            info.saver.Clear();
        }

        private double ti = 0;
        protected override void Update(double ds, Profiler profiler)
        {
            ti += ds;
            if (ti > 0.1)
            {
                ti -= 0.1;
                info.Update();
            }
        }
    }
}