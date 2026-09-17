using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Debug;
using Init.Constant;
using Snake2D.Renderer;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Misc;
using Snake2D.Util.Sets;
using Util.Rendering;
using View.Tool;
using World.Entity.Army;
using World.Entity.Caravan;
using World.Entity.Haven;
using World.Map.Regions;

namespace World.Entity
{
    public class WEntities : WORLD.WorldResource
    {
        private readonly ArrayListResize<WEntity> fast;
        private readonly ArrayListResize<WEntity> slow;
        private readonly _WEntityMap map;
        private readonly ArrayList<WEntity> tmp = new ArrayList<WEntity>(2056);
        private readonly ArrayList<WEntityConstructor<?>> constructors = new ArrayList<>(20);
        private readonly Rec rectmp = new Rec();

        public readonly Shipments caravans = new Shipments(constructors);
        public readonly WArmyConstructor armies = new WArmyConstructor(constructors);
        public readonly WHavens havens = new WHavens(constructors);
        private double slowUp = 0;

        private readonly Tree<WEntity> renderables = new Tree<WEntity>(2056)
        {
            protected override bool IsGreaterThan(WEntity current, WEntity cmp)
            {
                return current.Z < cmp.Z;
            }
        };

        public WEntities(WORLD world) : base("entities", "ENTS")
        {
            fast = new ArrayListResize<WEntity>(1024, 64000);
            slow = new ArrayListResize<WEntity>(256, 64000);
            map = new _WEntityMap(WORLD.PWIDTH(), WORLD.PHEIGHT());
        }

        private readonly WorldResourceManager saver = new WorldResourceManager()
        {
            public void Save(FilePutter file)
            {
                foreach (WEntityConstructor<?> c in constructors)
                    c.Save(file);
                file.D(slowUp);
                file.I(fast.Size + slow.Size);
                foreach (WEntity e in fast)
                {
                    file.I(e.Constructor().Index);
                    e.Save(file);
                    e.HitBox.Save(file);
                }
                foreach (WEntity e in slow)
                {
                    file.I(e.Constructor().Index);
                    e.Save(file);
                    e.HitBox.Save(file);
                }
            }

            public void Load(FileGetter file) throws IOException
            {
                foreach (WEntityConstructor<?> c in constructors)
                    c.Load(file);
                fast.Clear();
                slow.Clear();
                map.Clear();
                slowUp = file.D();
                int am = file.I();

                for (int i = 0; i < am; i++)
                {
                    WEntityConstructor<?> c = constructors.Get(file.I());
                    WEntity e = c.Create();
                    e = e.Load(file);
                    e.HitBox.Load(file);
                    WEntities.this.Clear(e);
                    e.Index = c.Fast ? fast.Add(e) : slow.Add(e);
                    e.RenderNext = null;
                    e.RegionNext = null;
                    e.RegionI = -1;
                    map.Add(e);
                }
            }

            public void Clear()
            {
                foreach (WEntityConstructor<?> c in constructors)
                    c.Clear();
                fast.Clear();
                slow.Clear();
                map.Clear();
            }

            public void Generate(ACTION loadPrint)
            {
                Clear();
                loadPrint.Exe();
                new Generator();
            }

            public LIST<PLACABLE> MakePlacers(ToolManager tm)
            {
                ArrayListGrower<PLACABLE> res = new ArrayListGrower<>();
                res.Add(new Placers(havens.Types));
                return res;
            }
        };

        public override WorldResourceManager Saver()
        {
            return saver;
        }

        private void Clear(WEntity e)
        {
            e.Index = -1;
            e.RenderNext = null;
            e.RegionNext = null;
            e.RegionI = -1;
            e.GridX = -1;
            e.GridY = -1;
        }

        public bool CanAdd(bool fast)
        {
            return fast ? this.fast.HasRoom() : this.slow.HasRoom();
        }

        void Add(WEntity e)
        {
            if (e.Index != -1)
                throw new RuntimeException();
            int i = e.Constructor().Fast ? this.fast.Add(e) : slow.Add(e);
            Clear(e);
            e.Index = i;
            map.Add(e);
        }

        WEntity RegFirst(Region reg)
        {
            return map.RegFirst(reg);
        }

        void Remove(WEntity e)
        {
            if (e.Index == -1)
                throw new RuntimeException();
            map.Remove(e);

            ArrayListResize<WEntity> ents = e.Constructor().Fast ? this.fast : slow;

            WEntity e2 = ents.Remove(ents.Size - 1);

            if (e2 != e)
            {
                ents.Replace(e.Index, e2);
                e2.Index = e.Index;
            }
            Clear(e);
        }

