using System;

namespace snake2d
{
    /**
     * Will anyone ever read this? If you do you're in luck. this is the single greatest feat in programming 
     * history. Here before you is a black and white three unlike anything you've ever seen.
     * It's directly stolen from oracle, but all fuzz has been stripped out, making it 50% faster.
     * 
     * 
     * @author mail__000
     *
     */
    class RBTileTree
    {
        private PathTile root = null;
        private int size = 0;

        public RBTileTree()
        {
        }

        public int Size()
        {
            return size;
        }

        public void Put(PathTile key)
        {
            PathTile t = root;
            if (t == null)
            {
                InitTile(key, null);
                root = key;
                size = 1;
                return;
            }
            int cmp;
            PathTile parent;

            do
            {
                parent = t;
                cmp = key.CompareTo(t);
                if (cmp < 0)
                    t = t.Left;
                else if (cmp > 0)
                    t = t.Right;
                else
                {
                    throw new RuntimeException("shitstorm");
                }

            } while (t != null);

            InitTile(key, parent);
            if (cmp < 0)
                parent.Left = key;
            else
                parent.Right = key;
            FixAfterInsertion(key);
            size++;
            return;
        }

        private void InitTile(PathTile t, PathTile parent)
        {
            t.Left = null;
            t.Right = null;
            t.Parent = parent;
            t.Color = RBTileTree.BLACK;
        }

        public PathTile PollSmallest()
        {
            PathTile t = GetFirstEntry();
            DeleteEntry(t);
            return t;
        }

        public PathTile PollGreatest()
        {
            PathTile t = GetLastEntry();
            DeleteEntry(t);
            return t;
        }

        public PathTile Smallest()
        {
            return GetFirstEntry();
        }

        public PathTile Greatest()
        {
            return GetLastEntry();
        }

        public void Remove(PathTile p)
        {
            if (p == null)
                return;
            DeleteEntry(p);
        }

        public void Clear()
        {
            size = 0;
            root = null;
        }

        // Red-black mechanics

        private static readonly bool RED = false;
        private static readonly bool BLACK = true;

        /**
         * Returns the first Entry in the TreeMap (according to the TreeMap's
         * key-sort function).  Returns null if the TreeMap is empty.
         */
        private PathTile GetFirstEntry()
        {
            PathTile p = root;
            if (p != null)
                while (p.Left != null)
                    p = p.Left;
            return p;
        }

        /**
         * Returns the last Entry in the TreeMap (according to the TreeMap's
         * key-sort function).  Returns null if the TreeMap is empty.
         */

        private PathTile GetLastEntry()
        {
            PathTile p = root;
            if (p != null)
                while (p.Right != null)
                    p = p.Right;
            return p;
        }

        /**
         * Returns the successor of the specified Entry, or null if no such.
         */
        private PathTile Successor(PathTile t)
        {
            if (t == null)
                return null;
            else if (t.Right != null)
            {
                PathTile p = t.Right;
                while (p.Left != null)
                    p = p.Left;
                return p;
            }
            else
            {
                PathTile p = t.Parent;
                PathTile ch = t;
                while (p != null && ch == p.Right)
                {
                    ch = p;
                    p = p.Parent;
                }
                return p;
            }
        }

        /**
         * Balancing operations.
         *
         * Implementations of rebalancings during insertion and deletion are
         * slightly different than the CLR version.  Rather than using dummy
         * nilnodes, we use a set of accessors that deal properly with null.  They
         * are used to avoid messiness surrounding nullness checks in the main
         * algorithms.
         */

        private static bool ColorOf(PathTile p)
        {
            return (p == null ? BLACK : p.Color);
        }

        private static PathTile ParentOf(PathTile p)
        {
            return (p == null ? null : p.Parent);
        }

        private static void SetColor(PathTile p, bool c)
        {
            if (p != null)
                p.Color = c;
        }

        private static PathTile LeftOf(PathTile p)
        {
            return (p == null) ? null : p.Left;
        }

        private static PathTile RightOf(PathTile p)
        {
            return (p == null) ? null : p.Right;
        }

        /** From CLR */
        private void RotateLeft(PathTile p)
        {
            if (p != null)
            {
                PathTile r = p.Right;
                p.Right = r.Left;
                if (r.Left != null)
                    r.Left.Parent = p;
                r.Parent = p.Parent;
                if (p.Parent == null)
                    root = r;
                else if (p.Parent.Left == p)
                    p.Parent.Left = r;
                else
                    p.Parent.Right = r;
                r.Left = p;
                p.Parent = r;
            }
        }

        /** From CLR */
        private void RotateRight(PathTile p)
        {
            if (p != null)
            {
                PathTile l = p.Left;
                p.Left = l.Right;
                if (l.Right != null)
                    l.Right.Parent = p;
                l.Parent = p.Parent;
                if (p.Parent == null)
                    root = l;
                else if (p.Parent.Right == p)
                    p.Parent.Right = l;
                else
                    p.Parent.Left = l;
                l.Right = p;
                p.Parent = l;
            }
        }

