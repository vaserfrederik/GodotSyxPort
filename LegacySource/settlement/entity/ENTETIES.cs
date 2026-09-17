using System;
using System.Collections.Generic;
using game;
using game.debug;
using init.constant;
using init.sprite;
using settlement.entity.animal;
using settlement.entity.humanoid;
using settlement.main;
using settlement.path;
using snake2d;
using snake2d.util.bit;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.sets;
using util.rendering;
using view.sett;
using view.sett.ui.minimap;
using view.tool;

namespace settlement.entity
{
    public class ENTETIES : SettResource
    {
        public const int MAX = 40000;
        public const int MM = MAX + 20000;
        private readonly Grid grid;
        private readonly ENTITY[] ents = new ENTITY[MM];
        private readonly IntegerStack freeIndexes = new IntegerStack(MM);
        private readonly Bits order = new Bits(0b0000_0000_0000_0011_1111_1111_1111_1111);
        private readonly Bits count = new Bits(0b0111_1111_1111_1100_0000_0000_0000_0000);
        private int eLastIndex = -1;
        private readonly Tree<ENTITY> renderables = new Tree<ENTITY>(8000)
        {
            protected override bool IsGreaterThan(ENTITY current, ENTITY cmp)
            {
                double d = current.Height() + current.physics.Z() - (cmp.Height() + cmp.physics.Z());
                return d > 0;
            }
        };
        private readonly ArrayList<ENTITY> temp = new ArrayList<ENTITY>(3000);
        private readonly Rec lWin = new Rec();

        public readonly MAP_BOOLEAN submerged = new SubmergedMap();

        public ENTETIES() : base("ENTETIES", true)
        {
            grid = new Grid();

            freeIndexes.Clear();
            for (int i = MM - 1; i >= 0; i--)
            {
                freeIndexes.Push(i);
            }
            eLastIndex = 0;

            new AvailabilityListener
            {
                protected override void Changed(int tx, int ty, AVAILABILITY a, AVAILABILITY old, bool isMax)
                {
                    if (isMax)
                    {
                        // Handle max capacity logic here
                    }
                    else if (a != old)
                    {
                        // Handle change in availability logic here
                    }
                }
            };

            remove = new PlacableSimple("Remove Entity", "")
            {
                public override void Place(int x, int y)
                {
                    ENTITY e = ENTITIES().GetAtPoint(x, y);
                    if (e != null)
                        e.helloMyNameIsInigoMontoyaYouKilledMyFatherPrepareToDie();
                }

                public override CharSequence IsPlacable(int x, int y)
                {
                    return (ENTITIES().GetAtPoint(x, y) != null) ? null : E;
                }
            };
        }

        public bool Save(string p)
        {
            throw new NotImplementedException();
        }

        public void Remove(ENTITY e)
        {
            if (e.handlerId == -1)
                throw new Exception();

            e.removeAction();
            grid.Remove(e);
            ents[order.Get(e.handlerId)] = null;
            freeIndexes.Push(e.handlerId);
            e.handlerId = -1;
        }

        public void Update(double ds, Profiler profiler)
        {
            int s = k == 0 ? 0 : eLastIndex / 2;
            int l = k == 0 ? eLastIndex : eLastIndex / 2;

            for (int i = s; i <= l; i++)
            {
                ENTITY e = ents[i];
                if (e == null)
                    continue;
                e.Update(ds);
                if (e.handlerId == -1)
                    continue;
                grid.Move(e);
            }
        }

        public void Move(ENTITY e)
        {
            grid.Move(e);
        }

        public ENTITY GetAtPoint(COORDINATE coo)
        {
            return GetAtPoint(coo.x(), coo.y());
        }

        public ENTITY GetAtPoint(int x, int y)
        {
            temp.Clear();
            grid.Fill(x, y, temp);
            if (!temp.IsEmpty())
                return temp.Get(0);
            return null;
        }

        public IEnumerable<ENTITY> GetAtPointL(int x, int y)
        {
            temp.Clear();
            grid.Fill(x, y, temp);
            return temp;
        }

