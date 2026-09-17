using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Boosting
{
    using GAME = game.GAME;
    using LOG = snake2d.LOG;
    using COLOR = snake2d.util.color.COLOR;
    using Json = snake2d.util.file.Json;
    using GUI_BOX = snake2d.util.gui.GUI_BOX;
    using ACTION = snake2d.util.misc.ACTION;
    using ArrayList = snake2d.util.sets.ArrayList;
    using ArrayListGrower = snake2d.util.sets.ArrayListGrower;
    using LIST = snake2d.util.sets.LIST;
    using LinkedList = snake2d.util.sets.LinkedList;
    using SPRITE = snake2d.util.sprite.SPRITE;
    using GCOLOR = util.colors.GCOLOR;
    using BOOLEANO = util.data.BOOLEANO;
    using GBox = util.gui.misc.GBox;
    using GText = util.gui.misc.GText;
    using GFORMAT = util.info.GFORMAT;
    using Dic = util.text.Dic;

    public class BoostSpecs
    {
        private readonly ArrayListGrower<BoostSpec> all = new ArrayListGrower<BoostSpec>();
        public readonly bool connect;

        public const string MUL = "MUL";
        public const string ADD = "ADD";

        public BoostSpecs(string name, SPRITE icon, bool connect)
        {
            this.connect = connect;
        }

        public void Push(string key, Booster factor, string path, bool isWeak)
        {
            if (li == null)
            {
                li = new PromiseList(this);
                BOOSTING.waiting.Add(li);
            }
            li.Push(key, factor, path, isWeak);
        }

        private void Push(BoostSpec spec)
        {
            all.Add(spec);
        }

        public void Read(Json json)
        {
            foreach (var entry in json)
            {
                string key = entry.Key;
                var factor = new Booster(entry.Value);
                Push(key, factor, "", false);
            }
        }

        public void Hover(GUI_BOX text, double input, string name, BOOLEANO<BoostSpec> filter, int catMask)
        {
            GBox b = (GBox)text;
            if (name != null)
                b.TextLL(name);
            b.NL();
            int tab = 0;

            foreach (BoostSpec l in all())
            {
                double d = l.booster.GetValue(input);
                if ((filter == null || filter.Is(l)) && (l.boostable.cat.typeMask & catMask) != 0 && Hover(b, l, d, tab))
                {
                    if (tab >= 1)
                    {
                        tab = 0;
                        b.NL();
                    }
                    else
                    {
                        tab++;
                    }
                }
            }
            b.NL();
        }

        private bool Hover(GBox b, BoostSpec l, double d, int tab)
        {
            if (l.boostable.name == null || l.boostable.name.Length == 0)
                return false;

            COLOR c = GCOLOR.T().INACTIVE;

            GText t = b.Text();

            if (l.booster.isMul)
            {
                if (d < 1)
                    c = GCOLOR.T().IBAD;
                else if (d > 1)
                    c = GCOLOR.T().IGOOD;
                d -= 1;
                GFORMAT.PercInc(t, d);
            }
            else
            {
                if (d < 0)
                    c = GCOLOR.T().IBAD;
                else if (d > 0)
                    c = GCOLOR.T().IGOOD;
                if (d == (int)d)
                    GFORMAT.IIncr(t, (int)d);
                else
                    GFORMAT.F0(t, d);
            }

            b.Tab(tab * (htab + 2));
            b.Add(l.boostable.icon.small);
            b.Add(b.Text().Color(c).Add(l.tName));
            b.Tab(tab * (htab + 2) + htab);

            t.Color(c);
            b.Add(t);
            return true;
        }

        private static class PromiseList : ACTION
        {
            public readonly LinkedList<Promise> all = new LinkedList<Promise>();
            public readonly BoostSpecs coll;

            public PromiseList(BoostSpecs coll)
            {
                this.coll = coll;
            }

            public void Exe()
            {
                foreach (Promise p in this.all)
                {
                    LIST<Boostable> bos = BOOSTING.MAP().Get(p.key);

                    if (bos.Size() == 0)
                    {
                        string m = p.path + Environment.NewLine + "no BOOSTABLE " + " named : " + p.key;
                        if (BOOSTING.hasErrored)
                        {
                            LOG.Ln(m);
                        }
                        else
                        {
                            BOOSTING.hasErrored = true;
                            GAME.Warn(m + Environment.NewLine + "Available:" + Environment.NewLine + BOOSTING.available());
                        }
                        continue;
                    }

                    bool isWeak = bos.Size() > 1;

                    foreach (Boostable bo in bos)
                    {
                        Add(p, bo, isWeak);
                    }
                }

                ArrayList<BoostSpec> rr = new ArrayList<BoostSpec>(coll.all.Size());
                foreach (BoostSpec s in coll.all)
                {
                    if (s.booster.isMul && s.booster.From() == 1 && s.booster.To() == 1)
                    {
                        continue;
                    }
                    else if (!s.booster.isMul && s.booster.From() == 0 && s.booster.To() == 0)
                    {
                        continue;
                    }
                    rr.Add(s);
                }

                rr.Sort((o1, o2) =>
                {
                    int ii = string.Compare(o1.boostable.cat.name, o2.boostable.cat.name);
                    if (ii == 0)
                    {
                        return string.Compare(o1.boostable.name, o2.boostable.name);
                    }
                    return ii;
                });
                coll.all.Clear();
                coll.all.Add(rr);
                coll.li = null;
            }

            private void Add(Promise p, Boostable b, bool isWeak)
            {
                BoostSpec boost = new BoostSpec(p.factor, b, null);
                foreach (BoostSpec bb in coll.all)
                {
                    if (boost.IsSameAs(bb))
                    {
                        if (isWeak)
                            return;
                        coll.Remove(bb);
                        if (coll.connect)
                        {
                            bb.boostable.RemoveFactor(bb);
                        }
                        break;
                    }
                }

                coll.Push(boost);
            }

            private void Push(string key, Booster factor, string path, bool isWeak)
            {
                for (Promise pp in all)
                {
                    if (pp.key == key && pp.factor.isMul == factor.isMul)
                        all.Remove(pp);
                }

                Promise p = new Promise(key, factor, path, isWeak);
                all.Add(p);
            }

            private static class Promise
            {
                public readonly string key;
                public readonly string path;
                public readonly Booster factor;

                public Promise(string key, Booster factor, string path, bool isWeak)
                {
                    this.key = key;
                    this.factor = factor;
                    this.path = path;
                }
            }
        }

        public double Max(Boostable bo)
        {
            double m = 0;
            foreach (BoostSpec s in all())
                if (s.boostable == bo)
                    m = Math.Max(m, s.booster.Max());
            return m;
        }

        private static int htab = 2;
    }
}