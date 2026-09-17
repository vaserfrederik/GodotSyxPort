using System;
using System.Collections.Generic;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.data;
using util.gui.misc;
using util.text;

namespace init.value
{
    public class Lockers
    {
        private static readonly string ¤¤uworld = "Unlocks (World)";
        private static readonly string ¤¤uIndu = "Unlocks (Subject)";
        private static readonly string ¤¤uFaction = "Unlocks (Faction)";

        static Lockers()
        {
            D.ts(typeof(Lockers));
        }

        private readonly ArrayListGrower<Lock<object>> locks = new ArrayListGrower<Lock<object>>();
        public readonly string name;
        public readonly SPRITE icon;

        public Lockers(string name, SPRITE icon)
        {
            this.name = name;
            this.icon = icon;
        }

        public void Add(GValueCat<object> vv, Json json, DOUBLE_O<object> v)
        {
            Add(vv, "UNLOCKS_" + vv.key, json, v);
        }

        public void Add(GValueCat<object> vv, string key, Json json, DOUBLE_O<object> v)
        {
            if (!json.Has(key))
                return;

            foreach (string s in json.Values(key))
            {
                Locker<object> locker = new Locker<object>(name, icon)
                {
                    InUnlocked = (t) => v.GetD(t) >= 1,
                    Hover = (text, t) => Hover(text, v.GetD(t)),
                    Progress = (t) => v.GetD(t)
                };

                LPromise<object> p = new LPromise<object>();
                p.key = s;
                p.path = json.Path() + " line:" + json.Line(key);
                p.locker = locker;
                p.vv = vv;
                vv.LOCK.inits.Add(p);
            }
        }

        protected void Hover(GUI_BOX text, double value)
        {
            GBox b = (GBox)text;
            if (value == 1)
            {
                b.Add(b.Text().Normalify2().Add(name));
            }
            else
            {
                b.Add(b.Text().Warnify().Add(name));
            }
            b.NL();
        }

        public void Hover(GUI_BOX text)
        {
            if (All().Size() > 0)
            {
                GBox b = (GBox)text;
                Hover(b, GVALUES.INDU, ¤¤uIndu);
                Hover(b, GVALUES.REGION, ¤¤uworld);
                Hover(b, GVALUES.FACTION, ¤¤uFaction);
                b.Sep();
            }
        }

        private void Hover(GBox b, GValueCat<object> v, string title)
        {
            bool has = false;
            foreach (Lock<object> l in All())
            {
                if (l.lockable.values == v)
                {
                    has = true;
                    break;
                }
            }
            if (has)
            {
                b.TextLL(title);
                b.NL();
                foreach (Lock<object> l in All())
                {
                    if (l.lockable.values == v)
                    {
                        b.Add(l.lockable.icon);
                        b.Text(l.lockable.name);
                        b.NL();
                    }
                }
            }
        }

        public LIST<Lock<object>> All()
        {
            return locks;
        }

        private class LPromise<T> : ACTION
        {
            public string key;
            public string path;
            public Locker<T> locker;
            public GValueCat<T> vv;

            public void Exe()
            {
                Lockable<T> lockable = vv.LOCK.Get(key);
                if (lockable == null)
                {
                    if (!vv.LOCK.hasSpewed)
                        GAME.Warn(path + Environment.NewLine + "no UNLOCKABLE " + vv.key + " named : " + key + " available: " + Environment.NewLine + vv.LOCK.Available());
                    else
                    {
                        LOG.Ln(path + Environment.NewLine + "no UNLOCKABLE " + vv.key + " named : " + key);
                    }
                    vv.LOCK.hasSpewed = true;
                    return;
                }
                Lock<T> lockObj = new Lock<T>(lockable, locker);
                lockable.res.Add(lockObj);
                Lockers.this.locks.Add(lockObj);
            }
        }
    }
}