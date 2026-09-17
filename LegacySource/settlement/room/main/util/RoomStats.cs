using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Main.Util
{
    public class RoomStats : RoomResource
    {
        private RoomStatsList list = new RoomStatsList(256);
        private RoomStatsList broken = new RoomStatsList(512);

        public RoomStatsList Finished()
        {
            return list;
        }

        public RoomStatsList Broken()
        {
            return broken;
        }

        protected override void Save(BinaryWriter file)
        {
            list.Save(file);
            broken.Save(file);
        }

        protected override void Load(BinaryReader file)
        {
            list.Load(file);
            broken.Load(file);
        }

        protected override void Clear()
        {
            list.Clear();
            broken.Clear();
        }

        public class RoomStatsList : SAVABLE
        {
            private Queue<int> list;
            private Queue<int> listTemp;

            private RoomStatsList(int size)
            {
                list = new Queue<int>(size);
                listTemp = new Queue<int>(size);
            }

            public void Add(int mx, int my)
            {
                if (list.Count >= list.Capacity)
                    list.Dequeue();
                int ii = mx | (my << 16);
                list.Enqueue(ii);
            }

            public void Remove(int mx, int my)
            {
                listTemp.Clear();
                while (list.Count > 0)
                {
                    int i = list.Dequeue();
                    int tx = i & 0x0FFFF;
                    int ty = (i >> 16) & 0x0FFFF;
                    if (mx == tx && my == ty)
                    {
                        continue;
                    }
                    listTemp.Enqueue(i);
                }
                Queue<int> l = list;
                list = listTemp;
                listTemp = l;
            }

            public int Amount()
            {
                return list.Count;
            }

            private readonly Coo tmp = new Coo();

            public COORDINATE Poll()
            {
                if (list.Count == 0)
                    return null;
                int i = list.Dequeue();
                int tx = i & 0x0FFFF;
                int ty = (i >> 16) & 0x0FFFF;
                Room r = SETT.ROOMS().map.Get(tx, ty);
                if (r != null)
                {
                    tmp.Set(tx, ty);
                    return tmp;
                }
                return null;
            }

            public void Save(BinaryWriter file)
            {
                list.Save(file);
            }

            public void Load(BinaryReader file)
            {
                list.Load(file);
            }

            public void Clear()
            {
                list.Clear();
            }
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }
    }
}