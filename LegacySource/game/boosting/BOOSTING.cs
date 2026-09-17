using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using init;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.keymap;

namespace game.boosting
{
    public sealed class BOOSTING : INIT.InitResource
    {
        public static readonly string KEY = "BOOST";

        private static readonly Map map = new Map();
        static readonly LinkedList<ACTION> waiting = new LinkedList<ACTION>();
        private static readonly LinkedList<ACTION> connecters = new LinkedList<ACTION>();
        // private static readonly ArrayListGrower<Boostable> all = new ArrayListGrower<>();
        static bool hasErrored = false;
        // private static BTypes types;

        // static
        // {
        //     new GameDisposable()
        //     {
        //         protected override void Dispose()
        //         {
        //             clear();
        //         }
        //     };
        // }

        static void clear()
        {
            map.clear();
            waiting.Clear();
            connecters.Clear();
            hasErrored = false;
            // all.Clear();
            // types = new BTypes();
        }

        public BOOSTING(INIT init) : base(init)
        {
            clear();
            BOOSTABLES.init();
            BoostableCat.init();
        }

        protected override void finishSetup()
        {
            foreach (var a in waiting)
                a.exe();
            foreach (var a in connecters)
            {
                a.exe();
            }

            // List<Boostable> all = new List<Boostable>(BOOSTING.all.size());
            // all.Add(BOOSTING.all);

            // all.Sort((o1, o2) =>
            // {
            //     int c = o1.cat.name.CompareTo(o2.cat.name);
            //     if (c == 0)
            //     {
            //         return o1.name.CompareTo(o2.name);
            //     }
            //     return c;
            // });

            // BOOSTING.all.Clear();
            // BOOSTING.all.Add(all);

            waiting.Clear();
            connecters.Clear();
            base.finishSetup();
        }

        public static LIST<Boostable> ALL()
        {
            return map.all();
        }

        // public static BTypes TYPES()
        // {
        //     return types;
        // }

        public static void connecter(ACTION a)
        {
            connecters.Add(a);
        }

        public static string available()
        {
            StringBuilder s = new StringBuilder();
            foreach (var ss in map.map().keysSorted())
            {
                s.Append(ss);
                s.Append("  - ");
                s.Append(map.map().get(ss).name);
                s.Append(System.Environment.NewLine);
            }
            return s.ToString();
        }

        public static Boostable push(string key, double baseValue, char[] name, char[] desc, SPRITE icon, BoostableCat cat)
        {
            return push(key, baseValue, name, desc, icon, cat, -10000000);
        }

        public static Boostable push(string key, double baseValue, char[] name, char[] desc, SPRITE icon, BoostableCat cat, double minValue)
        {
            if (key[0] == '_')
                key = key.Substring(1);
            key = cat.prefix + key;

            Boostable b = new Boostable(map.all().size(), key, baseValue, name, desc, icon, cat, minValue);

            map.add(b);

            return b;
        }

        public static RMAP<Boostable> MAP()
        {
            return map;
        }

        static class Entry
        {
            public readonly ArrayListGrower<Boostable> all = new ArrayListGrower<Boostable>();
            public readonly bool isMaster;

            Entry(Boostable b, bool isMaster)
            {
                all.Add(b);
                this.isMaster = isMaster;
            }
        }
    }
}