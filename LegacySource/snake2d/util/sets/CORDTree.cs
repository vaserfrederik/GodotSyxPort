using System;
using System.IO;

namespace snake2d.util.sets
{
    [Serializable]
    public class CORDTree : ISerializable
    {
        private static readonly long serialVersionUID = 1L;
        private CORD root = null;
        private int size = 0;

        public CORDTree()
        {
        }

        public int Size()
        {
            return size;
        }

        public void Put(CORD key, double value)
        {
            key.value = (float)value;
            CORD t = root;
            if (t == null)
            {
                InitTile(key, null);
                root = key;
                size = 1;
                return;
            }

            CORD parent;

            do
            {
                parent = t;

                if (key == t)
                    throw new RuntimeException();

                if (key.value < t.value)
                    t = t.left;
                else
                    t = t.right;

            } while (t != null);

            InitTile(key, parent);
            if (key.value < parent.value)
                parent.left = key;
            else
                parent.right = key;
            FixAfterInsertion(key);
            size++;
            return;
        }

        private void InitTile(CORD t, CORD parent)
        {
            t.left = null;
            t.right = null;
            t.parent = parent;
            t.color = CORDTree.BLACK;
        }

        public CORD PollSmallest()
        {
            CORD t = GetFirstEntry();
            DeleteEntry(t);
            return t;
        }

        public CORD PollGreatest()
        {
            CORD t = GetLastEntry();
            DeleteEntry(t);
            return t;
        }

        public CORD Smallest()
        {
            return GetFirstEntry();
        }

        public CORD Greatest()
        {
            return GetLastEntry();
        }

        public void Remove(CORD p)
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
        private CORD GetFirstEntry()
        {
            CORD p = root;
            if (p != null)
                while (p.left != null)
                    p = p.left;
            return p;
        }

        /**
         * Returns the last Entry in the TreeMap (according to the TreeMap's
         * key-sort function).  Returns null if the TreeMap is empty.
         */
        private CORD GetLastEntry()
        {
            CORD p = root;
            if (p != null)
                while (p.right != null)
                    p = p.right;
            return p;
        }

        /**
         * Returns the successor of the specified Entry, or null if no such.
         */
        private CORD Successor(CORD t)
        {
            if (t == null)
                return null;
            else if (t.right != null)
            {
                CORD p = t.right;
                while (p.left != null)
                    p = p.left;
                return p;
            }
            else
            {
                CORD p = t.parent;
                CORD ch = t;
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
         * Implementations of rebalancings during insertion and deletion are
         * slightly different than the CLR version.  Rather than using dummy
         * nilnodes, we use a set of accessors that deal properly with null.  They
         * are used to avoid messiness surrounding nullness checks in the main
         * algorithms.
         */

        private static bool ColorOf(CORD p)
        {
            return (p == null ? BLACK : p.color);
        }

        private static CORD ParentOf(CORD p)
        {
            return (p == null ? null : p.parent);
        }

        private static void SetColor(CORD p, bool c)
        {
            if (p != null)
                p.color = c;
        }

        private void RotateLeft(CORD p)
        {
            CORD r = p.right;
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

        private void RotateRight(CORD p)
        {
            CORD l = p.left;
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

        private void FixAfterInsertion(CORD x)
        {
            x.color = RED;
            while (x != null && x != root && ParentOf(x).color == RED)
            {
                if (ParentOf(x) == LeftOf(ParentOf(ParentOf(x))))
                {
                    CORD y = RightOf(ParentOf(ParentOf(x)));
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
                    CORD y = LeftOf(ParentOf(ParentOf(x)));
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
            SetColor(root, BLACK);
        }

        private void FixAfterDeletion(CORD x)
        {
            while (x != root && ColorOf(x) == BLACK)
            {
                if (x == LeftOf(ParentOf(x)))
                {
                    CORD sib = RightOf(ParentOf(x));

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
                {
                    CORD sib = LeftOf(ParentOf(x));

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

        private static CORD LeftOf(CORD p)
        {
            return (p == null ? null : p.left);
        }

        private static CORD RightOf(CORD p)
        {
            return (p == null ? null : p.right);
        }

        private void DeleteEntry(CORD p)
        {
            --size;

            if (p.left != null && p.right != null)
            {
                CORD s = GetSuccessor(p);
                SwapData(p, s);
                p = s;
            }

            CORD replacement = p.left != null ? p.left : p.right;

            if (replacement != null)
            {
                replacement.parent = p.parent;
                if (p.parent == null)
                    root = replacement;
                else if (p == p.parent.left)
                    p.parent.left = replacement;
                else
                    p.parent.right = replacement;

                if (p.color == BLACK)
                    FixAfterDeletion(replacement);
            }
            else if (p.parent == null)
                root = null;
            else
            {
                if (p.color == BLACK)
                    FixAfterDeletion(p);

                if (p.parent != null)
                {
                    if (p == p.parent.left)
                        p.parent.left = null;
                    else if (p == p.parent.right)
                        p.parent.right = null;
                }

                p.parent = null;
            }
        }

        private void SwapData(CORD p, CORD s)
        {
            short x = p.x;
            short y = p.y;
            float value = p.value;

            p.x = s.x;
            p.y = s.y;
            p.value = s.value;

            s.x = x;
            s.y = y;
            s.value = value;
        }

        private CORD GetSuccessor(CORD p)
        {
            if (p.right != null)
            {
                CORD s = p.right;
                while (s.left != null)
                    s = s.left;
                return s;
            }
            else
            {
                CORD s = p.parent;
                while (s != null && p == s.right)
                {
                    p = s;
                    s = s.parent;
                }
                return s;
            }
        }

        public static class CORD : COORDINATE, ISerializable
        {
            private static readonly long serialVersionUID = 1L;
            private short x; //2
            private short y; //2
            internal float value; //4

            internal CORD left; //4
            internal CORD right; //4
            internal CORD parent; //4
            internal bool color; //1

            public CORD()
            {
            }

            public int X()
            {
                return x;
            }

            public int Y()
            {
                return y;
            }

            public CORD Set(int x, int y)
            {
                this.x = (short)x;
                this.y = (short)y;
                return this;
            }

            public double Value()
            {
                return value;
            }
        }

        // ISerializable implementation
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("root", root, typeof(CORD));
            info.AddValue("size", size, typeof(int));
        }

        protected CORDTree(SerializationInfo info, StreamingContext context)
        {
            root = (CORD)info.GetValue("root", typeof(CORD));
            size = info.GetInt32("size");
        }
    }
}