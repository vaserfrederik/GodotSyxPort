using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake2d.Util.Sets
{
    public class KeyMap<T>
    {
        private readonly Dictionary<string, T> map = new Dictionary<string, T>();

        public KeyMap()
        {
            // TODO Auto-generated constructor stub
        }

        public void Put(string key, T t)
        {
            if (map.ContainsKey(key))
                throw new RuntimeException("'" + key + "' " + map[key] + " " + t);
            map[key] = t;
        }

        public void PutReplace(string key, T t)
        {
            map[key] = t;
        }

        public void Remove(string key)
        {
            map.Remove(key);
        }

        public bool ContainsKey(string key)
        {
            return map.ContainsKey(key);
        }

        public T Get(string key)
        {
            return map[key];
        }

        public void Debug()
        {
            foreach (var s in map.Keys)
                Console.Error.WriteLine(s);
        }

        public void Expand()
        {
            var bb = new List<string>(50);
            foreach (var s in map.Keys)
            {
                if (s.StartsWith("_"))
                {
                    string key = s.Substring(1);
                    if (map.ContainsKey(key))
                        continue;
                    bb.Add(s);
                }
            }

            foreach (var s in bb)
            {
                map[s.Substring(1)] = map[s];
            }
        }

        public int Size()
        {
            return map.Count;
        }

        public List<T> All()
        {
            return new List<T>(map.Values);
        }

        public List<T> AllSorted()
        {
            string[] keys = map.Keys.ToArray();
            Array.Sort(keys);
            List<T> res = new List<T>(keys.Length);
            foreach (var k in keys)
                res.Add(Get(k));
            return res;
        }

        public Set<string> Keys()
        {
            return new HashSet<string>(map.Keys);
        }

        public List<string> KeysSorted()
        {
            string[] keys = map.Keys.ToArray();
            Array.Sort(keys);
            List<string> res = new List<string>(keys.Length);
            foreach (var k in keys)
                res.Add(k);
            return res;
        }

        public string KeysString()
        {
            string s = "";
            foreach (var ss in KeysSorted())
            {
                s += ss + Environment.NewLine;
            }
            return s;
        }

        public void Clear()
        {
            map.Clear();
        }

        public sealed class CharMap<T>
        {
            private string[] table = new string[0];
            private object[] content = new object[0];
            private int last = 0;

            public void PutReplace(string key, T t)
            {
                int i = Search(key);
                if (i != -1)
                    content[i] = t;
                else
                    Put(key, t);
            }

            public T Get(IReadOnlyList<char> key)
            {
                int i = Search(key);
                if (i == -1)
                    return default(T);
                return (T)content[i];
            }

            public bool ContainsKey(IReadOnlyList<char> key)
            {
                return Search(key) != -1;
            }

            public void Remove(IReadOnlyList<char> key)
            {
                int i = Search(key);
                if (i < 0)
                    throw new RuntimeException();
                last--;
                for (; i < last; i++)
                {
                    table[i] = table[i + 1];
                }
            }

            public int Size()
            {
                return last;
            }

            public bool IsEmpty()
            {
                return last == 0;
            }

            public int Put(string key, T e)
            {
                if (Search(key) != -1)
                    throw new RuntimeException("'" + key + "' " + Get(key) + " " + e);
                last++;
                if (last >= table.Length)
                {
                    string[] table2 = new string[table.Length + 1];
                    object[] content2 = new object[table.Length + 1];
                    for (int i = 0; i < table.Length; i++)
                    {
                        table2[i] = table[i];
                        content2[i] = content[i];
                    }
                    table = table2;
                    content = content2;
                }
                for (int i = last - 1; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        table[i] = key;
                        content[i] = e;
                        return 0;
                    }
                    int comp = Compare(key, i - 1);
                    if (comp > 0)
                    {
                        table[i] = key;
                        content[i] = e;
                        return i;
                    }
                    if (comp == 0)
                        throw new RuntimeException();
                    table[i] = table[i - 1];
                    content[i] = content[i - 1];
                }
                return -1;
            }

            private int Compare(IReadOnlyList<char> key, int index)
            {
                string other = table[index];
                int le = Math.Max(key.Count, other.Length);

                for (int i = 0; i < le; i++)
                {
                    if (i >= key.Count)
                        return -1;
                    if (i >= other.Length)
                        return 1;
                    int bb = key[i] - other[i];
                    if (bb != 0)
                        return bb;
                }
                return 0;
            }

            private int Search(IReadOnlyList<char> key)
            {
                return RunBinarySearchIteratively(key, 0, last - 1);
            }

            private int RunBinarySearchIteratively(IReadOnlyList<char> key, int low, int high)
            {
                while (low <= high)
                {
                    int mid = low + ((high - low) / 2);
                    int comp = Compare(key, mid);

                    if (comp > 0)
                    {
                        low = mid + 1;
                    }
                    else if (comp < 0)
                    {
                        high = mid - 1;
                    }
                    else if (comp == 0)
                    {
                        return mid;
                    }
                }
                return -1;
            }

            public List<T> All()
            {
                List<T> tt = new List<T>(content.Length);
                foreach (var o in content)
                    tt.Add((T)o);
                return tt;
            }

            public List<string> KeysSorted()
            {
                List<string> res = new List<string>(table);
                return res;
            }

            public string KeysString()
            {
                string s = "";
                foreach (var ss in KeysSorted())
                {
                    s += ss + Environment.NewLine;
                }
                return s;
            }
        }
    }
}