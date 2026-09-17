using System;
using game.save;
using init.settings;
using snake2d;
using view.main;

namespace GameSave
{
    internal class AutoSaver
    {
        private long last = -1;
        private long count = 0;
        private readonly GameSaver saver;

        public AutoSaver(GameSaver saver)
        {
            this.saver = saver;
        }

        public void AutoSave(double ds)
        {
            if (S.Get().AutoSaveInterval.Get() > 0 && VIEW.CanSave())
            {
                if (ds != 0)
                {
                    if (last != -1)
                    {
                        count += CORE.GetUpdateInfo().GetNowMillis() - last;
                    }
                    last = CORE.GetUpdateInfo().GetNowMillis();

                    long time = 1 + 2 * (S.Get().AutoSaveInterval.Max() - S.Get().AutoSaveInterval.Get());
                    time *= 1000 * 60;

                    if (count >= time && VIEW.Current().UiManager.IsGoodTimeToSave())
                    {
                        Save();
                        Reset();
                    }
                }
            }
            else
            {
                count = 0;
            }
        }

        private void Save()
        {
            saver.SaveNamed("AutoSave", S.Get().AutoSaveFiles.Get(), true);
        }

        public void Reset()
        {
            count = 0;
        }
    }
}