using System;

namespace Snake2D.Path1D
{
    public sealed class Pathing1D
    {
        private readonly PTile1D[] tiles;
        int id = 0;
        readonly RBTileTree tree = new RBTileTree();
        private object user;

        public Pathing1D(int size)
        {
            tiles = new PTile1D[size];
            for (int i = 0; i < size; i++)
                tiles[i] = new PTile1D(i);
        }

        public Pathing1D Init(object user)
        {
            if (this.user != null)
            {
                throw new RuntimeException("already in use by: " + this.user.ToString());
            }
            this.user = user;
            id++;
            if (id == 0)
            {
                for (int i = 0; i < tiles.Length; i++)
                    tiles[i].pathId = 0;
                id = 1;
            }
            tree.Clear();
            return this;
        }

        public void Done()
        {
            user = null;
        }

        public PTile1D GetTile(int index)
        {
            return tiles[index];
        }

        public PTile1D PushGreater(int index, double value)
        {
            return PushGreater(index, value, null);
        }

        public PTile1D PushGreater(int index, double value, PTile1D parent)
        {
            PTile1D t = tiles[index];

            if (t.pathId == id)
            {
                if (t.value >= value)
                    return t;
                if (t.closed)
                    return t;
                tree.Remove(t);
            }

            t.pathId = id;
            t.closed = false;
            t.value = (float)value;
            t.pathParent = parent;
            tree.Put(t);
            return t;
        }

        public PTile1D PushSloppy(int index, double value)
        {
            return PushSloppy(index, value, null);
        }

        public PTile1D PushSloppy(int index, double value, PTile1D parent)
        {
            PTile1D t = tiles[index];

            if (t.pathId == id)
            {
                return null;
            }

            t.pathId = id;
            t.value = (float)value;
            t.pathParent = parent;
            tree.Put(t);
            t.closed = true;
            return t;
        }

        public PTile1D PushSmaller(int index, double value)
        {
            return PushSmaller(index, (float)value, null);
        }

        public PTile1D PushSmaller(int index, double value, PTile1D parent)
        {
            PTile1D t = tiles[index];

            if (t.pathId == id)
            {
                if (t.value <= value)
                    return null;
                if (t.closed)
                    return null;
                tree.Remove(t);
            }

            t.pathId = id;
            t.closed = false;
            t.value = (float)value;
            t.pathParent = parent;
            tree.Put(t);
            return t;
        }

        public bool HasBeenPushed(int index)
        {
            PTile1D t = tiles[index];
            return t.pathId == id;
        }

        public void Unclose(int index)
        {
            PTile1D t = tiles[index];
            t.closed = false;
        }

        /**
         * 
         * @return a tile that will be considered again.
         */
        public PTile1D PollAndReopen()
        {
            PTile1D t = tree.PollGreatest();
            t.pathId = id - 1;
            return t;
        }

        /**
         * closes the tile
         * @return the tile that has the highest value
         */
        public PTile1D PollGreatest()
        {
            PTile1D t = tree.PollGreatest();
            t.closed = true;
            return t;
        }

        public int Pushed()
        {
            return tree.Size();
        }

        public PTile1D PollSmallest()
        {
            PTile1D t = tree.PollSmallest();
            t.closed = true;
            return t;
        }

        public void CloseGreater(int index, double value)
        {
            PTile1D t = tiles[index];
            if (t.pathId == id && value > t.value)
                t.value = (float)value;
            t.pathId = id;
            t.closed = true;
        }

        public PTile1D Close(int index, double value)
        {
            PTile1D t = tiles[index];
            t.value = (float)value;
            t.pathId = id;
            t.closed = true;
            return t;
        }

        public PTile1D Close(int index, double value, PTile1D parent)
        {
            PTile1D t = tiles[index];
            t.value = (float)value;
            t.pathId = id;
            t.closed = true;
            t.pathParent = parent;
            return t;
        }

        public float GetValue(int index)
        {
            PTile1D t = tiles[index];
            if (t.pathId == id)
            {
                return t.value;
            }
            return 0;
        }

        public bool HasMore()
        {
            return tree.Size() > 0;
        }

        public float GetValue2(int index)
        {
            PTile1D t = tiles[index];
            return t.value2;
        }

        public void SetValue2(int index, double f)
        {
            PTile1D t = tiles[index];
            t.value2 = (float)f;
        }

        public PTile1D Force(int index, float value, PTile1D parent)
        {
            PTile1D t = tiles[index];
            t.value = (float)value;
            t.pathId = id;
            t.closed = true;
            t.pathParent = parent;
            return t;
        }

        public PTile1D Reverse(PTile1D t)
        {
            Init(this);
            PTile1D p = t.pathParent;
            t.pathParent = null;
            t = Reverse(p, t);
            Done();
            return t;
        }

        private PTile1D Reverse(PTile1D t, PTile1D newparent)
        {
            if (t == null)
                return newparent;
            PTile1D parent = t.pathParent;
            t.pathParent = newparent;
            return Reverse(parent, t);
        }
    }
}