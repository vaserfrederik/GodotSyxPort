using System;
using game.VERSION;
using game.faction.player;
using init.paths;
using launcher;
using menu;
using snake2d;
using util.error;
using util.text;

namespace init
{
    class MainProcess
    {
        public static void Main(string[] args)
        {
            PreLoader.Load(VERSION.VERSION_STRING, PATHS_BASE.PRELOADER, PATHS_BASE.ICON_FOLDER + "Icon64.png");
            CORE.Init(new ErrorHandler());

            LOG.Ln("*******************************");
            LOG.Ln("* GAME " + VERSION.VERSION_STRING);
            LOG.Ln("*******************************");

            LSettings s = new LSettings();

            string l = s.Lang.Get();
            PATHS.Init(s.Mods.Get(), l != null && l.Length > 0 ? l : null, s.Easy.Get() == 1);

            D.Init();

            // PreLoader.Exit();
            Menu.Start();
            PTitles.Achieve();
            PreLoader.Exit();
        }
    }
}