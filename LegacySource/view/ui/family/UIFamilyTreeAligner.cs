using System;
using System.Collections.Generic;
using System.Linq;

namespace View.UI.Family
{
    public class UIFamilyTreeAligner
    {
        private readonly Flooder f = GUTIL.Flooder();
        private readonly StatsRelations r = STATS.REL();
        private readonly BOOLEANImp redo = new BOOLEANImp(true);
        public int maxIterations = 128;

        private readonly float[] x2s = new float[UIFamilyTree.MAX_REFS];
        private readonly float[] levels = new float[UIFamilyTree.MAX_REFS];
        private readonly float[] parentXs = new float[UIFamilyTree.MAX_REFS];
        private readonly PathTile[] tmpLevel = new PathTile[UIFamilyTree.MAX_REFS];

        private readonly Coo cooTmp = new Coo();

        /**
         * sorts references by their parent order
         */
        private readonly IComparer<PathTile> parentSort = new ParentSorter();

        public UIFamilyTreeAligner()
        {
        }

        private COORDINATE coo(int ref)
        {
            cooTmp.Set(ref % SETT.TWIDTH, ref / SETT.TWIDTH);
            return cooTmp;
        }

        public void Init(UIFamilyTreeRefs refs)
        {
            f.Init(this);
            int MAX = refs.Max();
            /**
             * set depth 1 - N in value2 for all references. Set the amount of children for each reference.
             */
            {
                for (int refI = 0; refI < refs.Max(); refI++)
                {
                    int ref = refs.Get(refI);
                    f.SetValue2(coo(ref), 0);
                }

                for (int refI = 0; refI < refs.Max(); refI++)
                {
                    int i = refs.Get(refI);
                    if (!r.IsRef(i))
                        continue;
                    if (f.GetValue2(coo(i)) != 0)
                        continue;
                    int ref = i;
                    int depth = 1;
                    for (int tmp = 0; tmp < 1000 && r.HasParent(ref); tmp++)
                    {
                        ref = r.ParentRef(ref);
                        COORDINATE coo = coo(ref);
                        if (f.GetValue2(coo) != 0)
                        {
                            depth += f.GetValue2(coo);
                            break;
                        }

                        depth++;
                    }
                    ref = i;
                    for (int tmp = 0; tmp < 1000; tmp++)
                    {
                        COORDINATE coo = coo(ref);
                        if (f.GetValue2(coo) != 0)
                        {
                            break;
                        }

                        f.SetValue2(coo, depth);
                        depth--;
                        if (!r.HasParent(ref))
                            break;
                        ref = r.ParentRef(ref);
                    }
                }
            }

            /**
             * push all references, sorting by depth. Set value2 = their reference (order)
             */
            for (int refI = 0; refI < refs.Max(); refI++)
            {
                int i = refs.Get(refI);
                if (!r.IsRef(i))
                    continue;
                f.PushSloppy(coo(i), f.GetValue2(coo(i)));
                f.SetValue2(coo(i), i);
            }

            if (!f.HasMore())
            {
                f.Done();
                return;
            }

            /**
             * per depth level, starting at the top sort all references based on their parent order. Then set the natural order of the depth level and continue to the next level.
             */
            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                int li = 0;
                tmpLevel[li] = t;
                li++;

                while (f.HasMore())
                {
                    PathTile t2 = f.PollSmallest();
                    if (t2.GetValue() != t.GetValue())
                    {
                        f.Reopen(t2);
                        f.PushSloppy(t2, t2.GetValue());
                        break;
                    }
                    tmpLevel[li] = t2;
                    li++;
                }

                Array.Sort(tmpLevel, 0, li, parentSort);

                for (int i = 0; i < li; i++)
                {
                    tmpLevel[i].SetValue2(i);
                }
            }

            /**
             * Now we have a perfect sort of nodes. Siblings are together. All parents are sorted according to their children.
             * Now we must simply push nodes to the right in order to center parents above their children.
             */
            for (int refI = 0; refI < refs.Max(); refI++)
            {
                int i = refs.Get(refI);
                if (!r.IsRef(i))
                    continue;
                COORDINATE coo = coo(i);
                PathTile t = f.Get(coo);
                int depth = (int)(t.GetValue());
                f.Reopen(t);
                double order = t.GetValue2();
                f.PushSloppy(t, depth * MAX + MAX - 1 - order);
            }

