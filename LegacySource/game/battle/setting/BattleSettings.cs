using System;
using System.IO;
using game.battle.Armies;
using game.battle.div.Div;
using game.save;
using init.constant;
using snake2d.util.file;
using util.updating;

namespace game.battle.setting
{
    public class BattleSettings
    {
        private readonly DivSettings[] all;
        private readonly IUpdater updater;

        public BattleSettings(Armies armies)
        {
            all = new DivSettings[Config.battle().DIVISIONS_PER_BATTLE];
            updater = new IUpdater(Config.battle().DIVISIONS_PER_BATTLE, 1.0);

            updater.Update = (i, timeSinceLast) =>
            {
                all[i].Update();
            };

            foreach (Div d in armies.Divisions())
            {
                all[d.Index()] = new DivSettings(d);
            }

            GAME.Saver().AddSpecialSaver(new Savable("BATTLE_DIV_SETTINGS")
            {
                Save = (FilePutter file) =>
                {
                    foreach (DivSettings s in all)
                    {
                        s.Save(file);
                    }
                },

                Load = (FileGetter file) =>
                {
                    foreach (DivSettings s in all)
                    {
                        s.Load(file);
                    }
                },

                LoadFail = () =>
                {
                    foreach (DivSettings s in all)
                    {
                        s.Clear();
                    }
                }
            });
        }

        public static DivSettings Get(Div d)
        {
            return GAME.ARMIES().Settings.All[d.Index()];
        }

        public void Update(double ds)
        {
            updater.Update(ds);
        }

        public void Init(Div d)
        {
            d.Settings().Update();
        }
    }
}