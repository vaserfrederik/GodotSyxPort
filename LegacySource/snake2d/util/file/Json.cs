using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Snake2D.Util.File
{
    public class Json
    {
        private JsonValueJson data;
        private KeyMap<bool> testMap = new KeyMap<bool>();
        private static bool untest = false;

        private Json(JsonValueJson content)
        {
            data = content;
        }

        public Json(string path)
        {
            data = Read(path);
        }

        public Json(string[] paths)
        {
            this(paths[paths.Length - 1]);
            for (int i = paths.Length - 2; i >= 0; i--)
            {
                JsonValueJson e = Read(paths[i]);
                bool arrayAdd = e.map.ContainsKey("_ARRAY_ADD");
                bool jsonAdd = e.map.ContainsKey("_JSON_ADD");
                data.Overwrite(e, arrayAdd, jsonAdd);
            }
        }

        private static JsonValueJson Read(string path)
        {
            try
            {
                byte[] encoded = File.ReadAllBytes(path);
                string s = Encoding.UTF8.GetString(encoded);
                return new JsonValueJson(null, 0, path, s);
            }
            catch (FileNotFoundException e2)
            {
                throw new Errors.DataError("File does not exist", path);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);
                throw new Exception("can't open file:\n" + path + "\n make sure encoding is UTF_8\n" + e.Message);
            }
        }

        public void CheckUnused()
        {
            if (untest)
                return;

            foreach (JsonValue v in data.map.All())
            {
                if (!testMap.ContainsKey(v.key))
                {
                    Console.Error.WriteLine("unknown key: " + v.key + " in object at line: " + v.line + ". " + path());
                    Console.Error.WriteLine("available: ");
                    Console.Error.WriteLine(testMap.KeysString());
                    untest = true;
                }
            }
        }

        public bool Has(string key)
        {
            testMap.PutReplace(key, true);
            return data.map.ContainsKey(key);
        }

        private JsonValue Get(string key, string type)
        {
            if (!data.map.ContainsKey(key))
                data.ThrowError("Missing a " + type + " with key: " + key);
            testMap.PutReplace(key, true);
            return data.map.Get(key);
        }

        private string GetValue(string key, string type)
        {
            if (!data.map.ContainsKey(key))
                data.ThrowError("Missing a " + type + " with key: " + key);
            testMap.PutReplace(key, true);
            JsonValue v = data.map.Get(key);
            string res = v.value();
            if (res == null)
                data.ThrowError("Missing a " + type + " with key: " + key);
            return res;
        }

        private string[] GetValues(string key, string type)
        {
            if (!data.map.ContainsKey(key))
                data.ThrowError("Missing a " + type + " with key: " + key);
            testMap.PutReplace(key, true);
            JsonValue v = data.map.Get(key);
            string[] res = v.values();
            if (res == null)
                data.ThrowError("Missing a " + type + " with key: " + key);
            return res;
        }

        public string Text(string key)
        {
            JsonValue v = Get(key, "String");
            string s = v.text();
            if (s == null)
                v.ThrowError("Expecting a String");
            return s;
        }

        public string Text(string key, string fallback)
        {
            if (!Has(key))
                return fallback;
            return Text(key);
        }

        public string[] Texts(string key)
        {
            JsonValue v = Get(key, "String Array");
            string[] s = v.texts();
            if (s == null)
                v.ThrowError("Expecting a string Array");
            return s;
        }

        public string[] TextsTry(string key)
        {
            if (!Has(key))
                return new string[0];
            return Texts(key);
        }

        public string[] Texts(string key, int size)
        {
            string[] res = Texts(key);
            if (res.Length != size)
                data.ThrowError("invalid length: '" + res.Length + "'" + " should be: " + size);
            return res;
        }

        public int I(string key)
        {
            string s = GetValue(key, "Integer number");
            try
            {
                return int.Parse(s);
            }
            catch (FormatException e)
            {
                data.map.Get(key).ThrowError("'" + s + "'" + " is not a valid integer.");
            }
            return 0;
        }

        public int I(string key, int min, int max)
        {
            int d = I(key);
            if (d < min || d >= max)
                data.map.Get(key).ThrowError(d + " is outside of valid range: " + min + "-" + max);
            return d;
        }

        public int I(string key, int min, int max, int fallback)
        {
            if (!Has(key))
                return fallback;
            int d = I(key);
            if (d < min || d >= max)
                data.map.Get(key).ThrowError(d + " is outside of valid range: " + min + "-" + max);
            return d;
        }

        public int[] Is(string key)
        {
            string[] s = GetValues(key, "integer number array");
            int[] res = Alloc.Ii(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                try
                {
                    res[i] = int.Parse(s[i]);
                }
                catch (FormatException e)
                {
                    data.map.Get(key).ThrowError("'" + s[i] + "'" + " is not a valid integer");
                }
            }
            return res;
        }

        public string Value(string key)
        {
            return GetValue(key, " a value");
        }

        public string Value(string key, string fallback)
        {
            if (!Has(key))
                return fallback;
            return Value(key);
        }

        public string[] Values(string key)
        {
            return GetValues(key, "value array");
        }

        public string[][] Values2(string key)
        {
            if (!data.map.ContainsKey(key))
                data.ThrowError("Missing a value array with key: " + key);
            testMap.PutReplace(key, true);
            JsonValue v = data.map.Get(key);
            string[][] res = v.values2();
            if (res == null)
                data.ThrowError("Missing a value array with key: " + key);
            return res;
        }

        public string[] Values(string key, int min, int max)
        {
            string[] vs = Values(key);
            if (vs.Length < min || vs.Length >= max)
                data.map.Get(key).ThrowError(" invalid length of array. Valid: " + min + "-" + max);
            return vs;
        }

        public void Error(string message, string key)
        {
            data.ThrowError(message, key);
        }

        public string ErrorGet(string message, string key)
        {
            return data.GetError(message, key);
        }

        public int Line(string key)
        {
            return data.line;
        }

        public LIST<string> Keys()
        {
            return data.keys;
        }

        private static readonly string sTrue = "true";
        private static readonly string sFalse = "false";

        public bool Bool(string key)
        {
            string v = Value(key);
            if (v.Equals(sTrue))
                return true;
            if (v.Equals(sFalse))
                return false;
            Error("illegal value: '" + v + "' for boolean type. only true/false is valid", key);
            return false;
        }

        public bool Bool(string key, bool fallback)
        {
            if (Has(key))
                return Bool(key);
            return fallback;
        }

        public static class KeyValue
        {
            public readonly string key;
            public readonly double value;

            public KeyValue(string key, double value)
            {
                this.key = key;
                this.value = value;
            }
        }

        public override string ToString()
        {
            return data.ToString();
        }
    }
}