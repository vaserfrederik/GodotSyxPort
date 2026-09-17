using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.gui.renderable;
using snake2d.util.sets;

namespace view.sett
{
    public static class UISettMap
    {
        private static readonly KeyMap<RENDEROBJ> map = new KeyMap<RENDEROBJ>();

        private UISettMap()
        {
        }

        public static void Clear()
        {
            map.Clear();
        }

        public static void Add(RENDEROBJ o, string key)
        {
            map.Put(key, o);
        }

        public static RENDEROBJ GetByKey(string key)
        {
            if (!map.ContainsKey(key))
            {
                foreach (string s in map.KeysSorted())
                    LOG.Ln(s);
            }
            return map.Get(key);
        }
    }
}