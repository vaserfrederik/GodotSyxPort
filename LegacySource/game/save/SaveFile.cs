using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game;
using init.paths;
using settlement.stats;
using snake2d.util.sprite.text;
using util.text;

namespace game.save
{
    public class SaveFile : IComparable<SaveFile>
    {
        public readonly Path path;
        public readonly string name;
        public readonly CharSequence ago;
        public readonly CharSequence fullName;
        public readonly int version;
        public readonly int modHash;
        public readonly int pop;
        public readonly long t;

        private static CharSequence ¤¤Version = "¤This save is from a previous version and will probably not load!";
        private static CharSequence ¤¤Mod = "¤This save is made with a different mod configuration and will probably not load!";

        static SaveFile()
        {
            D.ts(typeof(SaveFile));
        }

        public SaveFile(Path path)
        {
            this.path = path;
            string f = path.GetFileName().ToString();
            if (f.LastIndexOf(".") > 0)
                f = f.Substring(0, f.LastIndexOf("."));
            fullName = f;
            name = name(f);
            t = time(f);
            version = version(f);
            modHash = modHash(f);
            pop = pop(f);
            if (t > 0 && System.currentTimeMillis() - t > 0)
            {
                double tt = (System.currentTimeMillis() - t) / 1000;
                double now = tt / (60 * 60 * 24 * 365);

                if (now < 1)
                {
                    now = tt / (60 * 60 * 24);
                    if (now < 1)
                    {
                        now = tt / (60 * 60);
                        if (now < 1)
                        {
                            now = tt / 60;
                            if (now == 0)
                            {
                                ago = DicTime.setSeconds(new Str(8), tt);
                            }
                            else
                            {
                                ago = DicTime.setMinutes(new Str(8), now);
                            }
                        }
                        else
                        {
                            ago = DicTime.setHours(new Str(8), now);
                        }
                    }
                    else
                    {
                        ago = DicTime.setDays(new Str(8), now);
                    }
                }
                else
                {
                    ago = DicTime.setYears(new Str(8), now);
                }
            }
            else
            {
                ago = "???";
            }
        }

        public static SaveFile[] list()
        {
            return list(PATHS.local().save());
        }

        public static SaveFile[] list(PATH path)
        {
            string[] ss = path.getFiles();

            SaveFile[] saves = new SaveFile[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                saves[i] = new SaveFile(path.get(ss[i]));
            }
            Array.Sort(saves);
            return saves;
        }

        public static string name(string file)
        {
            return get(file, 4, false);
        }

        public static long time(string file)
        {
            string s = get(file, 3, true);
            try
            {
                return long.Parse(s, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static int version(string file)
        {
            string s = get(file, 2, true);

            try
            {
                return (int)long.Parse(s, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static int modHash(string file)
        {
            string s = get(file, 1, true);
            try
            {
                return (int)long.Parse(s, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static int pop(string file)
        {
            string s = get(file, 0, true);
            try
            {
                return (int)long.Parse(s, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static string get(string file, int part, bool p)
        {
            while (part > 0)
            {
                int i = file.LastIndexOf('-');
                if (i <= 0)
                    return "0";
                file = file.Substring(0, i);
                part--;
            }
            int i = file.LastIndexOf('-');
            if (i <= 0 || !p)
                return file;
            return file.Substring(i + 1, file.Length - i - 1);
        }

        public int CompareTo(SaveFile arg0)
        {
            if (t < 0 && arg0.t >= 0)
                return 1;
            if (t >= 0 && arg0.t < 0)
                return -1;

            long ti = arg0.t - t;
            if (ti < 0)
                return -1;
            if (ti > 0)
                return 1;
            return name.CompareTo(arg0.name);
        }

        public static string stamp(CharSequence savefile)
        {
            string t = long.toHexString(System.currentTimeMillis());
            string v = int.toHexString(VERSION.VERSION);
            string mods = int.toHexString(PATHS.modHash());
            string pop = int.toHexString(POP.tot(null, null));

            string s = savefile + "-" + t + "-" + v + "-" + mods + "-" + pop;
            return s;
        }

        public CharSequence problem()
        {
            if (VERSION.VERSION_MAJOR != VERSION.versionMajor(version))
            {
                return ¤¤Version;
            }
            if (modHash != PATHS.modHash())
            {
                return ¤¤Mod;
            }
            return null;
        }

        private GameSpec spec;

        public GameSpec spec()
        {
            if (spec == null)
            {
                spec = GameSpec.get(path);
            }
            return spec;
        }

        public bool specReady()
        {
            return spec != null;
        }
    }
}