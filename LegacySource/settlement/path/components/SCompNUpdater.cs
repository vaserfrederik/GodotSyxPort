using System;
using System.Collections.Generic;

namespace Settlement.Path.Components
{
    public static class SETT
    {
        public static PATH PATH()
        {
            throw new NotImplementedException();
        }
    }

    public static class PATH
    {
        public static Components comps { get; } = new Components();
    }

    public class Components
    {
        public List<List<SComponent>> levels { get; } = new List<List<SComponent>>();
        public AllComponents all { get; } = new AllComponents();
    }

    public class AllComponents
    {
        public List<SComponent> this[int level]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class SCompNUpdater
    {
        private readonly SCompNFactory factory;
        private readonly Rec bounds;
        private readonly Rec boundsC = new Rec();
        private readonly SComponentChecker checkerUnderlings;
        private readonly SComponentChecker checkerSelf;
        private readonly SComponentLevel lower;
        private readonly ArrayListResize<SComponent> upUnderlings;
        private readonly int size;
        private readonly SCompNLevel map;

        public SCompNUpdater(SCompNLevel map, SCompNFactory f, SComponentLevel prev, int size)
        {
            this.lower = prev;
            this.factory = f;
            checkerUnderlings = new SComponentChecker(prev);
            checkerSelf = new SComponentChecker(map);
            upUnderlings = new ArrayListResize<SComponent>(SComp0Level.startSize >> map.level(), int.MaxValue);
            bounds = new Rec(size, size);
            this.size = size;
            this.map = map;
        }

        public void update()
        {
            checkerUnderlings.init();
            for (int i = 0; i < upUnderlings.size(); i++)
            {
                SComponent c = upUnderlings.get(i);
                if (!checkerUnderlings.isSetAndSet(c))
                {
                    update(c);
                }
            }

            upUnderlings.clearSoft();
        }

        public void remove(SComponent toBeRemoved)
        {
            if (toBeRemoved == null)
                return;
            SCompN o = (SCompN)toBeRemoved;
            if (o.superComp() != null)
            {
                SETT.PATH().comps.levels[map.level()].Remove(o.superComp());
            }
            GUTIL.filler().init(this);
            GUTIL.filler().fill(toBeRemoved.centreX(), toBeRemoved.centreY());
            while (GUTIL.filler().hasMore())
            {
                COORDINATE c = GUTIL.filler().poll();
                SComponent ss = SETT.PATH().comps.all[map.level() - 1][c.x(), c.y()];
                ss.superCompSet(null);
                SComponentEdge e = ss.edgefirst();
                while (e != null)
                {
                    if (e.to().superComp() == toBeRemoved)
                    {
                        GUTIL.filler().fill(e.to().centreX(), e.to().centreY());
                    }
                    e = e.next();
                }
            }
            GUTIL.filler().done();

            factory.retire(o);
        }

        public void add(SComponent c)
        {
            upUnderlings.add(c);
        }

        private void update(SComponent underling)
        {
            {
                int qx1 = size * (underling.centreX() / size);
                int qy1 = size * (underling.centreY() / size);
                bounds.moveX1Y1(qx1, qy1);
                boundsC.clear();
            }

            if (underling.superComp() != null)
            {
                throw new RuntimeException(size + " " + underling.superComp() + " " + underling.superComp().retired());
            }

            if (lower.get(underling.centreX(), underling.centreY()) != underling)
            {
                throw new RuntimeException(size + " " + underling.centreX() + " " + underling.centreY());
            }

            final SCompN comp = factory.create();

            Filler f = GUTIL.filler();
            f.init(this);
            f.fill(underling.centreX(), underling.centreY());

            while (f.hasMore())
            {
                COORDINATE coo = f.poll();
                SComponent c = lower.get(coo);
                if (c == null)
                    throw new RuntimeException();
                checkerUnderlings.isSetAndSet(c);
                c.superCompSet(comp);
                boundsC.unify(coo.x(), coo.y());
                SComponentEdge e = c.edgefirst();

                while (e != null)
                {
                    if (bounds.holdsPoint(e.to().centreX(), e.to().centreY()) && !checkerUnderlings.is(e.to()))
                    {
                        f.fill(e.to().centreX(), e.to().centreY());
                    }
                    e = e.next();
                }
            }
            f.done();

            comp.init(underling, boundsC, lower, checkerSelf);

            SETT.PATH().comps.data.initComponentN(comp);

            if (map.level() < SETT.PATH().comps.levels.Count)
            {
                SETT.PATH().comps.levels[map.level()].AddNew(comp);
            }
        }
    }
}