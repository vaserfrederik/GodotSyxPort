using System;
using System.IO;
using game;
using init.paths;
using snake2d;
using view.main;

namespace game.save
{
    public class GameLoader : CORE_STATE.Constructor
    {
        public readonly Path saveFile;
        public readonly string[] newScripts;

        public GameLoader(Path path, params string[] newScripts)
        {
            saveFile = path;
            this.newScripts = newScripts;
        }

        public override CORE_STATE getState()
        {
            FileGetter fg = null;
            GameSpec ss = null;
            string error = "No detectable error";

            try
            {
                fg = new FileGetter(saveFile, true);
                ss = GameSpec.Get(fg, newScripts);
                string s = ss.CrashCause();
                if (s != null)
                    error = s;
            }
            catch (IOException e)
            {
                e.printStackTrace();
                throw new Errors.DataError("Save is corrupted and can not be loaded!" + Environment.NewLine + " " + e, saveFile);
            }

            LOG.Ln("LOADING GAME", "Game version: " + VERSION.VERSION_STRING + " save: " + VERSION.versionString(VERSION.VERSION));
            string m = "";
            foreach (string mm in ss.Mods)
                m += mm + " | ";
            LOG.Ln("mod: " + m);

            try
            {
                VIEW v = GAME.Create(ss);
                VIEW.inters().load.Activate();
                CORE.Input.ClearAllInput();
                GAME.Saver().Load(fg);
                return v;
            }
            catch (Errors.GameError ee)
            {
                throw ee;
            }
            catch (Exception e)
            {
                e.printStackTrace(Console.Out);
                throw new Errors.DataError("Save is corrupted and can not be loaded!" + Environment.NewLine + " " + error + Environment.NewLine + " " + e, saveFile);
            }
        }

        public void Set()
        {
            CORE.SetCurrentState(this);
        }

        public static bool Quickload()
        {
            string ff = null;
            long time = -1;
            foreach (string s in PATHS.Local().save().GetFiles())
            {
                if (ff == null || SaveFile.Time(s) > time)
                {
                    time = SaveFile.Time(s);
                    ff = s;
                }
            }

            if (ff != null)
            {
                CORE.SetCurrentState(new GameLoader(PATHS.Local().save().Get(ff)));
                return true;
            }
            return false;
        }
    }
}