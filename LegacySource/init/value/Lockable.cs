using System;
using System.Collections.Generic;
using game;
using init.sprite;
using init.value;
using snake2d;
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
    public class Lockable<T>
    {
        private static readonly CharSequence ¤¤requires = "¤Requires";

        static Lockable()
        {
            D.ts(typeof(Lockable<T>));
        }

        private readonly ArrayListGrower<Lock<T>> res = new ArrayListGrower<Lock<T>>();
        public readonly GValueCat<T> values;
        private static bool hasSpewed = false;

        public readonly CharSequence name;
        public readonly CharSequence desc;
        public readonly string key;
        public readonly SPRITE icon;

        public Lockable(string key, CharSequence name, CharSequence desc, SPRITE icon, GValueCat<T> mm)
        {
            this.values = mm;
            this.name = name;
            this.icon = icon;
            this.desc = desc;
            this.key = key;
        }

        public LIST<Lock<T>> all()
        {
            return res;
        }

        public bool passes(T t)
        {
            foreach (Lock<T> r in all())
            {
                if (!r.unlocker.inUnlocked(t))
                    return false;
            }
            return true;
        }

        public double progress(T t)
        {
            if (all().size() == 0)
                return 1.0;
            double d = 0;
            foreach (Lock<T> r in all())
            {
                d += CLAMP.d(r.unlocker.progress(t), 0, 1);
            }

            return d / all().size();
        }

        public bool hover(GUI_BOX text, T t)
        {
            return hover(text, t, ¤¤requires);
        }

        public bool hover(GUI_BOX text, T t, CharSequence title)
        {
            if (all().size() == 0)
                return false;
            GBox b = (GBox)text;
            b.textLL(title);
            b.NL();

            foreach (Lock<T> r in all())
            {
                r.unlocker.hover(text, t);
                b.NL();
            }
            return true;
        }

        public void debug(T t)
        {
            LOG.ln(t);
            foreach (Lock<T> r in all())
            {
                LOG.ln(r.unlocker.name + " " + r.unlocker.inUnlocked(t) + " " + r.unlocker.progress(t));
            }
            LOG.ln();
        }

        public void push(string key, double value, object path, COMPARATOR comp)
        {
            RPromise p = new RPromise(key, value, path.ToString(), comp);
            values.inits.add(p);
        }

        public void push(Lock<T> r)
        {
            res.add(r);
        }

        public void push(Json json)
        {
            push("REQUIRES", json);
        }

        public void push(string key, Json json)
        {
            if (!json.has(key))
                return;
            json = json.json(key);
            pushPush(json);
        }

        public void pushPush(Json json)
        {
            foreach (string keyComp in json.keys())
            {
                COMPARATOR comp = COMPARATOR.map.get(keyComp, json);
                if (comp != null)
                {
                    Json j = json.json(keyComp);
                    foreach (string k in j.keys())
                    {
                        RPromise p = new RPromise(k, j.d(k), j.path() + ", line" + j.line(k), comp);
                        values.inits.add(p);
                    }
                }
            }
        }

        private class RPromise : ACTION
        {
            private readonly string key;
            private readonly double value;
            private readonly string path;
            private readonly COMPARATOR comp;

            public RPromise(string key, double value, string path, COMPARATOR comp)
            {
                this.key = key;
                this.value = value;
                this.path = path;
                this.comp = comp;
            }

            public void exe()
            {
                if (values.get(key) == null)
                {
                    if (!hasSpewed)
                        GAME.Warn(path + Environment.NewLine + "no " + values.key + " named : " + key + " available: " + Environment.NewLine + values.available());
                    else
                    {
                        LOG.err(path + Environment.NewLine + "no " + values.key + " named : " + key);
                    }
                    hasSpewed = true;

                    Value<T> v = new Value<T>(key, SPRITES.icons().s.cancel, "unknown", new DOUBLE_O<T>()
                    {
                        public double getD(T t)
                        {
                            return 0;
                        }
                    }, false, true);

                    Locker<T> un = new LockerValue<T>(comp, v, 1, icon);
                    Lock<T> lockObj = new Lock<T>(this, un);
                    res.add(lockObj);

                }
                else
                {

                    Value<T> v = values.get(key);

                    Locker<T> un = new LockerValue<T>(comp, v, value, icon);
                    Lock<T> lockObj = new Lock<T>(this, un);
                    res.add(lockObj);

                }

            }
        }

        public void clear()
        {
            res.clear();
        }
    }
}