        public override void Update(double ds, Profiler prof)
        {
            prof.LogStart(this);

            foreach (WEntityConstructor<?> c in constructors)
            {
                c.Update(ds);
            }

            for (int i = 0; i < fast.Size; i++)
            {
                WEntity e = fast.Get(i);
                e.Update(ds);
                if (!e.Added())
                    i--;
                else
                    map.Move(e);
            }

            if (slowUp > 10000)
            {
                slowUp -= 10000;
            }

            int from = (int)slowUp;
            slowUp += ds * 0.1;
            int to = (int)slowUp;
            int am = to - from;
            for (int k = 0; k < am; k++)
            {
                if (slow.Size <= 0)
                    break;

                int i = from + k;
                i %= slow.Size;

                if (k != 0 && i == from)
                    break;

                WEntity e = slow.Get(i);
                e.Update(ds);
                if (!e.Added())
                    i--;
                else
                    map.Move(e);
            }

            prof.LogEnd(this);
        }

        public void Fill(RECTANGLE area, LISTE<WEntity> result)
        {
            map.Fill(area, result);
        }

        public LIST<WEntity> Fill(RECTANGLE area)
        {
            tmp.Clear();
            Fill(area, tmp);
            return tmp;
        }

        public LIST<WEntity> Fill(int x1, int x2, int y1, int y2)
        {
            tmp.Clear();
            map.Fill(x1, x2, y1, y2, tmp);
            return tmp;
        }

        public LIST<WEntity> FillTiles(int x1, int x2, int y1, int y2)
        {
            return Fill(x1 * C.TILE_SIZE, x2 * C.TILE_SIZE, y1 * C.TILE_SIZE, y2 * C.TILE_SIZE);
        }

        public LIST<WEntity> Fill(int x1, int y1)
        {
            tmp.Clear();
            map.Fill(x1, x1 + 1, y1, y1 + 1, tmp);
            return tmp;
        }

        public void Fill(COORDINATE coo, LISTE<WEntity> result)
        {
            Fill(coo.X(), coo.Y(), result);
        }

        public void Fill(int x, int y, LISTE<WEntity> result)
        {
            map.Fill(x, y, result);
        }

        public WEntity GetTallest(COORDINATE coo)
        {
            tmp.Clear();
            Fill(coo, tmp);
            WEntity tallest = null;
            double dist = double.MaxValue;
            foreach (WEntity e in tmp)
            {
                double d = coo.Distance(e.Body().CX(), e.Body().CY());
                if (tallest == null || d < dist)
                {
                    tallest = e;
                    dist = d;
                }
            }
            return tallest;
        }

        public bool AreaIsClearOfEnts(RECTANGLE rec)
        {
            return Fill(rec).IsEmpty();
        }

        public LISTE<WEntity> GetTempsAtTile(int tileX, int tileY, int tilesX, int tilesY)
        {
            tmp.Clear();

            rectmp.Set(tileX * C.TILE_SIZE, tileY * C.TILE_SIZE, (tileX + tilesX) * C.TILE_SIZE, (tileY + tilesY) * C.TILE_SIZE);

            map.Fill(rectmp, tmp);

            return tmp;
        }

        public void RenderBelowTerrain(Renderer r, ShadowBatch s, float ds, RECTANGLE renWindow, int offX, int offY)
        {
            offX = offX - renWindow.X1();
            offY = offY - renWindow.Y1();

            foreach (WEntity e in tmp)
            {
                e.RenderBelowTerrain(r, s, ds, e.Body().X1() + offX, e.Body().Y1() + offY);
            }
        }

        public void RenderAboveTerrain(Renderer r, ShadowBatch s, float ds, RECTANGLE renWindow, int offX, int offY)
        {
            renderables.Clear();
            map.Fill(renWindow, renderables);

            offX = offX - renWindow.X1();
            offY = offY - renWindow.Y1();
            tmp.Clear();

            while (renderables.HasMore())
            {
                WEntity e = renderables.PollGreatest();
                e.HandleFow();
                tmp.Add(e);
            }

            foreach (WEntity e in tmp)
            {
                e.RenderAboveTerrain(r, s, ds, e.Body().X1() + offX, e.Body().Y1() + offY);
            }
        }

        public LIST<WEntity> AllFast()
        {
            return fast;
        }

        public LIST<WEntity> AllSlow()
        {
            return slow;
        }
    }
}