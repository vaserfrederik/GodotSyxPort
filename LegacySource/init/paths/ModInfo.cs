using System;
using System.IO;
using System.Numerics;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace init.paths
{
    public sealed class ModInfo
    {
        public readonly string absolutePath;
        public readonly string path;
        public readonly string name;
        public readonly string desc;
        public readonly string version;
        public readonly int majorVersion;
        public readonly string author;
        public readonly string info;
        int TEXTURE_CACHE_SIZE = 4096;

        public ModInfo(string dir) : this(dir, true) { }

        public ModInfo(string dir, bool loadJson) : this(dir, loadJson, 0, 16384, 4096) { }

        public ModInfo(string dir, bool loadJson, int minTextureCacheSize, int maxTextureCacheSize, int defaultTextureCacheSize)
        {
            path = dir;

            try
            {
                var g = PATHS.local().MODS.getFolder(dir, ".txt");
                absolutePath = g.get().FullName;
                if (loadJson)
                {
                    var j = new Json(g.gets("_Info"));
                    name = j.text("NAME", "???");
                    desc = j.text("DESC", "???");
                    version = j.text("VERSION", "???");
                    majorVersion = bestVersion(dir);
                    author = j.text("AUTHOR", "???");
                    info = j.text("INFO", "???");
                    TEXTURE_CACHE_SIZE = j.i("TEXTURE_CACHE_SIZE", minTextureCacheSize, maxTextureCacheSize, defaultTextureCacheSize);
                }
            }
            catch (Exception e)
            {
                LOG.ln("unable to load mod: " + dir + " reason: " + e.Message);
                //e.printStackTrace();
                throw new ModInfoException(e);
            }

            if (TEXTURE_CACHE_SIZE < minTextureCacheSize || TEXTURE_CACHE_SIZE > maxTextureCacheSize || (TEXTURE_CACHE_SIZE & (TEXTURE_CACHE_SIZE - 1)) != 0)
            {
                throw new ModInfoException("TEXTURE_CACHE_SIZE - Invalid value: " + TEXTURE_CACHE_SIZE + ".  Accepted are 4096, 8192, 16384" + " " + PATHS.local().MODS.get(dir));
            }
        }

        public static int bestVersion(string mod) throws ModInfoException
        {
            int best = -1;

            foreach (var ss in PATHS.local().MODS.getFolder(mod).folders())
            {
                if (ss.Length >= 2 && ss[0] == 'V')
                {
                    var nr = ss.Substring(1);
                    try
                    {
                        int i = int.Parse(nr);
                        if (i == VERSION.VERSION_MAJOR)
                            return i;
                        if (Math.Ceiling(VERSION.VERSION_MAJOR - i) > best)
                            best = i;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            if (best != -1)
                return best;
            throw new ModInfoException("There are no version folders in mod. " + PATHS.local().MODS.getFolder(mod).get());
        }

        DirectoryInfo getModFolder()
        {
            return PATHS.local().MODS.getFolder(path).getFolder("V" + majorVersion).get();
        }

        public static class ModInfoException : Exception
        {
            public ModInfoException(Exception e) : base("unable to load mod", e) { }

            public ModInfoException(string m) : base(m) { }
        }
    }
}