using System;
using System.Collections.Generic;

namespace snake2d
{
    public class COORDINATE
    {
        public int x;
        public int y;

        public COORDINATE Set(int x, int y)
        {
            this.x = x;
            this.y = y;
            return this;
        }
    }

    public class DIR
    {
        public static List<DIR> ALL = new List<DIR>
        {
            new DIR(1, 0),
            new DIR(-1, 0),
            new DIR(0, 1),
            new DIR(0, -1)
        };

        public int x;
        public int y;

        public DIR(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class PathTile
    {
        public int x;
        public int y;
        public double value;

        public int X() => x;
        public int Y() => y;
        public double GetValue() => value;
    }

    public class PathUtilOnline
    {
        public class Flooder
        {
            private List<PathTile> queue = new List<PathTile>();
            private List<PathTile> tempQueue = new List<PathTile>();

            public void Init(object context) { }
            public void Done() { }

            public void PushSloppy(int x, int y, double value)
            {
                queue.Add(new PathTile { x = x, y = y, value = value });
            }

            public bool HasMore() => queue.Count > 0;

            public PathTile PollSmallest()
            {
                PathTile smallest = queue[0];
                for (int i = 1; i < queue.Count; i++)
                {
                    if (queue[i].value < smallest.value)
                    {
                        smallest = queue[i];
                    }
                }
                queue.Remove(smallest);
                return smallest;
            }

            public void PushSmaller(int x, int y, double value)
            {
                tempQueue.Add(new PathTile { x = x, y = y, value = value });
            }

            public void Commit()
            {
                queue.AddRange(tempQueue);
                tempQueue.Clear();
            }
        }

        public Flooder GetFlooder() => new Flooder();
    }

    public class ArrayCooShort
    {
        private List<COORDINATE> coordinates = new List<COORDINATE>();

        public ArrayCooShort(int capacity)
        {
            for (int i = 0; i < capacity; i++)
            {
                coordinates.Add(new COORDINATE());
            }
        }

        public COORDINATE Set(int index)
        {
            return coordinates[index];
        }

        public int Size() => coordinates.Count;
    }

    public class Alloc
    {
        public static byte[] Bb(int size)
        {
            return new byte[size];
        }
    }

    public class Printer
    {
        public static void Ln(string message)
        {
            Console.WriteLine(message);
        }
    }

    public class CircleCooIterator
    {
        private readonly ArrayCooShort coos;
        private readonly byte[] radiuses;
        private readonly byte[] sides;

        public CircleCooIterator(int radius, PathUtilOnline.Flooder p)
        {
            if (radius > 127)
                throw new Exception("Radius too large");

            {
                p.Init(this);

                int amount = 0;
                p.PushSloppy(radius, radius, 0);

                while (p.HasMore())
                {
                    PathTile t = p.PollSmallest();
                    if (t.GetValue() > radius)
                        break;

                    amount++;

                    for (int i = 0; i < DIR.ALL.Count; i++)
                    {
                        DIR d = DIR.ALL[i];
                        int x = t.X() + d.x;
                        int y = t.Y() + d.y;
                        if (x < 0 || y < 0)
                            continue;
                        double v = Math.Sqrt((x - radius) * (x - radius) + (y - radius) * (y - radius));
                        p.PushSmaller(x, y, v);
                    }
                }

                coos = new ArrayCooShort(amount);
                radiuses = Alloc.Bb(amount);
                sides = Alloc.Bb(amount);

                p.Done();
            }

            {
                p.Init(this);

                int index = 0;
                p.PushSloppy(radius, radius, 0);

                while (p.HasMore())
                {
                    PathTile t = p.PollSmallest();
                    if (t.GetValue() > radius)
                        break;

                    coos.Set(index).Set(t.X() - radius, t.Y() - radius);
                    radiuses[index] = (byte)t.GetValue();
                    sides[index] = (byte)(Math.Abs(t.X() - radius) > Math.Abs(t.Y() - radius) ? Math.Abs(t.X() - radius) : Math.Abs(t.Y() - radius));
                    index++;

                    for (int i = 0; i < DIR.ALL.Count; i++)
                    {
                        DIR d = DIR.ALL[i];
                        int x = t.X() + d.x;
                        int y = t.Y() + d.y;
                        if (x < 0 || y < 0)
                            continue;
                        double v = Math.Sqrt((x - radius) * (x - radius) + (y - radius) * (y - radius));
                        p.PushSmaller(x, y, v);
                    }
                }
                p.Done();
            }
        }

        public COORDINATE Get(int index)
        {
            return coos.Set(index);
        }

        public int Radius(int index)
        {
            return radiuses[index];
        }

        public int SideLength(int index)
        {
            return sides[index];
        }

        public int Length()
        {
            return coos.Size();
        }

        public static void Main(string[] args)
        {
            CircleCooIterator c = new CircleCooIterator(50, new PathUtilOnline().GetFlooder());

            int i = 0;
            while (c.SideLength(i) <= 5)
            {
                Printer.Ln(c.Get(i).x + " " + c.Get(i).y);
                i++;
            }
            Printer.Ln(i.ToString());
        }
    }
}