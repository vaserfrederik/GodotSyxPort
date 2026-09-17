using System;
using System.Collections.Generic;
using game;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util;
using util.data;

namespace settlement.path.components
{
    class FindableData : INT_O<SComponent>
    {
        static LinkedList<FindableData> all = new LinkedList<FindableData>();
        public readonly string name;
        readonly INT_OE<SComponent> data;
        readonly INT_OE<SComponent> overflow;

        static DataOSimple<SComponent> datao;

        FindableData(string name)
        {
            this.name = name;
            this.data = datao.newDataByte();
            this.overflow = datao.newDataBit();
            all.Add(this);
        }

        public int min(SComponent c)
        {
            return 0;
        }

        public int max(SComponent c)
        {
            return 15;
        }

        public int get(SComponent c)
        {
            return data.get(c);
        }

        public bool overflow(SComponent c)
        {
            return overflow.isMax(c);
        }

        void add(SComponent c)
        {
            if (data.isMax(c))
            {
                overflow.set(c, 1);
            }
            else
            {
                data.inc(c, 1);
            }
        }

        bool remove(SComponent c)
        {
            int a = data.get(c);

            if (a == 0)
            {
                if (overflow.get(c) == 0)
                {
                    if (c.level().level() > 0)
                    {
                        GUTIL.filler().init(this);
                        GUTIL.filler().fill(c.centreX(), c.centreY());

                        SComponentLevel l = SETT.PATH().comps.all.get(c.level().level() - 1);

                        while (GUTIL.filler().hasMore())
                        {
                            COORDINATE coo = GUTIL.filler().poll();
                            SComponent s = l.get(coo);

                            SComponentEdge e = s.edgefirst();
                            while (e != null)
                            {
                                if (e.to().superComp() == c)
                                {
                                    GUTIL.filler().fill(e.to().centreX(), e.to().centreY());
                                }
                                e = e.next();
                            }
                        }

                        GUTIL.filler().done();
                    }
                    GAME.Notify(name + " " + c.centreX() + " " + c.centreY() + " " + c.level().level());
                }

                return true;
            }
            data.inc(c, -1);
            if (a == 0)
            {
                if (overflow.get(c) == 1)
                    return true;
            }
            return false;
        }

        private static readonly LIST<DIR> dirs = new ArrayList<DIR>(DIR.ORTHO).join(DIR.C);

        private static void uncheck(int tx, int ty)
        {
            foreach (DIR d in dirs)
            {
                SComponent n = PATH().comps.zero.get(tx, ty, d);
                if (n != null)
                {
                    n.checked = false;
                }
            }
        }

        public void reportPresence(int tx, int ty)
        {
            uncheck(tx, ty);

            foreach (DIR d in dirs)
            {
                SComponent n = PATH().comps.zero.get(tx, ty, d);
                if (n != null && !n.checked)
                {
                    n.checked = true;
                    add(n);

                    while (n.superComp() != null && get(n) == 1)
                    {
                        n = n.superComp();
                        add(n);
                    }
                }
            }
        }

        public void reportAbsence(int tx, int ty)
        {
            uncheck(tx, ty);
            bool up = false;
            foreach (DIR d in dirs)
            {
                SComponent n = PATH().comps.zero.get(tx, ty, d);
                if (n != null && !n.checked)
                {
                    n.checked = true;
                    int old = get(n);
                    up |= remove(n);
                    while (n.superComp() != null && old == 1)
                    {
                        n = n.superComp();
                        old = get(n);
                        up |= remove(n);
                    }
                }
            }

            if (up)
            {
                PATH().comps.updateService(tx, ty);
            }
        }
    }
}