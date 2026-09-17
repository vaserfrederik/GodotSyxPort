using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Room.Home.House
{
    public sealed class OddHome
    {
        private readonly QueueInteger[] queues;

        public OddHome()
        {
            queues = new QueueInteger[HGROUP.All().Count];
            for (int y = 0; y < queues.Length; y++)
            {
                queues[y] = new QueueInteger(256);
            }
        }

        public SAVABLE Saver => new SAVABLE
        {
            Save = file =>
            {
                foreach (QueueInteger q in queues)
                {
                    q.Save(file);
                }
            },
            Load = file =>
            {
                foreach (QueueInteger q in queues)
                {
                    q.Load(file);
                }
            },
            Clear = () =>
            {
                foreach (QueueInteger q in queues)
                {
                    q.Clear();
                }
            }
        };

        public void Update(int tx, int ty)
        {
            HomeInstance h = Test(tx, ty, this);
            if (h == null)
                return;

            HTypeBits s = h.Availability();
            for (int ti = 0; ti < HGROUP.All().Count; ti++)
            {
                if (s.Is(ti))
                {
                    HGROUP t = HGROUP.All()[ti];
                    QueueInteger i = queues[t.Index()];
                    if (!i.HasRoom())
                        i.Poll();
                    i.Push(tx + ty * SETT.TWIDTH);
                }
            }
        }

        public HomeInstance Get(Humanoid h, object user)
        {
            HGROUP t = HGROUP.Get(h);
            while (queues[t.Index()].HasNext())
            {
                int i = queues[t.Index()].Peek();
                int tx = i % SETT.TWIDTH;
                int ty = i / SETT.TWIDTH;

                HomeInstance ho = Test(tx, ty, user);
                if (ho != null && ho.Availability() != null && ho.Availability().Is(h))
                {
                    return ho;
                }
                else
                {
                    queues[t.Index()].Poll();
                }
            }
            return null;
        }

        public bool Has(Humanoid h)
        {
            HGROUP t = HGROUP.Get(h);
            while (queues[t.Index()].HasNext())
            {
                int i = queues[t.Index()].Peek();
                int tx = i % SETT.TWIDTH;
                int ty = i / SETT.TWIDTH;

                HomeInstance ho = Test(tx, ty, this);
                if (ho != null)
                {
                    return true;
                }
                queues[t.Index()].Poll();
            }
            return false;
        }

        private HomeInstance Test(int tx, int ty, object user)
        {
            if (!SETT.PATH().Connectivity.Is(tx, ty))
                return null;

            HomeInstance h = SETT.ROOMS().HOME.Getter.Get(tx, ty);
            if (h != null && h.ServiceX() == tx && h.ServiceY() == ty && h.Occupants() < h.OccupantsMax())
            {
                return h;
            }

            return null;
        }
    }

    public class QueueInteger
    {
        private readonly List<int> _queue;
        private int _head;

        public QueueInteger(int capacity)
        {
            _queue = new List<int>(capacity);
            _head = 0;
        }

        public bool HasRoom()
        {
            return _queue.Count - _head < _queue.Capacity;
        }

        public void Push(int value)
        {
            _queue.Add(value);
        }

        public int Peek()
        {
            return _queue[_head];
        }

        public void Poll()
        {
            _head++;
        }

        public void Save(FilePutter file)
        {
            file.WriteInt(_head);
            file.WriteInt(_queue.Count);
            for (int i = _head; i < _queue.Count; i++)
            {
                file.WriteInt(_queue[i]);
            }
        }

        public void Load(FileGetter file)
        {
            _head = file.ReadInt();
            int count = file.ReadInt();
            _queue.Clear();
            for (int i = 0; i < count; i++)
            {
                _queue.Add(file.ReadInt());
            }
        }

        public void Clear()
        {
            _queue.Clear();
            _head = 0;
        }
    }

    public interface SAVABLE
    {
        Action<FilePutter> Save { get; }
        Action<FileGetter> Load { get; }
        Action Clear { get; }
    }

    public static class HGROUP
    {
        public static List<HGROUP> All() => new List<HGROUP>();
        public int Index() => 0;
    }

    public static class HTypeBits
    {
        public bool Is(int ti) => true;
    }

    public class Humanoid
    {
    }

    public static class SETT
    {
        public static PATH PATH() => new PATH();
        public static ROOMS ROOMS() => new ROOMS();
        public static int TWIDTH => 1;
    }

    public class PATH
    {
        public CONNECTIVITY Connectivity => new CONNECTIVITY();
    }

    public class CONNECTIVITY
    {
        public bool Is(int tx, int ty) => true;
    }

    public class ROOMS
    {
        public HOME HOME => new HOME();
    }

    public class HOME
    {
        public GETTER Getter => new GETTER();
    }

    public class GETTER
    {
        public HomeInstance Get(int tx, int ty) => new HomeInstance();
    }

    public class HomeInstance
    {
        public int ServiceX() => 0;
        public int ServiceY() => 0;
        public int Occupants() => 0;
        public int OccupantsMax() => 1;
        public HTypeBits Availability() => new HTypeBits();
    }
}