            if (!f.HasMore())
            {
                f.Done();
                return;
            }

            final PathTile start = f.PollGreatest();
            {
                PathTile current = start;
                while (f.HasMore())
                {
                    PathTile t = f.PollGreatest();
                    current.ParentSet(t);
                    current = t;
                }
            }

            redo.Set(true);
            for (int k = 0; k < maxIterations && redo.Is(); k++)
            {
                redo.Set(false);

                PathTile t = start;
                while (t != null)
                {
                    int level = (int)(t.GetValue() / MAX);
                    int li = 0;
                    tmpLevel[li] = t;
                    li++;

                    while (t.Parent != null)
                    {
                        PathTile t2 = t.Parent;
                        int level2 = (int)(t2.GetValue() / MAX);
                        if (level != level2)
                        {
                            break;
                        }
                        tmpLevel[li] = t2;
                        li++;
                        t = t2;
                    }

                    AdjustXLevel(tmpLevel, li, level, redo);
                    t = t.Parent;
                }
            }

            for (int refI = 0; refI < refs.Max(); refI++)
            {
                int i = refs.Get(refI);
                if (!r.IsRef(i))
                    continue;
                COORDINATE coo = coo(i);
                PathTile t = f.Get(coo);
                int level = (int)(t.GetValue() / MAX);
                double parentX = -1;
                int ref = t.X + t.Y * SETT.TWIDTH;
                if (r.HasParent(ref))
                {
                    parentX = GUTIL.Flooder().GetValue2(coo(r.ParentRef(ref)));
                }
                x2s[refI] = t.GetValue2();
                levels[refI] = level;
                parentXs[refI] = (float)parentX;
                //drawer.draw(ref, t.getValue2(), level, parentX);
            }

            f.Done();
        }

        private void AdjustXLevel(PathTile[] refs, int li, int level, BOOLEANImp changed)
        {
            int parent = r.ParentRef(refs[0].X + refs[0].Y * SETT.TWIDTH);

            int startI = 0;
            double startX = 0;

            for (int i = 0; i < li; i++)
            {
                PathTile t2 = refs[i];
                int parent2 = r.ParentRef(t2.X + t2.Y * SETT.TWIDTH);

                if (parent != parent2)
                {
                    startX = AdjustXSiblings(refs, startI, i, startX, level, changed);
                    startI = i;
                    parent = parent2;
                }
            }
            AdjustXSiblings(refs, startI, li, startX, level, changed);
        }

        private double AdjustXSiblings(PathTile[] refs, int startI, int endI, double lastX, int level, BOOLEANImp parentsHaveChange)
        {
            int parentRef = r.ParentRef(refs[startI].X + refs[startI].Y * SETT.TWIDTH);
            if (!r.IsRef(parentRef))
            {
                return lastX;
            }

            double parentX = 0;
            for (int i = startI; i < endI; i++)
            {
                PathTile t = refs[i];

                if (lastX > t.GetValue2())
                {
                    t.SetValue2(lastX);
                }
                parentX += t.GetValue2();
                lastX = t.GetValue2() + 1;
            }

            parentX /= endI - startI;
            parentX = (int)Math.Round(parentX * 2) / 2.0;

            double oldParent = f.GetValue2(coo(parentRef));
            if (oldParent > parentX)
            {
                double delta = oldParent - parentX;
                for (int i = startI; i < endI; i++)
                {
                    PathTile t = refs[i];
                    t.SetValue2(t.GetValue2() + delta);
                }
                lastX += delta;
                //changed.Set(true);
            }
            else if (oldParent < parentX)
            {
                PathTile p = f.Get(coo(parentRef));
                p.SetValue2(parentX);
                parentsHaveChange.Set(true);
            }
            else
            {

            }

            return lastX;
        }

        public double X2(int index)
        {
            return x2s[index];
        }

        public double Level(int index)
        {
            return levels[index];
        }

        public double ParentX(int index)
        {
            return parentXs[index];
        }
    }

    class ParentSorter : IComparer<PathTile>
    {
        public int Compare(PathTile x, PathTile y)
        {
            return x.GetValue2.CompareTo(y.GetValue2());
        }
    }
}