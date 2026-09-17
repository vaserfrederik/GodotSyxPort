using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Faction.Player
{
    public class PlayerColors
    {
        private static readonly KeyMap<LinkedList<PlayerColor>> cats = new KeyMap<LinkedList<PlayerColor>>();

        static PlayerColors()
        {
            GameDisposable.Add(new GameDisposable(() =>
            {
                cats.Clear();
            }));
        }

        public static KeyMap<LinkedList<PlayerColor>> Cats()
        {
            return cats;
        }

        public static class PlayerColor
        {
            public readonly ColorImp color;
            public readonly COLOR def;
            public readonly string name;
            public readonly string cat;
            public readonly string key;

            public PlayerColor(string key, string category, string name)
                : this(new ColorImp(), key, category, name)
            {
            }

            public PlayerColor(ColorImp col, string key, string category, string name)
            {
                this.key = key;
                this.cat = category;
                this.name = name;
                this.color = col;
                this.def = new ColorImp(col);
                if (!cats.ContainsKey(cat))
                    cats.Put(cat, new LinkedList<PlayerColor>());
                cats.Get(cat).Add(this);
            }
        }

        public static readonly SAVABLE Saver = new SAVABLE
        {
            Save = file =>
            {
                int am = 0;
                foreach (var li in cats.All())
                {
                    am += li.Size();
                }
                file.I(am);
                foreach (var li in cats.All())
                {
                    foreach (var col in li)
                    {
                        file.Chars(col.cat + col.key);
                        col.color.Save(file);
                    }
                }
            },
            Load = file =>
            {
                KeyMap<PlayerColor> map = new KeyMap<PlayerColor>();
                foreach (var li in cats.All())
                {
                    foreach (var col in li)
                    {
                        map.Put(col.cat + col.key, col);
                    }
                }
                int am = file.I();
                while (am-- > 0)
                {
                    string k = file.Chars();
                    ColorImp c = new ColorImp();
                    c.Load(file);
                    if (map.ContainsKey(k))
                        map.Get(k).color.Set(c);
                }
            },
            Clear = () =>
            {
                foreach (var li in cats.All())
                {
                    foreach (var col in li)
                    {
                        col.color.Set(col.def);
                    }
                }
            }
        };
    }
}