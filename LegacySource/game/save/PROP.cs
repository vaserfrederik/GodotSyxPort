using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Game.Save
{
    public sealed class PROP : Savable
    {
        private static PROP s;
        private bool dirty;
        private readonly KeyMap<string> profileMap = new KeyMap<string>();
        private readonly KeyMap<string> gameMap = new KeyMap<string>();
        private const string fn = "Properties";

        public PROP(GameSaver s) : base("PROP")
        {
            PROP.s = this;
            Read();
        }

        public static PropGame Game(string prefix)
        {
            return new PropGame(prefix);
        }

        public static string Prop(string key)
        {
            if (s.profileMap.ContainsKey(key))
                return s.profileMap[key];
            return null;
        }

        public static void PropSet(string key, string value)
        {
            if (value == Prop(key))
                return;
            s.dirty = true;
            s.profileMap.PutReplace(key, value);
        }

        public static int PropI(string key, int fallback)
        {
            string kk = Prop(key);
            if (kk == null)
                return fallback;
            try
            {
                int r = int.Parse(kk);
                return r;
            }
            catch (FormatException)
            {
                return fallback;
            }
        }

        public static void PropISet(string key, int i)
        {
            PropSet(key, i.ToString());
        }

        private static void Read()
        {
            try
            {
                s.profileMap.Clear();
                var jsonPath = PATHS.Local().PROFILE.Get(fn);
                if (File.Exists(jsonPath))
                {
                    var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(jsonPath));
                    foreach (var kvp in json)
                    {
                        s.profileMap.Put(kvp.Key, kvp.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                s.profileMap.Clear();
                try
                {
                    var profilePath = PATHS.Local().PROFILE.Get(fn);
                    if (!File.Exists(profilePath))
                        File.Create(profilePath).Close();
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee);
                }
            }
        }

        private static void Flush()
        {
            if (!s.dirty)
                return;
            s.dirty = false;
            try
            {
                var sortedKeys = s.profileMap.KeysSorted();
                var json = new Dictionary<string, string>();
                foreach (var key in sortedKeys)
                {
                    json[key] = s.profileMap[key];
                }
                var jsonPath = PATHS.Local().PROFILE.Get(fn);
                if (!File.Exists(jsonPath))
                    File.Create(jsonPath).Close();
                File.WriteAllText(jsonPath, JsonConvert.SerializeObject(json, Formatting.Indented));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void Save(FilePutter file)
        {
            file.I(gameMap.Count);
            foreach (var key in gameMap.Keys)
            {
                file.Chars(key);
                file.Chars(gameMap[key]);
            }
            Flush();
        }

        protected override void Load(FileGetter file) throws IOException
        {
            gameMap.Clear();
            int am = file.I();
            for (int i = 0; i < am; i++)
            {
                string k = file.Chars();
                string v = file.Chars();
                gameMap.Put(k, v);
            }
            Read();
        }

        public static class PropGame
        {
            private readonly string prefix;

            public PropGame(string prefix)
            {
                this.prefix = prefix;
            }

            public int I(string key, int fallback)
            {
                string kk = Chars(prefix + "_" + key);
                if (kk == null)
                    return fallback;
                try
                {
                    int r = int.Parse(kk);
                    return r;
                }
                catch (FormatException)
                {
                    return fallback;
                }
            }

            public void SetI(string key, int i)
            {
                CharsSet(prefix + "_" + key, i.ToString());
            }

            public string Chars(string key)
            {
                if (s.gameMap.ContainsKey(key))
                    return s.gameMap[key];
                return null;
            }

            public void CharsSet(string key, string value)
            {
                s.gameMap.PutReplace(key, value);
            }
        }
    }
}