        public ENTITY GetArroundPoint(int x, int y)
        {
            temp.Clear();

            Rec.TEMP.SetDim(C.TILE_SIZE);
            Rec.TEMP.MoveC(x, y);
            grid.Fill(Rec.TEMP, temp);

            ENTITY res = null;
            while (!temp.IsEmpty())
            {
                ENTITY candidate = temp.RemoveLast();
                if (res == null || candidate.Body().GetDistance(Rec.TEMP) < res.Body().GetDistance(Rec.TEMP))
                    res = candidate;
            }

            return res;
        }

        public LIST<ENTITY> GetArroundPoint(int x, int y, int size)
        {
            temp.Clear();

            Rec.TEMP.SetDim(size);
            Rec.TEMP.MoveC(x, y);
            grid.Fill(Rec.TEMP, temp);

            return temp;
        }

        public IEnumerable<ENTITY> GetAtTile(int tx, int ty)
        {
            temp.Clear();
            grid.FillTile(tx, ty, temp);
            return temp;
        }

        public ENTITY GetAtTileSingle(int tx, int ty)
        {
            return grid.GetFirst(tx, ty);
        }

        public bool TileIsClear(int tx, int ty)
        {
            temp.Clear();
            grid.Fill(tx, ty, temp);
            foreach (ENTITY e in temp)
            {
                if (e.physics.GetMass() != 0)
                    return false;
            }
            return true;
        }

        public bool HasAtTile(ENTITY asker, int tx, int ty)
        {
            ENTITY e = grid.GetFirst(tx, ty);
            if (e != null && e == asker)
                e = e.next;
            return e != null;
        }

        public bool HasAtTileHigher(ENTITY asker, int tx, int ty)
        {
            ENTITY e = grid.GetFirst(tx, ty);
            while (e != null)
            {
                if (e != asker && e.id() > asker.id())
                    return true;
                e = e.next;
            }
            return false;
        }

        public int AmountAtTile(int tx, int ty)
        {
            int am = 0;
            ENTITY e = grid.GetFirst(tx, ty);
            if (e != null && am < 10)
            {
                e = e.next;
                am++;
            }
            return am;
        }

        public bool HasAtTile(int tx, int ty)
        {
            ENTITY e = grid.GetFirst(tx, ty);
            return e != null;
        }

        public LIST<ENTITY> GetInProximity(ENTITY e, int radius)
        {
            temp.Clear();
            grid.Fill(e, radius, temp);
            return temp;
        }

        public ENTITY GetByID(int id)
        {
            if (id < 0)
                return null;
            int oi = order.Get(id);
            if (oi > ents.Length)
                return null;
            ENTITY t = ents[oi];
            if (t == null)
                return null;
            if (t.isRemoved())
                return null;
            if (id != t.handlerId)
                return null;
            return t;
        }

        public ENTITY GetByIndex(int id)
        {
            return ents[id];
        }

        public int Size()
        {
            return MAX - freeIndexes.Size();
        }

        public bool IsMax()
        {
            return freeIndexes.IsEmpty();
        }

        public void Fill(Coo coo, ADDABLE<ENTITY> result)
        {
            grid.Fill(coo.x(), coo.y(), result);
        }

        public void Fill(RECTANGLE area, ADDABLE<ENTITY> result)
        {
            grid.Fill(area, result);
        }

        public LIST<ENTITY> Fill(RECTANGLE pixels)
        {
            temp.Clear();
            grid.Fill(pixels, temp);
            return temp;
        }

        public LIST<ENTITY> FillTiles(RECTANGLE tiles)
        {
            lWin.MoveX1Y1(tiles.x1() << C.T_SCROLL, tiles.y1() << C.T_SCROLL);
            lWin.SetWidth(tiles.width() << C.T_SCROLL).SetHeight(tiles.height() << C.T_SCROLL);
            return Fill(lWin);
        }

        public LIST<ENTITY> FillTiles(int tx, int ty, int w, int h)
        {
            lWin.MoveX1Y1(tx << C.T_SCROLL, ty << C.T_SCROLL);
            lWin.SetWidth((w) << C.T_SCROLL).SetHeight(h << C.T_SCROLL);
            return Fill(lWin);
        }

        private int k = 0;
        private double lastDs = 0;

        public PLACABLE remove { get; private set; }
    }
}