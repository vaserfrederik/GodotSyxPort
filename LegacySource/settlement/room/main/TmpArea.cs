using System;
using System.Collections.Generic;
using System.IO;
using settlement.main;
using settlement.maintenance;
using settlement.path;
using settlement.path.finders;
using settlement.room.main.construction;
using snake2d;
using util;
using util.rendering;

namespace settlement.room.main
{
    public sealed class TmpArea : MAP_SETTER, ROOMA
    {
        private readonly RoomBlueprint b = new RoomBlueprint("_TMPAREA")
        {
            protected override void update(double ds)
            {
                if (lastUser != null)
                    error();
            }

            public override SFinderFindable service(int tx, int ty)
            {
                return null;
            }

            protected override void save(FilePutter file)
            {
                // TODO Auto-generated method stub
            }

            public override COLOR miniC(int tx, int ty)
            {
                return null;
            }

            protected override void load(FileGetter file)
            {
                clear();
            }

            protected override void clear()
            {
                TmpArea.this.clear();
            }

            public override COLOR miniCPimped(ColorImp origional, int tx, int ty, bool northern, bool southern)
            {
                return origional;
            }
        };

        private readonly Instance ins;
        private object lastUser;
        private StackTraceElement[] els = new StackTraceElement[0];
        private static readonly RoomAreaWrapper wrap = new RoomAreaWrapper();
        private Furnisher cons;
        private bool removeFloor;

        public TmpArea(ROOMS m)
        {
            ins = new Instance(m, b);
        }

        public void init(object user)
        {
            if (lastUser != null)
                error();
            lastUser = user;
            cons = null;
            removeFloor = true;
        }

        public void setDontRemoveFloor()
        {
            removeFloor = false;
        }

        public void setRemoveFloor()
        {
            removeFloor = true;
        }

        public void set(Room o, int rx, int ry)
        {
            ROOMA a = wrap.init(o, rx, ry);
            cons = o.constructor();
            GUTIL.coos().set(0);

            foreach (COORDINATE c in a.body())
            {
                if (a.is(c))
                {
                    GUTIL.coos().get().set(c);
                    GUTIL.coos().inc();
                }
            }

            int k = GUTIL.coos().getI();

            for (int i = 0; i < k; i++)
            {
                COORDINATE c = GUTIL.coos().set(i);
                ROOMS().map.replace(c.x() + c.y() * SETT.TWIDTH, o, ins);
                ins.setP(c.x(), c.y());
            }
            wrap.done();
        }

        public MAP_SETTER set(int tile)
        {
            ins.set(tile % TWIDTH, tile / TWIDTH);
            return this;
        }

        public MAP_SETTER set(int tx, int ty)
        {
            ins.set(tx, ty);
            return this;
        }

        public void replaceAndClear(Room o)
        {
            foreach (COORDINATE c in ins.body)
            {
                if (ins.is(c))
                {
                    ROOMS().map.replace(c.x() + c.y() * SETT.TWIDTH, ins, o);
                }
            }
            clear();
        }

        public void clear()
        {
            SETT.ROOMS().fData.clear(mx(), my(), ins);
            if (lastUser != null)
            {
                foreach (COORDINATE c in ins.body)
                {
                    if (ins.is(c))
                    {
                        ROOMS().map.clear(c.x() + c.y() * SETT.TWIDTH, ins);
                        if (removeFloor || dFloored.is(ROOMS().data.get(c.x(), c.y()), 1))
                            SETT.FLOOR().clearer.clear(c);
                    }
                }
            }

            lastUser = null;
            ins.area = 0;
            ins.body.setDim(0).moveX1Y1(-1, -1);
            ins.mx = -1;
            ins.my = -1;
        }

        public void clearAndUpdate()
        {
            SETT.ROOMS().fData.clear(mx(), my(), ins);

            if (lastUser != null)
            {
                foreach (COORDINATE c in ins.body)
                {
                    if (ins.is(c))
                    {
                        ROOMS().map.clear(c.x() + c.y() * SETT.TWIDTH, ins);
                        if (removeFloor || dFloored.is(ROOMS().data.get(c.x(), c.y()), 1))
                            SETT.FLOOR().clearer.clear(c);
                    }
                }
            }

            lastUser = null;
            ins.area = 0;
            ins.body.setDim(0).moveX1Y1(-1, -1);
            ins.mx = -1;
            ins.my = -1;
        }

        public RoomInstanceImp room()
        {
            return ins;
        }

        public int mx()
        {
            return ins.mx;
        }

        public int my()
        {
            return ins.my;
        }

        private void error()
        {
            if (lastUser != null)
            {
                foreach (StackTraceElement e in els)
                {
                    Console.Error.WriteLine(e);
                }
                throw new Exception("In use by: " + lastUser);
            }
            throw new Exception();
        }

        private class Instance : Room.RoomInstanceImp
        {
            private int area = 0;
            private Rec body = new Rec();
            private short mx, my;

            public Instance(ROOMS m, RoomBlueprint p) : base(m, p, true)
            {
            }

            private void setP(int tx, int ty)
            {
                if (area == 0)
                {
                    mx = (short)tx;
                    my = (short)ty;
                    body.setDim(1).moveX1Y1(tx, ty);
                }
                else
                {
                    body.unify(tx, ty);
                }
                area++;
            }

            private void set(int tx, int ty)
            {
                setP(tx, ty);
                ROOMS().map.set(tx + ty * SETT.TWIDTH, this);
            }

            public override int area()
            {
                return area;
            }

            public override RECTANGLE body()
            {
                return body;
            }

            public override bool is(int tile)
            {
                return SETT.ROOMS().map.indexGetter.get(tile) == roomI;
            }

            protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
            {
                SETT.ROOMS().tmpArea.error();
                return false;
            }

            public override string name(int tx, int ty)
            {
                return "should never be";
            }

            protected override AVAILABILITY getAvailability(int tile)
            {
                return null;
            }

            public override bool destroyTileCan(int tx, int ty)
            {
                SETT.ROOMS().tmpArea.error();
                return false;
            }

            public override ROOM_DEGRADER degrader(int tx, int ty)
            {
                return null;
            }

            public override int mX()
            {
                return mx;
            }

            public override int mY()
            {
                return my;
            }

            public override SPRITE icon()
            {
                SETT.ROOMS().tmpArea.error();
                return null;
            }

            public override int resAmount(int ri, int upgrade)
            {
                return 0;
            }

            public override Furnisher constructor()
            {
                return SETT.ROOMS().tmpArea.cons;
            }

            public override void destroyTile(int tx, int ty)
            {
                SETT.ROOMS().tmpArea.error();
            }

            public override TmpArea remove(int tx, int ty, bool scatter, object user, bool forced)
            {
                SETT.ROOMS().tmpArea.error();
                return null;
            }
        }

        public RECTANGLE body()
        {
            return ins.body();
        }

        public bool is(int tile)
        {
            return ins.is(tile);
        }

        public bool is(int tx, int ty)
        {
            return ins.is(tx, ty);
        }

        public int area()
        {
            return ins.area;
        }

        public int index()
        {
            return ins.index();
        }

        public int mX()
        {
            return ins.mX();
        }

        public int mY()
        {
            return ins.mY();
        }
    }
}