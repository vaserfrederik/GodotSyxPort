using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class GameSpec
    {
        public bool Fubar { get; private set; }
        public readonly int Version;
        public readonly double PlaySeconds;
        public readonly int Population;
        public readonly int Enemies;
        public readonly int Regions;
        public readonly int RegPop;
        public readonly string Race;
        public readonly string City;
        public readonly string Ruler;
        public readonly string Desc;
        public readonly string[] Scripts;
        public readonly string[] Mods;
        public readonly int Wx;
        public readonly int Wy;
        public readonly string Races;
        public readonly string Rooms;
        public readonly string Resources;
        public readonly string Industries;

        private GameSpec(string[] scripts)
        {
            Fubar = true;
            this.Scripts = scripts;
            Version = VERSION.VERSION;
            PlaySeconds = 0;
            Population = 0;
            Regions = 0;
            RegPop = 0;
            Enemies = 0;
            Race = "?";
            City = "?";
            Ruler = "?";
            Desc = "?";
            Mods = Mods();
            Wx = Config.World().WORLD_SIZE;
            Wy = Config.World().WORLD_SIZE;
            Races = GetResources("race");
            Rooms = GetResources("room");
            Resources = GetResources("resource");
            Industries = GetResourcesArray("room", "INDUSTRY");
        }

        private GameSpec(FileGetter f, string[] scripts) 
        {
            int pos = f.Position + f.i() + 4;
            Version = f.i();
            PlaySeconds = f.d();
            Population = f.i();
            Enemies = f.i();
            Regions = f.i();
            RegPop = f.i();
            Wx = f.i();
            Wy = f.i();

            Race = f.chars();
            City = f.chars();
            Ruler = f.chars();
            Desc = f.chars();

            Mods = f.charss();
            string[] ss = f.charss();
            this.Scripts = scripts.Length > 0 ? scripts : ss;

            Races = f.chars();
            Rooms = f.chars();
            Resources = f.chars();
            Industries = f.chars();
            if (f.Position != pos)
            {
                Fubar = true;
                f.Position = pos;
            }
        }

        public static void Save(FilePutter f)
        {
            int pos = f.Position;
            f.i(0);
            f.i(VERSION.VERSION);
            f.d((int)TIME.PlayedGame());
            f.i(POP.Tot(null, null));
            f.i(STATS.POP().Pop(HTYPES.ENEMY()));
            f.i(FACTIONS.Player().Realm().Regions() - 1);
            f.i(RD.RACES().Population.Faction().Get(FACTIONS.Player()));
            f.i(WORLD.TWIDTH());
            f.i(WORLD.THEIGHT());

            f.chars("" + FACTIONS.Player().Race().Info.Name);
            f.chars("" + FACTIONS.Player().Name);
            f.chars("" + FACTIONS.Player().RulerName);
            f.chars("" + FACTIONS.Player().Desc);

            f.charss(Mods());
            f.charss(GAME.Script().CurrentScripts());

            f.chars(GetResources("race"));
            f.chars(GetResources("room"));
            f.chars(GetResources("resource"));
            f.chars(GetResourcesArray("room", "INDUSTRY"));

            int le = f.Position - pos - 4;
            f.SetAtPosition(pos, le);
        }

        public static GameSpec Get(string[] scripts)
        {
            return new GameSpec(scripts);
        }

        public static GameSpec Get(FileGetter f, string[] scripts)
        {
            try
            {
                GameSpec s = new GameSpec(f, scripts);
                return s;
            }
            catch (Exception e)
            {
                return new GameSpec(scripts);
            }
        }

        public static GameSpec Get(Path path)
        {
            try
            {
                FileGetter g = new FileGetter(path, true);
                GameSpec s = new GameSpec(g);
                return s;
            }
            catch (Exception e)
            {
                return new GameSpec();
            }
        }

        private static string[] Mods()
        {
            string[] mods = new string[PATHS.CurrentMods().Count];
            for (int i = 0; i < PATHS.CurrentMods().Count; i++)
            {
                mods[i] = "'" + PATHS.CurrentMods()[i].Name + "', version: " + PATHS.CurrentMods()[i].Version;
            }
            return mods;
        }

        private CharSequence prob;
        private CharSequence warn;
        private bool hasCheck = false;

        private static CharSequence ¤¤version = "¤Version mismatch! Save is made with major game version: {0}. Try downgrading the game to this version. The current game version is: ";
        private static CharSequence ¤¤race = "¤The amount of races does not match the current configuration.";
        private static CharSequence ¤¤room = "¤The amount of rooms does not match the current configuration.";
        private static CharSequence ¤¤industries = "¤The amount of industries does not match the current configuration.";
        private static CharSequence ¤¤resources = "¤The amount of resources does not match the current configuration.";
        private static CharSequence ¤¤modOther = "¤The save can not be loaded as it was made with another mod configuration:";
        private static CharSequence ¤¤modNone = "¤The save can not be loaded as it was made with an un-modified game. Disable all mods in the launcher to load the game.";
        private static CharSequence ¤¤script = "¤The script: {0} that the game was saved with can not be found.";
        private static CharSequence ¤¤fubar = "The save-file is corrupt.";
        private static CharSequence ¤¤mod2 = "¤Try to enable the same mods, in the same order, in the launcher and reload the save.";
        private static CharSequence ¤¤underlaying = "¤UnderLaying problem:";

        private static CharSequence ¤¤mods = "Game was saved with other mods than those currently enabled.";

        static
        {
            D.ts(typeof(GameSpec));
        }

        public CharSequence Warning()
        {
            if (!hasCheck)
            {
                hasCheck = true;
                warn = Pwarning();
                prob = Pproblem();
            }
            return warn;
        }

        public CharSequence CrashCause()
        {
            if (!hasCheck)
            {
                hasCheck = true;
                warn = Pwarning();
                prob = Pproblem();
            }
            return prob;
        }

        public CharSequence Pwarning()
        {
            if (Fubar)
            {
                return ¤¤fubar;
            }
            if (VERSION.VersionMajor(Version) != VERSION.VERSION_MAJOR)
                return "" + (Str.TMP.Clear().Add(¤¤version).Insert(0, VERSION.VersionMajor(Version)) + " " + VERSION.VERSION_MAJOR);

            if (!ModsEqual())
                return "" + (Str.TMP.Clear().Add(¤¤mods));
            KeyMap<string> avai = new KeyMap<string>();
            foreach (ScriptLoad sc in ScriptEngine.GetAll())
                avai.Put(sc.Key, sc.Key);
            foreach (string sc in Scripts)
            {
                if (!avai.ContainsKey(sc))
                {
                    return "" + (Str.TMP.Clear().Add(¤¤script).Insert(0, sc));
                }
            }
            return null;
        }

        public CharSequence Pproblem()
        {
            if (Fubar)
            {
                return ¤¤fubar;
            }
            if (VERSION.VersionMajor(Version) != VERSION.VERSION_MAJOR)
                return "" + (Str.TMP.Clear().Add(¤¤version).Insert(0, VERSION.VersionMajor(Version)) + " " + VERSION.VERSION_MAJOR);

            if (!ModsEqual())
                return "" + (Str.TMP.Clear().Add(¤¤mods));
            KeyMap<string> avai = new KeyMap<string>();
            foreach (ScriptLoad sc in ScriptEngine.GetAll())
                avai.Put(sc.Key, sc.Key);
            foreach (string sc in Scripts)
            {
                if (!avai.ContainsKey(sc))
                {
                    return "" + (Str.TMP.Clear().Add(¤¤script).Insert(0, sc));
                }
            }
            if (!ModsEqual())
            {
                Str s = Str.TMP;
                if (Mods.Length == 0)
                {
                    s.Add(¤¤modNone);
                }
                else
                {
                    s.Add(¤¤modOther);
                    s.NL();
                    s.NL();
                    foreach (string ss in Mods)
                    {
                        s.Add(ss);
                        s.NL();
                    }
                    s.NL();
                    s.Add(¤¤mod2);
                }
                return "" + s;
            }
            return null;
        }

        private CharSequence ModException(CharSequence problem)
        {
            if (!ModsEqual())
            {
                Str s = Str.TMP;
                s.Clear();

                if (Mods.Length == 0)
                {
                    s.Add(¤¤modNone);
                }
                else
                {
                    s.Add(¤¤modOther);
                    s.NL();
                    s.NL();
                    foreach (string ss in Mods)
                    {
                        s.Add(ss);
                        s.NL();
                    }
                    s.NL();
                    s.Add(¤¤mod2);
                }
                s.NL();
                s.NL();
                s.Add(¤¤underlaying);
                s.NL();
                s.Add(problem);
                return "" + s;
            }
            else
            {
                return "" + problem;
            }
        }

        private static string GetResourcesArray(string init, string key)
        {
            string s = "";
            PATH p = PATHS.INIT().GetFolder(init);
            foreach (string k in p.GetFiles())
            {
                Json j = new Json(p.GetS(k));
                if (j.Has(key) && j.JsonsIs(key))
                    s += k + j.Jsons(key).Length;
            }
            return s;
        }

        private static string GetResources(string init)
        {
            string s = "";
            foreach (string k in PATHS.INIT().GetFolder(init).GetFiles())
                s += k;
            return s;
        }
    }
}