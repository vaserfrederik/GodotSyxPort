using System;
using System.Collections.Generic;
using System.IO;

namespace snake2d.util.file
{
    public class JsonE
    {
        private readonly LinkedList<Tuple<string, string>> list = new LinkedList<Tuple<string, string>>();
        private readonly HashSet<string> map = new HashSet<string>();

        public JsonE()
        {
        }

        public void Add(string key, string value)
        {
            PAdd(key, value);
        }

        private void PAdd(string key, string value)
        {
            if (map.Contains(key))
                throw new RuntimeException(" " + key);
            map.Add(key);
            list.Add(new Tuple<string, string>(key, value));
        }

        public void Add(string key, string[] values)
        {
            Add(key, new List<string>(values));
        }

        public void Add(string key, IList<string> values)
        {
            StringBuilder b = new StringBuilder();
            b.Append('[');
            b.Append(Environment.NewLine);
            foreach (string v in values)
            {
                b.Append(v);
                b.Append(',');
                b.Append(Environment.NewLine);
            }
            b.Append(']');
            Add(key, b.ToString());
        }

        public void AddString(string key, string value)
        {
            Add(key, "\"" + value + "\"");
        }

        public void AddStrings(string key, string[] values)
        {
            string[] vvs = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                vvs[i] = "\"" + values[i] + "\"";
            }
            Add(key, vvs);
        }

        public bool Has(string key)
        {
            return map.Contains(key);
        }

        public void Add(string key, bool b)
        {
            PAdd(key, b ? "true" : "false");
        }

        public void Add(string key, int i)
        {
            PAdd(key, "" + i);
        }

        public void Add(string key, int[] isValues)
        {
            string[] values = new string[isValues.Length];
            for (int i = 0; i < isValues.Length; i++)
            {
                values[i] = "" + isValues[i];
            }
            Add(key, values);
        }

        public void Add(string key, double d)
        {
            PAdd(key, "" + d);
        }

        public void Add(string key, double[] isValues)
        {
            string[] values = new string[isValues.Length];
            for (int i = 0; i < isValues.Length; i++)
            {
                values[i] = "" + isValues[i];
            }
            Add(key, values);
        }

        public void Add(string key, JsonE json)
        {
            StringBuilder b = new StringBuilder();
            b.Append('{');
            b.Append(Environment.NewLine);
            b.Append(json.ToString());
            b.Append('}');
            Add(key, b.ToString());
        }

        public void Add(string key, JsonE[] jsons)
        {
            string[] strings = new string[jsons.Length];
            for (int i = 0; i < jsons.Length; i++)
            {
                strings[i] = "{" + Environment.NewLine + jsons[i].ToString() + Environment.NewLine + "}";
            }
            Add(key, strings);
        }

        public void AddJ(string key, IList<JsonE> jsons)
        {
            string[] strings = new string[jsons.Count];
            for (int i = 0; i < jsons.Count; i++)
            {
                strings[i] = '\t' + "{" + Environment.NewLine + jsons[i].ToString(2) + Environment.NewLine + '\t' + "}";
            }
            Add(key, strings);
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public string ToString(int tabs)
        {
            StringBuilder b = new StringBuilder();
            foreach (Tuple<string, string> e in list)
            {
                for (int t = 0; t < tabs; t++)
                    b.Append('\t');
                b.Append(e.Item1);
                b.Append(':');
                b.Append(' ');
                string[] ss = e.Item2.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                b.Append(ss[0]);
                for (int i = 1; i < ss.Length; i++)
                {
                    b.Append(Environment.NewLine);
                    for (int t = 0; t < tabs; t++)
                        b.Append('\t');
                    if (i < ss.Length - 1)
                        b.Append('\t');
                    b.Append(ss[i]);
                }
                b.Append(',');
                b.Append(Environment.NewLine);
            }
            return b.ToString();
        }

        public bool Save(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                using (StreamWriter outStream = new StreamWriter(path))
                {
                    outStream.WriteLine(ToString());
                }
                return true;
            }
            catch (FileNotFoundException e)
            {
                e.printStackTrace();
                return false;
            }
        }

        public bool Save(Path path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                using (StreamWriter outStream = new StreamWriter(path))
                {
                    outStream.WriteLine(ToString());
                }
                return true;
            }
            catch (Exception e)
            {
                e.printStackTrace();
                return false;
            }
        }
    }
}