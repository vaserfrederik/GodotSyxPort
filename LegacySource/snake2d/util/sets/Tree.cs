using System;

namespace snake2d.util.sets
{
    /**
     * Will anyone ever read this? If you do you're in luck. this is the single
     * greatest feat in programming history. Here before you is a black and white
     * three unlike anything you've ever seen. It's directly stolen from oracle, but
     * all fuzz has been stripped out, making it 50% faster.
     * 
     * 
     * @author mail__000
     *
     */
    public abstract class Tree<T> : ADDABLE<T>
    {
        private readonly Nodes nodes;
        private Node root = null;

        public Tree(int size)
        {
            nodes = new Nodes(size);
        }

        public int size()
        {
            return nodes.current;
        }

        public override int tryAdd(T e)
        {
            if (nodes.current >= nodes.nodes.Length)
                return -1;
            return add(e);
        }

        public override bool hasRoom()
        {
            if (nodes.current >= nodes.nodes.Length)
                return false;
            return true;
        }

        public int add(T element)
        {
            if (nodes.current >= nodes.nodes.Length)
                throw new Exception("full");
            Node key = nodes.getNext();
            key.element = element;

            Node t = root;
            if (t == null)
            {
                initTile(key, null);
                root = key;
                return 1;
            }
            bool greater;
            Node parent;

            do
            {
                parent = t;
                greater = isGreaterThan(element, (T)t.element);

                if (!greater)
                    t = t.left;
                else
                    t = t.right;

            } while (t != null);

            initTile(key, parent);
            if (!greater)
                parent.left = key;
            else
                parent.right = key;
            fixAfterInsertion(key);
            return 1;
        }

        public bool contains(T element)
        {
            Node t = root;
            if (t == null)
            {
                return false;
            }
            bool greater;
            Node parent;

            do
            {
                parent = t;
                if (parent.element.Equals(element))
                    return true;
                greater = isGreaterThan(element, (T)t.element);

                if (!greater)
                    t = t.left;
                else
                    t = t.right;

            } while (t != null);

            return false;
        }

        public bool removeElement(T element)
        {
            Node t = root;
            if (t == null)
            {
                return false;
            }
            bool greater;
            Node parent;

            do
            {
                parent = t;
                greater = isGreaterThan(element, (T)t.element);

                if (!greater)
                    t = t.left;
                else
                    t = t.right;

            } while (t != null && !t.element.Equals(element));

            if (t == null)
                return false;

            deleteNode(t);
            return true;
        }

        private void deleteNode(Node t)
        {
            if (t.left == null && t.right == null)
            {
                // No children, just remove the node
                if (t.parent != null)
                {
                    if (t == t.parent.left)
                        t.parent.left = null;
                    else
                        t.parent.right = null;
                }
                else
                {
                    root = null;
                }
                nodes.returnNode(t);
            }
            else if (t.left != null && t.right != null)
            {
                // Two children, replace with successor
                Node successor = getSuccessor(t);
                switchLoc(successor, t);
                deleteNode(successor); // Recursively delete the successor
            }
            else
            {
                // One child, replace with child
                Node child = t.left != null ? t.left : t.right;
                if (t.parent != null)
                {
                    if (t == t.parent.left)
                        t.parent.left = child;
                    else
                        t.parent.right = child;
                }
                else
                {
                    root = child;
                }
                child.parent = t.parent;
                nodes.returnNode(t);
            }
        }

        private Node getSuccessor(Node node)
        {
            Node current = node.right;
            while (current.left != null)
            {
                current = current.left;
            }
            return current;
        }

        private void initTile(Node key, Node parent)
        {
            key.left = null;
            key.right = null;
            key.parent = parent;
            key.color = false; // false for black, true for red
        }

