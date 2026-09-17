using System;

namespace snake2d.path1D
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
        private PTile1D root = null;
        private int size = 0;

        public RBTileTree()
        {
        }

        public int Size()
        {
            return size;
        }

        public void Put(PTile1D key)
        {
            PTile1D t = root;
            if (t == null)
            {
                InitTile(key, null);
                root = key;
                size = 1;
                return;
            }
            int cmp;
            PTile1D parent;

            do
            {
                parent = t;

                cmp = cmp(key, t);
                if (cmp < 0)
                    t = t.left;
                else if (cmp > 0)
                    t = t.right;
                else
                {
                    throw new RuntimeException("shitstorm");
                }

            } while (t != null);

            InitTile(key, parent);
            if (cmp < 0)
                parent.left = key;
            else
                parent.right = key;
            FixAfterInsertion(key);
            size++;
            return;
        }

        private int cmp(PTile1D key, PTile1D t)
        {
            if (key == t)
                throw new RuntimeException("shitstorm");
            if (key.value < t.value)
                return -1;
            return 1;

        }

        private void InitTile(PTile1D t, PTile1D parent)
        {
            t.left = null;
            t.right = null;
            t.parent = parent;
            t.color = RBTileTree.BLACK;
        }

        public PTile1D PollSmallest()
        {
            PTile1D t = GetFirstEntry();
            DeleteEntry(t);
            return t;
        }

        public PTile1D PollGreatest()
        {
            PTile1D t = GetLastEntry();
            DeleteEntry(t);
            return t;
        }

        public PTile1D Smallest()
        {
            return GetFirstEntry();
        }

        public PTile1D Greatest()
        {
            return GetLastEntry();
        }

        public void Remove(PTile1D p)
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
        private PTile1D GetFirstEntry()
        {
            PTile1D p = root;
            if (p != null)
                while (p.left != null)
                    p = p.left;
            return p;
        }

        /**
         * Returns the last Entry in the TreeMap (according to the TreeMap's
         * key-sort function).  Returns null if the TreeMap is empty.
         */

        private PTile1D GetLastEntry()
        {
            PTile1D p = root;
            if (p != null)
                while (p.right != null)
                    p = p.right;
            return p;
        }

        /**
         * Returns the successor of the specified Entry, or null if no such.
         */
        private PTile1D Successor(PTile1D t)
        {
            if (t == null)
                return null;
            else if (t.right != null)
            {
                PTile1D p = t.right;
                while (p.left != null)
                    p = p.left;
                return p;
            }
            else
            {
                PTile1D p = t.parent;
                PTile1D ch = t;
                while (p != null && ch == p.right)
                {
                    ch = p;
                    p = p.parent;
                }
                return p;
            }
        }

        /**
         * Balancing operations.
         *
         * Implementations of fixAfterInsertion and fixAfterDeletion are slightly
         * different than the CLR version. Rather than using dummy nilnodes, we
         * use null to represent null links. The advantage is a cleaner
         * implementation; the disadvantage is that we cannot always assert
         * that non-null links are non-red and non-null. However, the
         * resulting code appears to perform as well as the CLR version.
         */

        /**
         * Restore red-black properties following insert of the given node.
         */
        private void FixAfterInsertion(PTile1D x)
        {
            x.color = RED;

            while (x != null && x != root && x.parent.color == RED)
            {
                if (ParentOf(x) == LeftOf(ParentOf(ParentOf(x))))
                {
                    PTile1D y = RightOf(ParentOf(ParentOf(x)));
                    if (ColorOf(y) == RED)
                    {
                        setColor(ParentOf(x), BLACK);
                        setColor(y, BLACK);
                        setColor(ParentOf(ParentOf(x)), RED);
                        x = ParentOf(ParentOf(x));
                    }
                    else
                    {
                        if (x == RightOf(ParentOf(x)))
                        {
                            x = ParentOf(x);
                            RotateLeft(x);
                        }
                        setColor(ParentOf(x), BLACK);
                        setColor(ParentOf(ParentOf(x)), RED);
                        RotateRight(ParentOf(ParentOf(x)));
                    }
                }
                else
                {
                    PTile1D y = LeftOf(ParentOf(ParentOf(x)));
                    if (ColorOf(y) == RED)
                    {
                        setColor(ParentOf(x), BLACK);
                        setColor(y, BLACK);
                        setColor(ParentOf(ParentOf(x)), RED);
                        x = ParentOf(ParentOf(x));
                    }
                    else
                    {
                        if (x == LeftOf(ParentOf(x)))
                        {
                            x = ParentOf(x);
                            RotateRight(x);
                        }
                        setColor(ParentOf(x), BLACK);
                        setColor(ParentOf(ParentOf(x)), RED);
                        RotateLeft(ParentOf(ParentOf(x)));
                    }
                }
            }
            setColor(root, BLACK);
        }

        /**
         * Delete node p, and then rebalance the tree.
         */
        private void DeleteEntry(PTile1D p)
        {
            size--;

            // If strictly internal, copy successor's element to p and then make p
            // point to successor.
            if (p.left != null && p.right != null)
            {
                PTile1D s = Successor(p);
                p.value = s.value;
                p = s;
            }

            // Start fixup at replacement node, if it exists.
            PTile1D replacement = (p.left != null ? p.left : p.right);

            if (replacement != null)
            {
                // Link replacement to parent
                replacement.parent = p.parent;
                if (p.parent == null)
                    root = replacement;
                else if (p == p.parent.left)
                    p.parent.left = replacement;
                else
                    p.parent.right = replacement;

                // Null out links so they are OK to use by fixAfterDeletion.
                p.left = p.right = p.parent = null;

                // Fix replacement
                if (p.color == BLACK)
                    FixAfterDeletion(replacement);
            }
            else if (p.parent == null) // return if we are the only node.
                root = null;
            else // No children. Use self as phantom replacement and unlink.
            {
                if (p.color == BLACK)
                    FixAfterDeletion(p);

                if (p.parent != null)
                {
                    if (p == p.parent.left)
                        p.parent.left = null;
                    else if (p == p.parent.right)
                        p.parent.right = null;
                    p.parent = null;
                }
            }
        }

        private void SwitchLoc(PTile1D a, PTile1D b)
        {
            // color
            bool ac = a.color;
            a.color = b.color;
            b.color = ac;

            if (a.parent == b && b != null)
            {
                PTile1D lc = a.left;
                PTile1D rc = a.right;

                if (b.left == a)
                {
                    a.left = b;
                    a.right = b.right;
                    if (a.right != null)
                        a.right.parent = a;
                }
                else
                {
                    a.right = b;
                    a.left = b.left;
                    if (a.left != null)
                        a.left.parent = a;
                }

                a.parent = b.parent;

                if (a.parent != null)
                {
                    if (a.parent.left == b)
                        a.parent.left = a;
                    else
                        a.parent.right = a;
                }

                b.parent = a;

                b.left = lc;
                if (b.left != null)
                    b.left.parent = b;
                b.right = rc;
                if (b.right != null)
                    b.right.parent = b;
            }
            else if (b.parent == a && b != null)
            {
                throw new RuntimeException("should not happen!");
            }
            else
            {
                // parent
                if (b.parent != null)
                {
                    if (b.parent.left == b)
                        b.parent.left = a;
                    else
                        b.parent.right = a;
                }
                if (a.parent != null)
                {
                    if (a.parent.left == a)
                        a.parent.left = b;
                    else
                        a.parent.right = b;
                }

                PTile1D ap = a.parent;
                a.parent = b.parent;
                b.parent = ap;

                // children's parents
                if (a.left != null)
                    a.left.parent = b;
                if (a.right != null)
                    a.right.parent = b;
                if (b.left != null)
                    b.left.parent = a;
                if (b.right != null)
                    b.right.parent = a;

                // children
                PTile1D al = a.left;
                PTile1D ar = a.right;
                a.left = b.left;
                a.right = b.right;
                b.left = al;
                b.right = ar;
            }

            if (a == root)
            {
                root = b;
            }
            else if (b == root)
            {
                root = a;
            }
        }

        /**
         * Restore red-black properties following a deletion.
         */
        private void FixAfterDeletion(PTile1D x)
        {
            while (x != root && ColorOf(x) == BLACK)
            {
                if (x == LeftOf(ParentOf(x)))
                {
                    PTile1D sib = RightOf(ParentOf(x));

                    if (ColorOf(sib) == RED)
                    {
                        setColor(sib, BLACK);
                        setColor(ParentOf(x), RED);
                        RotateLeft(ParentOf(x));
                        sib = RightOf(ParentOf(x));
                    }

                    if (ColorOf(LeftOf(sib)) == BLACK &&
                        ColorOf(RightOf(sib)) == BLACK)
                    {
                        setColor(sib, RED);
                        x = ParentOf(x);
                    }
                    else
                    {
                        if (ColorOf(RightOf(sib)) == BLACK)
                        {
                            setColor(LeftOf(sib), BLACK);
                            setColor(sib, RED);
                            RotateRight(sib);
                            sib = RightOf(ParentOf(x));
                        }
                        setColor(sib, ColorOf(ParentOf(x)));
                        setColor(ParentOf(x), BLACK);
                        setColor(RightOf(sib), BLACK);
                        RotateLeft(ParentOf(x));
                        x = root;
                    }
                }
                else
                { // symmetric
                    PTile1D sib = LeftOf(ParentOf(x));

                    if (ColorOf(sib) == RED)
                    {
                        setColor(sib, BLACK);
                        setColor(ParentOf(x), RED);
                        RotateRight(ParentOf(x));
                        sib = LeftOf(ParentOf(x));
                    }

                    if (ColorOf(RightOf(sib)) == BLACK &&
                        ColorOf(LeftOf(sib)) == BLACK)
                    {
                        setColor(sib, RED);
                        x = ParentOf(x);
                    }
                    else
                    {
                        if (ColorOf(LeftOf(sib)) == BLACK)
                        {
                            setColor(RightOf(sib), BLACK);
                            setColor(sib, RED);
                            RotateLeft(sib);
                            sib = LeftOf(ParentOf(x));
                        }
                        setColor(sib, ColorOf(ParentOf(x)));
                        setColor(ParentOf(x), BLACK);
                        setColor(LeftOf(sib), BLACK);
                        RotateRight(ParentOf(x));
                        x = root;
                    }
                }
            }

            setColor(x, BLACK);
        }

        private static PTile1D ParentOf(PTile1D p)
        {
            return (p == null ? null : p.parent);
        }

        private static PTile1D LeftOf(PTile1D p)
        {
            return (p == null ? null : p.left);
        }

        private static PTile1D RightOf(PTile1D p)
        {
            return (p == null ? null : p.right);
        }

        private static bool ColorOf(PTile1D p)
        {
            return (p == null ? BLACK : p.color);
        }

        private static void setColor(PTile1D p, bool c)
        {
            if (p != null)
                p.color = c;
        }

        private void RotateLeft(PTile1D p)
        {
            if (p != null)
            {
                PTile1D r = p.right;
                p.right = r.left;
                if (r.left != null)
                    r.left.parent = p;
                r.parent = p.parent;
                if (p.parent == null)
                    root = r;
                else if (p.parent.left == p)
                    p.parent.left = r;
                else
                    p.parent.right = r;
                r.left = p;
                p.parent = r;
            }
        }

        private void RotateRight(PTile1D p)
        {
            if (p != null)
            {
                PTile1D l = p.left;
                p.left = l.right;
                if (l.right != null)
                    l.right.parent = p;
                l.parent = p.parent;
                if (p.parent == null)
                    root = l;
                else if (p.parent.right == p)
                    p.parent.right = l;
                else
                    p.parent.left = l;
                l.right = p;
                p.parent = l;
            }
        }
    }