        private void FixAfterInsertion(PathTile x)
        {
            x.Color = RED;
            while (x != null && x != root && x.Parent.Color == RED)
            {
                if (ParentOf(x) == LeftOf(ParentOf(ParentOf(x))))
                {
                    PathTile y = RightOf(ParentOf(ParentOf(x)));
                    if (ColorOf(y) == RED)
                    {
                        SetColor(ParentOf(x), BLACK);
                        SetColor(y, BLACK);
                        SetColor(ParentOf(ParentOf(x)), RED);
                        x = ParentOf(ParentOf(x));
                    }
                    else
                    {
                        if (x == RightOf(ParentOf(x)))
                        {
                            x = ParentOf(x);
                            RotateLeft(x);
                        }
                        SetColor(ParentOf(x), BLACK);
                        SetColor(ParentOf(ParentOf(x)), RED);
                        RotateRight(ParentOf(ParentOf(x)));
                    }
                }
                else
                {
                    PathTile y = LeftOf(ParentOf(ParentOf(x)));
                    if (ColorOf(y) == RED)
                    {
                        SetColor(ParentOf(x), BLACK);
                        SetColor(y, BLACK);
                        SetColor(ParentOf(ParentOf(x)), RED);
                        x = ParentOf(ParentOf(x));
                    }
                    else
                    {
                        if (x == LeftOf(ParentOf(x)))
                        {
                            x = ParentOf(x);
                            RotateRight(x);
                        }
                        SetColor(ParentOf(x), BLACK);
                        SetColor(ParentOf(ParentOf(x)), RED);
                        RotateLeft(ParentOf(ParentOf(x)));
                    }
                }
            }
            root.Color = BLACK;
        }

        private void FixAfterDeletion(PathTile x)
        {
            while (x != root && ColorOf(x) == BLACK)
            {
                if (x == LeftOf(ParentOf(x)))
                {
                    PathTile sib = RightOf(ParentOf(x));

                    if (ColorOf(sib) == RED)
                    {
                        SetColor(sib, BLACK);
                        SetColor(ParentOf(x), RED);
                        RotateLeft(ParentOf(x));
                        sib = RightOf(ParentOf(x));
                    }

                    if (ColorOf(LeftOf(sib)) == BLACK &&
                        ColorOf(RightOf(sib)) == BLACK)
                    {
                        SetColor(sib, RED);
                        x = ParentOf(x);
                    }
                    else
                    {
                        if (ColorOf(RightOf(sib)) == BLACK)
                        {
                            SetColor(LeftOf(sib), BLACK);
                            SetColor(sib, RED);
                            RotateRight(sib);
                            sib = RightOf(ParentOf(x));
                        }
                        SetColor(sib, ColorOf(ParentOf(x)));
                        SetColor(ParentOf(x), BLACK);
                        SetColor(RightOf(sib), BLACK);
                        RotateLeft(ParentOf(x));
                        x = root;
                    }
                }
                else
                { // symmetric
                    PathTile sib = LeftOf(ParentOf(x));

                    if (ColorOf(sib) == RED)
                    {
                        SetColor(sib, BLACK);
                        SetColor(ParentOf(x), RED);
                        RotateRight(ParentOf(x));
                        sib = LeftOf(ParentOf(x));
                    }

                    if (ColorOf(RightOf(sib)) == BLACK &&
                        ColorOf(LeftOf(sib)) == BLACK)
                    {
                        SetColor(sib, RED);
                        x = ParentOf(x);
                    }
                    else
                    {
                        if (ColorOf(LeftOf(sib)) == BLACK)
                        {
                            SetColor(RightOf(sib), BLACK);
                            SetColor(sib, RED);
                            RotateLeft(sib);
                            sib = LeftOf(ParentOf(x));
                        }
                        SetColor(sib, ColorOf(ParentOf(x)));
                        SetColor(ParentOf(x), BLACK);
                        SetColor(LeftOf(sib), BLACK);
                        RotateRight(ParentOf(x));
                        x = root;
                    }
                }
            }
            SetColor(x, BLACK);
        }

        private void DeleteEntry(PathTile p)
        {
            size--;

            // If strictly internal, copy successor's element to p and then make p point to successor.
            if (p.Left != null && p.Right != null)
            {
                PathTile s = Successor(p);
                p.Value = s.Value;
                p = s;
            }

            PathTile replacement = (p.Left != null ? p.Left : p.Right);

            if (replacement != null)
            {
                // Link replacement to parent
                replacement.Parent = p.Parent;
                if (p.Parent == null)
                    root = replacement;
                else if (p == p.Parent.Left)
                    p.Parent.Left = replacement;
                else
                    p.Parent.Right = replacement;

                // Null out links so they are OK to use by fixAfterDeletion.
                p.Left = p.Right = p.Parent = null;

                // Fix replacement
                if (p.Color == BLACK)
                    FixAfterDeletion(replacement);
            }
            else if (p.Parent == null) // return if we are the only node.
            {
                root = null;
            }
            else // No children. Use self as phantom replacement and delete parent directly.
            {
                if (p.Color == BLACK)
                    FixAfterDeletion(p);

                if (p.Parent != null)
                {
                    if (p == p.Parent.Left)
                        p.Parent.Left = null;
                    else if (p == p.Parent.Right)
                        p.Parent.Right = null;
                    p.Parent = null;
                }
            }
        }
    }
}