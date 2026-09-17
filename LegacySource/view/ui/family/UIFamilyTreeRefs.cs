using System;
using System.Collections.Generic;

namespace view.ui.family
{
    final class UIFamilyTreeRefs
    {
        private readonly int[] refs = new int[UIFamilyTree.MAX_REFS];
        private int max = 0;
        private Bitmap1D hasMore = new Bitmap1D(UIFamilyTree.MAX_REFS, false);

        private readonly Flooder f = GUTIL.Flooder();
        private readonly StatsRelations r = STATS.REL();

        UIFamilyTreeRefs()
        {
        }

        private readonly Coo cooTmp = new Coo();

        private COORDINATE coo(int ref)
        {
            cooTmp.set(ref % SETT.TWIDTH, ref / SETT.TWIDTH);
            return cooTmp;
        }

        void init(int mainRef)
        {
            const int MAX = r.references();
            const double unprocessedChild = -3;
            const double unprocessed = -2;
            const double invalid = -1;

            /**
             * identify leafs
             */
            for (int ref = 0; ref < MAX; ref++)
            {
                f.setValue2(coo(ref), unprocessedChild);
            }
            for (int ref = 0; ref < MAX; ref++)
            {
                if (r.isRef(ref) && r.hasParent(ref))
                {
                    f.setValue2(coo(r.parentRef(ref)), unprocessed);
                }
            }

            /**
             * set distance of parents from the main ref in value 2
             */
            {
                int ref = mainRef;
                for (int i = 0; i < 1000; i++)
                {
                    f.setValue2(coo(ref), i);
                    if (!r.hasParent(ref))
                        break;
                    ref = r.parentRef(ref);
                }
            }

            /**
             * now, we set the distance to the nodes in the tree, then mark the distance.
             */
            for (int ref = 0; ref < MAX; ref++)
            {
                if (!r.isRef(ref))
                    continue;
                if (f.getValue2(coo(ref)) != unprocessedChild)
                    continue;

                int traveler = ref;
                int dist = 1;
                f.setValue2(coo(ref), invalid);
                while (r.hasParent(traveler))
                {
                    int par = r.parentRef(traveler);
                    double vv = f.getValue2(coo(par));
                    if (vv >= 0)
                    {
                        traveler = ref;
                        while (dist > 0)
                        {
                            f.setValue2(coo(traveler), vv + dist);
                            dist--;
                            traveler = r.parentRef(traveler);
                        }
                        break;
                    }
                    dist++;
                    traveler = par;
                }
            }

            /**
             * Now we push all the nodes that have a distance to the main reference, value is the distance
             */
            f.init(this);
            for (int ref = 0; ref < MAX; ref++)
            {
                if (r.isRef(ref))
                {
                    COORDINATE coo = coo(ref);
                    double vv = f.getValue2(coo);
                    if (vv >= 0)
                    {
                        f.pushSloppy(coo, vv);
                    }
                }
            }
            /**
             * now we simply add the nodes.
             */
            max = 0;
            while (f.hasMore() && max < refs.Length)
            {
                PathTile t = f.pollSmallest();
                int ref = t.x() + t.y() * SETT.TWIDTH;
                refs[max] = ref;
                max++;
            }
            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                t.setValue2(invalid);
            }

            f.done();

            /**
             * Now we need to sort them by their reference, to get the same order every time.
             */
            f.init(this);
            for (int i = 0; i < max; i++)
            {
                int ref = refs[i];
                COORDINATE coo = coo(ref);
                f.pushSloppy(coo, ref);
            }
            max = 0;
            /**
             * we set their order in value2
             */
            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                refs[max] = t.x() + t.y() * SETT.TWIDTH;
                t.setValue2(max);
                max++;
            }
            f.done();

            /**
             * Then, we simply check our set for parents that were not included
             */
            hasMore.clear();
            for (int i = 0; i < max; i++)
            {
                int ref = refs[i];
                if (r.hasParent(ref))
                {
                    int pref = r.parentRef(ref);
                    if (f.getValue2(coo(pref)) < 0)
                    {
                        hasMore.set(ref, true);
                    }
                }
            }
            /**
             * also children that are not included, we mark their parent if it is included
             */
            for (int ref = 0; ref < MAX; ref++)
            {
                if (r.isRef(ref))
                {
                    COORDINATE coo = coo(ref);
                    double vv = f.getValue2(coo);
                    if (vv < 0 && r.hasParent(ref))
                    {
                        int po = (int)f.getValue2(coo(r.parentRef(ref)));
                        if (po >= 0)
                            hasMore.set(po, true);

                    }
                }
            }
        }

        public int get(int index)
        {
            return refs[index];
        }

        public bool hasChild(int index)
        {
            return hasMore.get(index);
        }

        public int max()
        {
            return max;
        }
    }
}