        private void switchLoc(Node a, Node b)
        {
            // color
            bool ac = a.color;
            a.color = b.color;
            b.color = ac;

            if (a.parent == b && b != null)
            {
                Node lc = a.left;
                Node rc = a.right;

                if (b.left == a)
                {
                    a.left = b;
                    a.right = b.right;
                    if (b.right != null) b.right.parent = a;
                }
                else
                {
                    a.right = b;
                    a.left = b.left;
                    if (b.left != null) b.left.parent = a;
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
                if (lc != null) lc.parent = b;
                b.right = rc;
                if (rc != null) rc.parent = b;
            }
            else if (b.parent == a && b != null)
            {
                throw new Exception("should not happen!");
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

                Node ap = a.parent;
                a.parent = b.parent;
                b.parent = ap;

                // childrens parents
                if (a.left != null) a.left.parent = b;
                if (a.right != null) a.right.parent = b;
                if (b.left != null) b.left.parent = a;
                if (b.right != null) b.right.parent = a;

                // children
                Node al = a.left;
                Node ar = a.right;
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

        private void fixAfterInsertion(Node x)
        {
            x.color = true; // Set new node to red

            while (x != null && x != root && x.parent.color)
            {
                if (parentOf(x) == leftOf(parentOf(parentOf(x))))
                {
                    Node y = rightOf(parentOf(parentOf(x)));
                    if (colorOf(y))
                    {
                        setColor(parentOf(x), false);
                        setColor(y, false);
                        setColor(parentOf(parentOf(x)), true);
                        x = parentOf(parentOf(x));
                    }
                    else
                    {
                        if (x == rightOf(parentOf(x)))
                        {
                            x = parentOf(x);
                            rotateLeft(x);
                        }
                        setColor(parentOf(x), false);
                        setColor(parentOf(parentOf(x)), true);
                        rotateRight(parentOf(parentOf(x)));
                    }
                }
                else
                {
                    Node y = leftOf(parentOf(parentOf(x)));
                    if (colorOf(y))
                    {
                        setColor(parentOf(x), false);
                        setColor(y, false);
                        setColor(parentOf(parentOf(x)), true);
                        x = parentOf(parentOf(x));
                    }
                    else
                    {
                        if (x == leftOf(parentOf(x)))
                        {
                            x = parentOf(x);
                            rotateRight(x);
                        }
                        setColor(parentOf(x), false);
                        setColor(parentOf(parentOf(x)), true);
                        rotateLeft(parentOf(parentOf(x)));
                    }
                }
            }
            root.color = false;
        }

        private void fixAfterDeletion(Node x)
        {
            while (x != root && !colorOf(x))
            {
                if (x == leftOf(parentOf(x)))
                {
                    Node sib = rightOf(parentOf(x));

                    if (colorOf(sib))
                    {
                        setColor(sib, false);
                        setColor(parentOf(x), true);
                        rotateLeft(parentOf(x));
                        sib = rightOf(parentOf(x));
                    }

                    if (!colorOf(leftOf(sib)) && !colorOf(rightOf(sib)))
                    {
                        setColor(sib, true);
                        x = parentOf(x);
                    }
                    else
                    {
                        if (!colorOf(rightOf(sib)))
                        {
                            setColor(leftOf(sib), false);
                            setColor(sib, true);
                            rotateRight(sib);
                            sib = rightOf(parentOf(x));
                        }
                        setColor(sib, colorOf(parentOf(x)));
                        setColor(parentOf(x), false);
                        setColor(rightOf(sib), false);
                        rotateLeft(parentOf(x));
                        x = root;
                    }
                }
                else
                {
                    Node sib = leftOf(parentOf(x));

                    if (colorOf(sib))
                    {
                        setColor(sib, false);
                        setColor(parentOf(x), true);
                        rotateRight(parentOf(x));
                        sib = leftOf(parentOf(x));
                    }

                    if (!colorOf(rightOf(sib)) && !colorOf(leftOf(sib)))
                    {
                        setColor(sib, true);
                        x = parentOf(x);
                    }
                    else
                    {
                        if (!colorOf(leftOf(sib)))
                        {
                            setColor(rightOf(sib), false);
                            setColor(sib, true);
                            rotateLeft(sib);
                            sib = leftOf(parentOf(x));
                        }
                        setColor(sib, colorOf(parentOf(x)));
                        setColor(parentOf(x), false);
                        setColor(leftOf(sib), false);
                        rotateRight(parentOf(x));
                        x = root;
                    }
                }
            }
            setColor(x, false);
        }

        private void rotateLeft(Node x)
        {
            if (x != null)
            {
                Node y = x.right;
                x.right = y.left;
                if (y.left != null) y.left.parent = x;
                y.parent = x.parent;
                if (x.parent == null)
                {
                    root = y;
                }
                else if (x == leftOf(x.parent))
                {
                    x.parent.left = y;
                }
                else
                {
                    x.parent.right = y;
                }
                y.left = x;
                x.parent = y;
            }
        }

        private void rotateRight(Node x)
        {
            if (x != null)
            {
                Node y = x.left;
                x.left = y.right;
                if (y.right != null) y.right.parent = x;
                y.parent = x.parent;
                if (x.parent == null)
                {
                    root = y;
                }
                else if (x == rightOf(x.parent))
                {
                    x.parent.right = y;
                }
                else
                {
                    x.parent.left = y;
                }
                y.right = x;
                x.parent = y;
            }
        }

        private Node parentOf(Node n)
        {
            return n != null ? n.parent : null;
        }

        private Node leftOf(Node n)
        {
            return n != null ? n.left : null;
        }

        private Node rightOf(Node n)
        {
            return n != null ? n.right : null;
        }

        private bool colorOf(Node n)
        {
            return n == null ? false : n.color;
        }

        private void setColor(Node n, bool color)
        {
            if (n != null) n.color = color;
        }

        private static class Node
        {
            private int index;
            private Node left;
            private Node right;
            private Node parent;
            private bool color; // false for black, true for red
            public object element;

            public Node(object element)
            {
                this.element = element;
            }
        }

        private static class Nodes
        {
            private Node[] array;
            private int size;

            public Nodes(int capacity)
            {
                array = new Node[capacity];
                size = 0;
            }

            public void add(Node node)
            {
                if (size < array.Length)
                {
                    array[size++] = node;
                }
                else
                {
                    // Resize if necessary
                    Node[] newArray = new Node[array.Length * 2];
                    Array.Copy(array, newArray, size);
                    array = newArray;
                    add(node);
                }
            }

            public Node get(int index)
            {
                if (index >= 0 && index < size)
                {
                    return array[index];
                }
                else
                {
                    throw new IndexOutOfRangeException("Index out of range");
                }
            }

            public void remove(int index)
            {
                if (index >= 0 && index < size)
                {
                    for (int i = index; i < size - 1; i++)
                    {
                        array[i] = array[i + 1];
                    }
                    array[--size] = null;
                }
                else
                {
                    throw new IndexOutOfRangeException("Index out of range");
                }
            }
        }

        private static class Tree
        {
            private Node root;

            public Tree()
            {
                root = null;
            }

            public void add(object element)
            {
                Node newNode = new Node(element);
                if (root == null)
                {
                    root = newNode;
                    root.color = false; // Root should always be black
                }
                else
                {
                    insert(newNode, root);
                    fixAfterInsertion(newNode);
                }
            }

            private void insert(Node z, Node x)
            {
                while (x != null)
                {
                    z.parent = x;
                    if (z.element.CompareTo(x.element) < 0)
                    {
                        x = x.left;
                    }
                    else
                    {
                        x = x.right;
                    }
                }

                z.parent = x;
                if (z.parent == null)
                {
                    root = z;
                }
                else if (z.element.CompareTo(z.parent.element) < 0)
                {
                    z.parent.left = z;
                }
                else
                {
                    z.parent.right = z;
                }
            }

            public bool remove(object element)
            {
                Node x = root;
                while (x != null && !x.element.Equals(element))
                {
                    if (x.element.CompareTo(element) < 0)
                    {
                        x = x.right;
                    }
                    else
                    {
                        x = x.left;
                    }
                }

                if (x == null) return false;

                deleteNode(x);
                return true;
            }

            private void deleteNode(Node z)
            {
                Node y;
                Node x;

                if (z.left == null || z.right == null)
                {
                    y = z;
                }
                else
                {
                    y = getSuccessor(z);
                }

                x = (y.left != null) ? y.left : y.right;

                if (x != null)
                {
                    x.parent = y.parent;
                }

                if (y.parent == null)
                {
                    root = x;
                }
                else if (y == y.parent.left)
                {
                    y.parent.left = x;
                }
                else
                {
                    y.parent.right = x;
                }

                if (y != z)
                {
                    z.element = y.element;
                }

                if (!y.color)
                {
                    fixAfterDeletion(x);
                }
            }

            private Node getSuccessor(Node node)
            {
                Node current = node.right;
                while (current.left != null)
                {
                    current = current.left;
                }
                return current;
            }

            private void fixAfterDeletion(Node x)
            {
                while (x != root && !colorOf(x))
                {
                    if (x == leftOf(parentOf(x)))
                    {
                        Node w = rightOf(parentOf(x));

                        if (colorOf(w))
                        {
                            setColor(w, false);
                            setColor(parentOf(x), true);
                            rotateLeft(parentOf(x));
                            w = rightOf(parentOf(x));
                        }

                        if (!colorOf(leftOf(w)) && !colorOf(rightOf(w)))
                        {
                            setColor(w, true);
                            x = parentOf(x);
                        }
                        else
                        {
                            if (!colorOf(rightOf(w)))
                            {
                                setColor(leftOf(w), false);
                                setColor(w, true);
                                rotateRight(w);
                                w = rightOf(parentOf(x));
                            }
                            setColor(w, colorOf(parentOf(x)));
                            setColor(parentOf(x), false);
                            setColor(rightOf(w), false);
                            rotateLeft(parentOf(x));
                            x = root;
                        }
                    }
                    else
                    {
                        Node w = leftOf(parentOf(x));

                        if (colorOf(w))
                        {
                            setColor(w, false);
                            setColor(parentOf(x), true);
                            rotateRight(parentOf(x));
                            w = leftOf(parentOf(x));
                        }

                        if (!colorOf(rightOf(w)) && !colorOf(leftOf(w)))
                        {
                            setColor(w, true);
                            x = parentOf(x);
                        }
                        else
                        {
                            if (!colorOf(leftOf(w)))
                            {
                                setColor(rightOf(w), false);
                                setColor(w, true);
                                rotateLeft(w);
                                w = leftOf(parentOf(x));
                            }
                            setColor(w, colorOf(parentOf(x)));
                            setColor(parentOf(x), false);
                            setColor(leftOf(w), false);
                            rotateRight(parentOf(x));
                            x = root;
                        }
                    }
                }
                setColor(x, false);
            }

            private void rotateLeft(Node x)
            {
                if (x != null)
                {
                    Node y = x.right;
                    x.right = y.left;
                    if (y.left != null) y.left.parent = x;
                    y.parent = x.parent;
                    if (x.parent == null)
                    {
                        root = y;
                    }
                    else if (x == leftOf(x.parent))
                    {
                        x.parent.left = y;
                    }
                    else
                    {
                        x.parent.right = y;
                    }
                    y.left = x;
                    x.parent = y;
                }
            }

            private void rotateRight(Node x)
            {
                if (x != null)
                {
                    Node y = x.left;
                    x.left = y.right;
                    if (y.right != null) y.right.parent = x;
                    y.parent = x.parent;
                    if (x.parent == null)
                    {
                        root = y;
                    }
                    else if (x == rightOf(x.parent))
                    {
                        x.parent.right = y;
                    }
                    else
                    {
                        x.parent.left = y;
                    }
                    y.right = x;
                    x.parent = y;
                }
            }

            private Node parentOf(Node n)
            {
                return n != null ? n.parent : null;
            }

            private Node leftOf(Node n)
            {
                return n != null ? n.left : null;
            }

            private Node rightOf(Node n)
            {
                return n != null ? n.right : null;
            }

            private bool colorOf(Node n)
            {
                return n == null ? false : n.color;
            }

            private void setColor(Node n, bool color)
            {
                if (n != null) n.color = color;
            }

            public Node find(object element)
            {
                Node x = root;
                while (x != null && !x.element.Equals(element))
                {
                    if (x.element.CompareTo(element) < 0)
                    {
                        x = x.right;
                    }
                    else
                    {
                        x = x.left;
                    }
                }
                return x;
            }
        }

        private static class RedBlackTree
        {
            private Node root;

            public RedBlackTree()
            {
                root = null;
            }

            public void add(object element)
            {
                Node newNode = new Node(element);
                if (root == null)
                {
                    root = newNode;
                    root.color = false; // Root should always be black
                }
                else
                {
                    insert(newNode, root);
                    fixAfterInsertion(newNode);
                }
            }

            private void insert(Node z, Node x)
            {
                while (x != null)
                {
                    z.parent = x;
                    if (z.element.CompareTo(x.element) < 0)
                    {
                        x = x.left;
                    }
                    else
                    {
                        x = x.right;
                    }
                }

                z.parent = x;
                if (z.parent == null)
                {
                    root = z;
                }
                else if (z.element.CompareTo(z.parent.element) < 0)
                {
                    z.parent.left = z;
                }
                else
                {
                    z.parent.right = z;
                }

                fixAfterInsertion(z);
            }

            private void fixAfterInsertion(Node x)
            {
                x.color = true; // Set new node to red

                while (x != null && x != root && x.parent.color)
                {
                    if (parentOf(x) == leftOf(parentOf(parentOf(x))))
                    {
                        Node y = rightOf(parentOf(parentOf(x)));
                        if (colorOf(y))
                        {
                            setColor(parentOf(x), false);
                            setColor(y, false);
                            setColor(parentOf(parentOf(x)), true);
                            x = parentOf(parentOf(x));
                        }
                        else
                        {
                            if (x == rightOf(parentOf(x)))
                            {
                                x = parentOf(x);
                                rotateLeft(x);
                            }
                            setColor(parentOf(x), false);
                            setColor(parentOf(parentOf(x)), true);
                            rotateRight(parentOf(parentOf(x)));
                        }
                    }
                    else
                    {
                        Node y = leftOf(parentOf(parentOf(x)));
                        if (colorOf(y))
                        {
                            setColor(parentOf(x), false);
                            setColor(y, false);
                            setColor(parentOf(parentOf(x)), true);
                            x = parentOf(parentOf(x));
                        }
                        else
                        {
                            if (x == leftOf(parentOf(x)))
                            {
                                x = parentOf(x);
                                rotateRight(x);
                            }
                            setColor(parentOf(x), false);
                            setColor(parentOf(parentOf(x)), true);
                            rotateLeft(parentOf(parentOf(x)));
                        }
                    }
                }

                root.color = false; // Root should always be black
            }

            public bool remove(object element)
            {
                Node x = root;
                while (x != null && !x.element.Equals(element))
                {
                    if (x.element.CompareTo(element) < 0)
                    {
                        x = x.right;
                    }
                    else
                    {
                        x = x.left;
                    }
                }

                if (x == null) return false;

                deleteNode(x);
                return true;
            }

            private void deleteNode(Node z)
            {
                Node y;
                Node x;

                if (z.left == null || z.right == null)
                {
                    y = z;
                }
                else
                {
                    y = getSuccessor(z);
                }

                x = (y.left != null) ? y.left : y.right;

                if (x != null)
                {
                    x.parent = y.parent;
                }

                if (y.parent == null)
                {
                    root = x;
                }
                else if (y == y.parent.left)
                {
                    y.parent.left = x;
                }
                else
                {
                    y.parent.right = x;
                }

                if (y != z)
                {
                    z.element = y.element;
                }

                if (!y.color)
                {
                    fixAfterDeletion(x);
                }
            }

            private void fixAfterDeletion(Node x)
            {
                while (x != root && !colorOf(x))
                {
                    if (x == leftOf(parentOf(x)))
                    {
                        Node w = rightOf(parentOf(x));

                        if (colorOf(w))
                        {
                            setColor(w, false);
                            setColor(parentOf(x), true);
                            rotateLeft(parentOf(x));
                            w = rightOf(parentOf(x));
                        }

                        if (!colorOf(leftOf(w)) && !colorOf(rightOf(w)))
                        {
                            setColor(w, true);
                            x = parentOf(x);
                        }
                        else
                        {
                            if (!colorOf(rightOf(w)))
                            {
                                setColor(leftOf(w), false);
                                setColor(w, true);
                                rotateRight(w);
                                w = rightOf(parentOf(x));
                            }
                            setColor(w, colorOf(parentOf(x)));
                            setColor(parentOf(x), false);
                            setColor(rightOf(w), false);
                            rotateLeft(parentOf(x));
                            x = root;
                        }
                    }
                    else
                    {
                        Node w = leftOf(parentOf(x));

                        if (colorOf(w))
                        {
                            setColor(w, false);
                            setColor(parentOf(x), true);
                            rotateRight(parentOf(x));
                            w = leftOf(parentOf(x));
                        }

                        if (!colorOf(rightOf(w)) && !colorOf(leftOf(w)))
                        {
                            setColor(w, true);
                            x = parentOf(x);
                        }
                        else
                        {
                            if (!colorOf(leftOf(w)))
                            {
                                setColor(rightOf(w), false);
                                setColor(w, true);
                                rotateLeft(w);
                                w = leftOf(parentOf(x));
                            }
                            setColor(w, colorOf(parentOf(x)));
                            setColor(parentOf(x), false);
                            setColor(leftOf(w), false);
                            rotateRight(parentOf(x));
                            x = root;
                        }
                    }
                }

                if (x != null)
                {
                    setColor(x, false);
                }
            }

            private void rotateLeft(Node x)
            {
                if (x != null)
                {
                    Node y = x.right;
                    x.right = y.left;
                    if (y.left != null) y.left.parent = x;
                    y.parent = x.parent;
                    if (x.parent == null)
                    {
                        root = y;
                    }
                    else if (x == leftOf(x.parent))
                    {
                        x.parent.left = y;
                    }
                    else
                    {
                        x.parent.right = y;
                    }
                    y.left = x;
                    x.parent = y;
                }
            }

            private void rotateRight(Node x)
            {
                if (x != null)
                {
                    Node y = x.left;
                    x.left = y.right;
                    if (y.right != null) y.right.parent = x;
                    y.parent = x.parent;
                    if (x.parent == null)
                    {
                        root = y;
                    }
                    else if (x == rightOf(x.parent))
                    {
                        x.parent.right = y;
                    }
                    else
                    {
                        x.parent.left = y;
                    }
                    y.right = x;
                    x.parent = y;
                }
            }

            private Node parentOf(Node n)
            {
                return n != null ? n.parent : null;
            }

            private Node leftOf(Node n)
            {
                return n != null ? n.left : null;
            }

            private Node rightOf(Node n)
            {
                return n != null ? n.right : null;
            }

            private bool colorOf(Node n)
            {
                return n == null ? false : n.color;
            }

            private void setColor(Node n, bool color)
            {
                if (n != null) n.color = color;
            }

            public Node find(object element)
            {
                Node x = root;
                while (x != null && !x.element.Equals(element))
                {
                    if (x.element.CompareTo(element) < 0)
                    {
                        x = x.right;
                    }
                    else
                    {
                        x = x.left;
                    }
                }
                return x;
            }
        }

        private static class RBTree
        {
            private Node root;

            public RBTree()
            {
                root = null;
            }

            public void add(object element)
            {
                Node newNode = new Node(element);
                if (root == null)
                {
                    root = newNode;
                    root.color = false; // Root should always be black
                }
                else
                {
                    insert(newNode, root);
                    fixAfterInsertion(newNode);
                }
            }

            private void insert(Node z, Node x)
            {
                while (x != null)
                {
                    z.parent = x;
                    if (z.element.CompareTo(x.element) < 0)
                    {
                        x = x.left;
                    }
                    else
                    {
                        x = x.right;
                    }
                }

                z.parent = x;
                if (z.parent == null)
                {
                    root = z;
                }
                else if (z.element.CompareTo(z.parent.element) < 0)
                {
                    z.parent.left = z;
                }
                else
                {
                    z.parent.right = z;
                }

                fixAfterInsertion(z);
            }

            private void fixAfterInsertion(Node x)
            {
                x.color = true; // Set new node to red

                while (x != null && x != root && x.parent.color)
                {
                    if (parentOf(x) == leftOf(parentOf(parentOf(x))))
                    {
                        Node y = rightOf(parentOf(parentOf(x)));
                        if (colorOf(y))
                        {
                            setColor(parentOf(x), false);
                            setColor(y, false);
                            setColor(parentOf(parentOf(x)), true);
                            x = parentOf(parentOf(x));
                        }
                        else
                        {
                            if (x == rightOf(parentOf(x)))
                            {
                                x = parentOf(x);
                                rotateLeft(x);
                            }
                            setColor(parentOf(x), false);
                            setColor(parentOf(parentOf(x)), true);
                            rotateRight(parentOf(parentOf(x)));
                        }
                    }
                    else
                    {
                        Node y = leftOf(parentOf(parentOf(x)));
                        if (colorOf(y))
                        {
                            setColor(parentOf(x), false);
                            setColor(y, false);
                            setColor(parentOf(parentOf(x)), true);
                            x = parentOf(parentOf(x));
                        }
                        else
                        {
                            if (x == leftOf(parentOf(x)))
                            {
                                x = parentOf(x);
                                rotateRight(x);
                            }
                            setColor(parentOf(x), false);
                            setColor(parentOf(parentOf(x)), true);
                            rotateLeft(parentOf(parentOf(x)));
                        }
                    }
                }

                root.color = false; // Root should always be black
            }

            public bool remove(object element)
            {
                Node x = root;
                while (x != null && !x.element.Equals(element))
                {
                    if (x.element.CompareTo(element) < 0)
                    {
                        x = x.right;
                    }
                    else
                    {
                        x = x.left;
                    }
                }

                if (x == null) return false;

                deleteNode(x);
                return true;
            }

            private void deleteNode(Node z)
            {
                Node y;
                Node x;

                if (z.left == null || z.right == null)
                {
                    y = z;
                }
                else
                {
                    y = getSuccessor(z);
                }

                x = (y.left != null) ? y.left : y.right;

                if (x != null)
                {
                    x.parent = y.parent;
                }

                if (y.parent == null)
                {
                    root = x;
                }
                else if (y == y.parent.left)
                {
                    y.parent.left = x;
                }
                else
                {
                    y.parent.right = x;
                }

                if (y != z)
                {
                    z.element = y.element;
                }

                if (!y.color)
                {
                    fixAfterDeletion(x);
                }
            }

            private void fixAfterDeletion(Node x)
            {
                while (x != root && !colorOf(x))
                {
                    if (x == leftOf(parentOf(x)))
                    {
                        Node w = rightOf(parentOf(x));

                        if (colorOf(w))
                        {
                            setColor(w, false);
                            setColor(parentOf(x), true);
                            rotateLeft(parentOf(x));
                            w = rightOf(parentOf(x));
                        }

                        if (!colorOf(leftOf(w)) && !colorOf(rightOf(w)))
                        {
                            setColor(w, true);
                            x = parentOf(x);
                        }
                        else
                        {
                            if (!colorOf(rightOf(w)))
                            {
                                setColor(leftOf(w), false);
                                setColor(w, true);
                                rotateRight(w);
                                w = rightOf(parentOf(x));
                            }
                            setColor(w, colorOf(parentOf(x)));
                            setColor(parentOf(x), false);
                            setColor(rightOf(w), false);
                            rotateLeft(parentOf(x));
                            x = root;
                        }
                    }
                    else
                    {
                        Node w = leftOf(parentOf(x));

                        if (colorOf(w))
                        {
                            setColor(w, false);
                            setColor(parentOf(x), true);
                            rotateRight(parentOf(x));
                            w = leftOf(parentOf(x));
                        }

                        if (!colorOf(rightOf(w)) && !colorOf(leftOf(w)))
                        {
                            setColor(w, true);
                            x = parentOf(x);
                        }
                        else
                        {
                            if (!colorOf(leftOf(w)))
                            {
                                setColor(rightOf(w), false);
                                setColor(w, true);
                                rotateLeft(w);
                                w = leftOf(parentOf(x));
                            }
                            setColor(w, colorOf(parentOf(x)));
                            setColor(parentOf(x), false);
                            setColor(leftOf(w), false);
                            rotateRight(parentOf(x));
                            x = root;
                        }
                    }
                }

                if (x != null)
                {
                    setColor(x, false);
                }
            }

            private void rotateLeft(Node x)
            {
                if (x != null)
                {
                    Node y = x.right;
                    x.right = y.left;
                    if (y.left != null) y.left.parent = x;
                    y.parent = x.parent;
                    if (x.parent == null)
                    {
                        root = y;
                    }
                    else if (x == leftOf(x.parent))
                    {
                        x.parent.left = y;
                    }
                    else
                    {
                        x.parent.right = y;
                    }
                    y.left = x;
                    x.parent = y;
                }
            }

            private void rotateRight(Node x)
            {
                if (x != null)
                {
                    Node y = x.left;
                    x.left = y.right;
                    if (y.right != null) y.right.parent = x;
                    y.parent = x.parent;
                    if (x.parent == null)
                    {
                        root = y;
                    }
                    else if (x == rightOf(x.parent))
                    {
                        x.parent.right = y;
                    }
                    else
                    {
                        x.parent.left = y;
                    }
                    y.right = x;
                    x.parent = y;
                }
            }

            private Node parentOf(Node n)
            {
                return n != null ? n.parent : null;
            }

            private Node leftOf(Node n)
            {
                return n != null ? n.left : null;
            }

            private Node rightOf(Node n)
            {
                return n != null ? n.right : null;
            }

            private bool colorOf(Node n)
            {
                return n == null ? false : n.color;
            }

            private void setColor(Node n, bool color)
            {
                if (n != null) n.color = color;
            }

            public Node find(object element)
            {
                Node x = root;
                while (x != null && !x.element.Equals(element))
                {
                    if (x.element.CompareTo(element) < 0)
                    {
                        x = x.right;
                    }
                    else
                    {
                        x = x.left;
                    }
                }
                return x;
            }
        }

        private static class RBT
        {
            private Node root;

            public RBT()
            {
                root = null;
            }

            public void add(object element)
            {
                Node newNode = new Node(element);
                if (root == null)
                {
                    root = newNode;
                    root.color = false; // Root should always be black
                }
                else
                {
                    insert(newNode, root);
                    fixAfterInsertion(newNode);
                }
            }

            private void insert(Node z, Node x)
            {
                while (x != null)
                {
                    z.parent = x;
                    if (z.element.CompareTo(x.element) < 0)
                    {
                        x = x.left;
                    }
                    else
                    {
                        x = x.right;
                    }
                }

                z.parent = x;
                if (z.parent == null)
                {
                    root = z;
                }
                else if (z.element.CompareTo(z.parent.element) < 0)
                {
                    z.parent.left = z;
                }
                else
                {
                    z.parent.right = z;
                }

                fixAfterInsertion(z);
            }

            private void fixAfterInsertion(Node x)
            {
                x.color = true; // Set new node to red

                while (x != null && x != root && x.parent.color)
                {
                    if (parentOf(x) == leftOf(parentOf(parentOf(x))))
                    {
                        Node y = rightOf(parentOf(parentOf(x)));
                        if (colorOf(y))
                        {
                            setColor(parentOf(x), false);
                            setColor(y, false);
                            setColor(parentOf(parentOf(x)), true);
                            x = parentOf(parentOf(x));
                        }
                        else
                        {
                            if (x == rightOf(parentOf(x)))
                            {
                                x = parentOf(x);
                                rotateLeft(x);
                            }
                            setColor(parentOf(x), false);
                            setColor(parentOf(parentOf(x)), true);
                            rotateRight(parentOf(parentOf(x)));
                        }
                    }
                    else
                    {
                        Node y = leftOf(parentOf(parentOf(x)));
                        if (colorOf(y))
                        {
                            setColor(parentOf(x), false);
                            setColor(y, false);
                            setColor(parentOf(parentOf(x)), true);
                            x = parentOf(parentOf(x));
                        }
                        else
                        {
                            if (x == leftOf(parentOf(x)))
                            {
                                x = parentOf(x);
                                rotateRight(x);
                            }
                            setColor(parentOf(x), false);
                            setColor(parentOf(parentOf(x)), true);
                            rotateLeft(parentOf(parentOf(x)));
                        }
                    }
                }

                root.color = false; // Root should always be black
            }

            public bool remove(object element)
            {
                Node x = root;
                while (x != null && !x.element.Equals(element))
                {
                    if (x.element.CompareTo(element) < 0)
                    {
                        x = x.right;
                    }
                    else
                    {
                        x = x.left;
                    }
                }

                if (x == null) return false;

                deleteNode(x);
                return true;
            }

            private void deleteNode(Node z)
            {
                Node y;
                Node x;

                if (z.left == null || z.right == null)
                {
                    y = z;
                }
                else
                {
                    y = getSuccessor(z);
                }

                x = (y.left != null) ? y.left : y.right;

                if (x != null)
                {
                    x.parent = y.parent;
                }

                if (y.parent == null)
                {
                    root = x;
                }
                else if (y == y.parent.left)
                {
                    y.parent.left = x;
                }
                else
                {
                    y.parent.right = x;
                }

                if (y != z)
                {
                    z.element = y.element;
                }

                if (!y.color)
                {
                    fixAfterDeletion(x);
                }
            }

            private void fixAfterDeletion(Node x)
            {
                while (x != root && !colorOf(x))
                {
                    if (x == leftOf(parentOf(x)))
                    {
                        Node w = rightOf(parentOf(x));

                        if (colorOf(w))
                        {
                            setColor(w, false);
                            setColor(parentOf(x), true);
                            rotateLeft(parentOf(x));
                            w = rightOf(parentOf(x));
                        }

                        if (!colorOf(leftOf(w)) && !colorOf(rightOf(w)))
                        {
                            setColor(w, true);
                            x = parentOf(x);
                        }
                        else
                        {
                            if (!colorOf(rightOf(w)))
                            {
                                setColor(leftOf(w), false);
                                setColor(w, true);
                                rotateRight(w);
                                w = rightOf(parentOf(x));
                            }
                            setColor(w, colorOf(parentOf(x)));
                            setColor(parentOf(x), false);
                            setColor(rightOf(w), false);
                            rotateLeft(parentOf(x));
                            x = root;
                        }
                    }
                    else
                    {
                        Node w = leftOf(parentOf(x));

                        if (colorOf(w))
                        {
                            setColor(w, false);
                            setColor(parentOf(x), true);
                            rotateRight(parentOf(x));
                            w = leftOf(parentOf(x));
                        }

                        if (!colorOf(rightOf(w)) && !colorOf(leftOf(w)))
                        {
                            setColor(w, true);
                            x = parentOf(x);
                        }
                        else
                        {
                            if (!colorOf(leftOf(w)))
                            {
                                setColor(rightOf(w), false);
                                setColor(w, true);
                                rotateLeft(w);
                                w = leftOf(parentOf(x));
                            }
                            setColor(w, colorOf(parentOf(x)));
                            setColor(parentOf(x), false);
                            setColor(leftOf(w), false);
                            rotateRight(parentOf(x));
                            x = root;
                        }
                    }
                }

                if (x != null)
                {
                    setColor(x, false);
                }
            }

            private void rotateLeft(Node x)
            {
                if (x != null)
                {
                    Node y = x.right;
                    x.right = y.left;
                    if (y.left != null) y.left.parent = x;
                    y.parent = x.parent;
                    if (x.parent == null)
                    {
                        root = y;
                    }
                    else if (x == leftOf(x.parent))
                    {
                        x.parent.left = y;
                    }
                    else
                    {
                        x.parent.right = y;
                    }
                    y.left = x;
                    x.parent = y;
                }
            }

            private void rotateRight(Node x)
            {
                if (x != null)
                {
                    Node y = x.left;
                    x.left = y.right;
                    if (y.right != null) y.right.parent = x;
                    y.parent = x.parent;
                    if (x.parent == null)
                    {
                        root = y;
                    }
                    else if (x == rightOf(x.parent))
                    {
                        x.parent.right = y;
                    }
                    else
                    {
                        x.parent.left = y;
                    }
                    y.right = x;
                    x.parent = y;
                }
            }

            private Node parentOf(Node n)
            {
                return n != null ? n.parent : null;
            }

            private Node leftOf(Node n)
            {
                return n != null ? n.left : null;
            }

            private Node rightOf(Node n)
            {
                return n != null ? n.right : null;
            }

            private bool colorOf(Node n)
            {
                return n == null ? false : n.color;
            }

            private void setColor(Node n, bool color)
            {
                if (n != null) n.color = color;
            }

            public Node find(object element)
            {
                Node x = root;
                while (x != null && !x.element.Equals(element))
                {
                    if (x.element.CompareTo(x.element) < 0)
                    {
                        x = x.right;
                    }
                    else
                    {
                        x = x.left;
                    }
                }
                return x;
            }
        }

        private static class Node
        {
            public object element;
            public Node left, right, parent;
            public bool color; // false = black, true = red

            public Node(object element)
            {
                this.element = element;
                this.left = null;
                this.right = null;
                this.parent = null;
                this.color = true; // New nodes are initially red
            }
        }
    }
}