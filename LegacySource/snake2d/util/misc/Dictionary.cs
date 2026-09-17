using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Snake2D.Util.Misc
{
    public sealed class Dictionary
    {
        private readonly IEnumerable<char>[] keys;
        private readonly IEnumerable<char>[] values;
        private readonly string path;

        private Dictionary(string path)
        {
            this.path = path;
            Json json = new Json(File.OpenRead(path));
            List<string> keysList = json.Keys();
            this.keys = new IEnumerable<char>[keysList.Count];
            values = new IEnumerable<char>[keysList.Count];

            var t = new Tree<string>(values.Length)
            {
                IsGreaterThan = (current, cmp) => Compare(current, cmp) == -1
            };

            foreach (var s in keysList)
            {
                t.Add(s);
            }

            for (int i = 0; i < this.keys.Length; i++)
            {
                string s = t.PollSmallest();
                this.keys[i] = s.ToCharArray();
                values[i] = json.Text(s).ToCharArray();
            }
        }

        public IEnumerable<char> Get(IEnumerable<char> key)
        {
            int start = 0;
            int length = keys.Length - 1;

            while (length >= start)
            {
                int mid = start + (length - start) / 2;

                int c = Compare(keys[mid], key);

                if (c == 0)
                {
                    return values[mid];
                }

                if (c == -1)
                {
                    length = mid - 1;
                }
                else
                {
                    start = mid + 1;
                }
            }
            Notify(key);
            return key;
        }

        private void Notify(IEnumerable<char> key)
        {
            // new Exception().PrintStackTrace(System.out);
        }

        public IEnumerable<char> Camel(IEnumerable<char> key)
        {
            int start = 0;
            int length = keys.Length - 1;

            while (length >= start)
            {
                int mid = start + (length - start) / 2;

                int c = Compare(keys[mid], key);

                if (c == 0)
                {
                    return values[mid];
                }

                if (c == -1)
                {
                    length = mid - 1;
                }
                else
                {
                    start = mid + 1;
                }
            }
            Console.Error.WriteLine("Couldn't find mapping for: " + new string(key.ToArray()));
            Console.Error.WriteLine("in: : " + path);
            return key;
        }

        public static int Compare(IEnumerable<char> current, IEnumerable<char> cmp)
        {
            if (current == null)
                return -1;
            if (cmp == null)
                return 1;

            int len1 = current.Count();
            int len2 = cmp.Count();

            for (int i = 0; i < len1; i++)
            {
                if (i >= len2)
                    return 1;
                if (current.ElementAt(i) < cmp.ElementAt(i))
                    return -1;
                if (current.ElementAt(i) > cmp.ElementAt(i))
                    return 1;
            }
            return len2 > len1 ? -1 : 0;
        }

        public static double CompareValue(IEnumerable<char> s)
        {
            if (s == null)
                return -1;

            double v = 0;
            int len = s.Count();
            for (int i = 0; i < len; i++)
            {
                v += s.ElementAt(i) * (len - i);
            }
            return v;
        }
    }
}