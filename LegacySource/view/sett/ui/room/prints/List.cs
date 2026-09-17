using System;
using System.Collections.Generic;
using System.Linq;

namespace view.sett.ui.room.prints
{
    final class List
    {
        private readonly Cat[] catmap = new Cat[SETT.ROOMS().all().size()];
        private readonly List<Cat> cats = new List<Cat>();
        private readonly List<Entry> free = new List<Entry>();
        private readonly List<Entry> current = new List<Entry>();
        private readonly StringInputSprite filter;

        public List(StringInputSprite filter)
        {
            this.filter = filter;
            foreach (RoomBlueprintImp b in SETT.ROOMS().imps())
            {
                if (SETT.ROOMS().copy.prints.canAdd(b))
                {
                    foreach (Cat c in cats)
                    {
                        if (c != null && c.prints[0].GetType() == b.GetType()
                                && c.prints[0].constructor().mustBeIndoors() == b.constructor().mustBeIndoors())
                        {
                            catmap[b.index()] = c;
                            catmap[b.index()].prints.Add(b);
                            break;
                        }
                    }

                    if (catmap[b.index()] == null)
                    {
                        catmap[b.index()] = new Cat(b);
                        cats.Add(catmap[b.index()]);
                    }
                }
            }
            next(0);
        }

        private int vi = -1;

        public IList<Entry> get()
        {
            if (vi == VIEW.RI())
                return current;
            vi = VIEW.RI();

            int fi = 0;
            current.Clear();
            foreach (Cat c in cats)
            {
                if (c == null)
                    continue;

                if (filter.text().Length != 0)
                {
                    bool contains = false;
                    foreach (RoomBlueprintImp b in c.prints)
                    {
                        if (Str.containsText(b.info.name, filter.text()))
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                        continue;
                }

                c.entries = 0;
                bool hasAny = false;
                bool locked = true;
                foreach (RoomBlueprintImp b in c.prints)
                {
                    if (SETT.ROOMS().copy.prints.all(b).Count > 0)
                    {
                        hasAny = true;
                        c.entries += SETT.ROOMS().copy.prints.all(b).Count;
                    }
                    if (b.reqs.passes(FACTIONS.player()))
                        locked = false;
                }

                if (!hasAny)
                    continue;

                Entry e = next(fi);
                fi++;
                e.cat = c;
                e.print = null;
                current.Add(e);

                int nn = current.Count;

                foreach (RoomBlueprintImp b in c.prints)
                {
                    foreach (SavedPrint p in SETT.ROOMS().copy.prints.all(b))
                    {
                        e = next(fi);
                        if (c.expanded)
                            current.Add(e);
                        fi++;
                        e.cat = c;
                        e.print = p;
                        e.isLocked = locked;
                    }
                }

                for (; nn < current.Count; nn++)
                {
                    e.isLocked = locked;
                }

                if (c.entries == 0)
                    c.expanded = false;
            }

            return current;
        }

        public void expand(SavedPrint p)
        {
            catmap[p.blue.index()].expanded = true;
            vi = -1;
        }

        private Entry next(int fi)
        {
            if (fi >= free.Count)
            {
                List<Entry> free = new List<Entry>(this.free.Count + 64);
                List<Entry> current = new List<Entry>(free.Capacity);
                foreach (Entry e in this.free)
                    free.Add(e);
                foreach (Entry e in this.current)
                    current.Add(e);
                for (int i = 0; i < 64; i++)
                    free.Add(new Entry());
                this.free = free;
                this.current = current;
            }
            return free[fi];
        }
    }
}