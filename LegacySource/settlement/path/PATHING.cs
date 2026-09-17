using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Settlement.Path
{
    public sealed class PATHING : SettResource
    {
        public readonly CostMethods coster = new CostMethods();
        public readonly SFINDERS finders = new SFINDERS();

        private BOOLEAN_MUTABLE performanceTest = new BOOLEANImp(false);

        public readonly PlayerHuristics huristics = new PlayerHuristics();

        public readonly SCOMPONENTS comps = new SCOMPONENTS();
        public readonly AvailabilityMap availability = new AvailabilityMap(comps);
        public readonly FinderThread thread = new FinderThread(comps);

        public PATHING() : base("PATHING", true)
        {
            new ON_TOP_RENDERABLE
            {
                {
                    IDebugPanelSett.Add("availability", new BOOLEANImp
                    {
                        public BOOLEAN_MUTABLE Set(bool bool)
                        {
                            if (bool)
                                Add();
                            else
                                Remove();
                            return base.Set(bool);
                        }

                        public bool Is()
                        {
                            // TODO Auto-generated method stub
                            return false;
                        }
                    });
                }

                public void Render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
                {
                    RenderData.RenderIterator i = data.OnScreenTiles();
                    COLOR.WHITE65.Bind();
                    while (i.Has())
                    {
                        if (cost.Get(i.Tile()) < 0)
                            COLOR.RED100.Bind();
                        else if (cost.Get(i.Tile()) == 1)
                            COLOR.BLUE100.Bind();
                        else
                            COLOR.YELLOW100.Bind();
                        SPRITES.Cons().BIG.Dashed.Render(r, 0x0F, i.X(), i.Y());
                        i.Next();
                    }
                    COLOR.Unbind();
                }
            };

            IDebugPanelSett.Add("2100 paths/s", performanceTest);
        }

        public SFINDERS Finders()
        {
            return finders;
        }

        protected override void Save(FilePutter saveFile)
        {
            thread.Stop();
            huristics.Saver.Save(saveFile);
            thread.Start();
            finders.Saver.Save(saveFile);
        }

        protected override void Load(FileGetter saveFile) => throw new NotImplementedException();

        protected override void Clear()
        {
            thread.Stop();
            huristics.Saver.Clear();
            comps.Clear();
            finders.Saver.Clear();
        }

        public bool WillUpdateTile(int tx, int ty)
        {
            return comps.Zero.Updating().Is(tx, ty);
        }

        public bool WillUpdate()
        {
            return comps.Zero.Uping();
        }

        protected override void Update(double ds, Profiler profiler)
        {
            thread.SetStop();
            huristics.Update(ds);
            finders.Update(ds);
            thread.Stop();
            comps.Update();
            thread.Start();
        }

        protected override void Init(bool loaded)
        {
            thread.Stop();
            availability.Init();
            comps.Init();
            thread.Start();
        }

        public readonly MAP_BOOLEAN Solidity = new MAP_BOOLEAN
        {
            public bool Is(int tile)
            {
                return availability.Get(tile).Player < 0;
            }

            public bool Is(int tx, int ty)
            {
                if (!SETT.IN_BOUNDS(tx, ty))
                    return true;
                return Is(tx + ty * SETT.TWIDTH);
            }
        };

        public readonly MAP_BOOLEAN Reachability = new MAP_BOOLEAN
        {
            public bool Is(int tx, int ty)
            {
                SComponent c = comps.SuperComp.Get(tx, ty);
                if (c != null && c.Is(THRONE.Coo()))
                    return true;
                for (int i = 0; i < DIR.ORTHO.Size; i++)
                {
                    c = comps.SuperComp.Get(tx, ty, DIR.ORTHO.Get(i));
                    if (c != null && c.Is(THRONE.Coo()))
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool Is(int tile)
            {
                throw new RuntimeException();
            }
        };

        public readonly MAP_BOOLEAN Connectivity = new MAP_BOOLEAN
        {
            public bool Is(int tx, int ty)
            {
                SComponent c = comps.SuperComp.Get(tx, ty);
                if (c != null && c.Is(THRONE.Coo()))
                    return true;
                return false;
            }

            public bool Is(int tile)
            {
                return Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }
        };

        public AVAILABILITY GetAvailability(int x, int y)
        {
            if (!IN_BOUNDS(x, y))
                return AVAILABILITY.SOLID;
            return availability.Get(x, y);
        }

        public readonly MAP_DOUBLE Cost = new MAP_DOUBLE
        {
            public double Get(int tile)
            {
                return availability.Get(tile).Player;
            }

            public double Get(int tx, int ty)
            {
                AVAILABILITY a = availability.Get(tx, ty);
                if (a == null)
                    return -1;
                return availability.Get(tx, ty).Player;
            }
        };

        public bool IsInTheNeighbourhood(int tx, int ty, int dx, int dy)
        {
            SComponent c = comps.Levels.Get(0).Get(tx, ty);
            if (c == null)
                return false;
            SComponent d = comps.Levels.Get(0).Get(dx, dy);
            if (d == null)
                return false;
            if (c == d)
                return true;
            SComponentEdge e = c.EdgeFirst();
            while (e != null)
            {
                if (e.To() == d)
                    return true;
                e = e.Next();
            }
            return false;
        }
    }
}