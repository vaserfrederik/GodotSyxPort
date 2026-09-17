using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Util.KeyMap
{
    public class MAPSAVE
    {
        public static void SaveMeta(FilePutter saveFile, LIST<MAPPED> all)
        {
            saveFile.I(all.Size());
            foreach (var j in all)
            {
                saveFile.Chars(j.Key());
            }
        }

        public static int[] SaveWash(FileGetter f, LIST<MAPPED> all, int nothingReplacer)
        {
            int am = f.I();
            int[] order = new int[am];
            KeyMap<MAPPED> map = new KeyMap<MAPPED>();

            foreach (var t in all)
            {
                map.Put(t.Key(), t);
            }

            order.Fill(nothingReplacer);
            bool different = false;
            for (int i = 0; i < am; i++)
            {
                string k = f.Chars();

                order[i] = map.ContainsKey(k) ? map.Get(k).Index() : nothingReplacer;
                different |= order[i] != i;
            }
            if (!different)
                return null;
            return order;
        }
    }
}