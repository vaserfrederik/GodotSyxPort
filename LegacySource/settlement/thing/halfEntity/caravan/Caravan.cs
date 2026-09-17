using System;
using System.IO;
using init.constant;
using init.resources;
using init.trade;
using settlement.main;
using settlement.path.path;
using settlement.thing.halfEntity;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using util.gui.misc;
using util.rendering;
using util.text;

namespace settlement.thing.halfEntity.caravan
{
    public class Caravan : HalfEntity
    {
        private static readonly int TRAIL = 5;
        private static readonly int LOAD_PER_ANIMAL = 128;

        public static int MAX_LOAD = LOAD_PER_ANIMAL * (TRAIL - 1);

        public Caravan() : base(C.TILE_SIZE, C.TILE_SIZE)
        {
        }

        private readonly SPath path = new SPath();
        private RESOURCE res;
        private short amountCarried;
        private short reserved;
        private short reservedGlobally;
        private short tmp;
        private short itype;
        private short state;
        private float movement;
        private Type type;

        private int prev;
        private bool returning;
        private readonly byte ran = (byte)RND.rInt();

        private static readonly int[] animation = new int[]
        {
            0, 1, 2, 1, 0, 3, 4, 3
        };

        protected override void save(FilePutter file)
        {
            path.save(file);
            RESOURCES.map().saver().save(res, file);
            file.i(amountCarried);
            file.i(reserved);
            file.i(reservedGlobally);
            file.d(movement);
            file.i(type.index);
            file.i(prev);
            file.s(tmp);
            file.s(itype);
            file.bool(returning);
        }

        public int carried()
        {
            return amountCarried;
        }

        public RESOURCE res()
        {
            return res;
        }

        Type type()
        {
            return type;
        }

        public TRADE_TYPE tType()
        {
            return TRADE_TYPE.all.get(itype);
        }

        protected override HalfEntity load(FileGetter file)
        {
            path.load(file);
            res = RESOURCES.map().loader().load(file);
            amountCarried = (short)file.i();
            reserved = (short)file.i();
            reservedGlobally = (short)file.i();
            movement = (float)file.d();
            type = Type.all.get(file.i());
            prev = file.i();
            tmp = file.s();
            itype = file.s();
            returning = file.bool();

            if (res == null)
            {
                res = RESOURCES.WOOD();
                amountCarried = 0;
                reserved = 0;
                reservedGlobally = 0;
            }

            type().load(this);

            return this;
        }

        bool init(int tx, int ty, Type type, RESOURCE res, int amount, TRADE_TYPE itype)
        {
            body().moveC(tx * C.TILE_SIZE + C.TILE_SIZEH, ty * C.TILE_SIZE + C.TILE_SIZEH);
            this.res = res;
            this.amountCarried = 0;
            movement = 1;
            reserved = 0;
            reservedGlobally = 0;
            this.itype = (short)itype.index;
            this.type = type;
            this.path.clear();
            this.prev = 0;
            returning = false;
            if (this.type.init(this, amount))
            {
                add();
                if (added())
                    return true;
            }
            return false;
        }

        protected override void update(double ds)
        {
            movement += ds * 2.0;
            if (!path.isSuccessful())
            {
                remove();
                return;
            }

            if (movement >= 1)
            {
                if (path.isDest())
                {
                    if (!type.update(this, ds))
                    {
                        if (added())
                            remove();
                        return;
                    }
                    movement -= 1;
                }
                else if (movement >= length())
                {
                    movement -= length();
                    move();
                }
            }
        }

        void move()
        {
            if (!path.isSuccessful())
            {
                if (added())
                    remove();
                return;
            }
            if (path.setNext())
            {
                int dx = ctx();
                int dy = cty();
                if (!path.isDest())
                {
                    body().moveC(path.getSettCX(), path.getSettCY());
                    dx -= ctx();
                    dy -= cty();
                    prevPush(dx, dy);
                }
            }
        }

        public override void render(Renderer r, ShadowBatch s, float ds, int x, int y)
        {
            int t = 0;
            double d = 1;
            if (!path.isDest())
            {
                d = movement / length();
                t += 8 * animation[(int)((movement * animation.Length)) % animation.Length];
            }

            int cx = x + body().width() / 2;
            int cy = y + body().height() / 2;
            int max = (int)Math.Ceiling((double)reservedGlobally / LOAD_PER_ANIMAL);
            int maxload = LOAD_PER_ANIMAL;
            int am = (int)Math.Ceiling(maxload * (double)amountCarried / LOAD_PER_ANIMAL);
            for (int i = 0; i < max + 1; i++)
            {
                int dx = prevX(i);
                int dy = prevY(i);
                int dir = DIR.get(-dx, -dy).id() & 0x07;
                if (dx != 0 || dy != 0)
                {
                    cx += dx * C.TILE_SIZE;
                    cy += dy * C.TILE_SIZE;
                    int px = (int)(cx + -C.TILE_SIZE * dx * d);
                    int py = (int)(cy + -C.TILE_SIZE * dy * d);
                    if (i == 0)
                    {
                        SETT.THINGS().sprites.caravan.renderC(r, t + dir, px, py);
                        s.setHeight(4);
                        SETT.THINGS().sprites.caravan.renderC(s, t + dir, px, py);
                    }
                    else
                    {
                        int a = CLAMP.i(am, 0, maxload);
                        SETT.ANIMALS().renderCaravan(r, s, d, px, py, res, (int)Math.Ceiling(a / 9.0), false, dir, ran);
                        am -= a;
                    }
                }
            }
        }

        private double length()
        {
            if (path.length() == 0)
                return 0;
            if ((path.x() - ctx()) * (path.y() - cty()) != 0)
                return C.SQR2;
            return 1.0;
        }

        protected override void removeAction()
        {
            type.cancel(this, true);
        }

        protected override Factory<HalfEntity> constructor()
        {
            return SETT.HALFENTS().caravans;
        }

        private int prevX(int back)
        {
            int am = (prev >> (back * 4)) & 0b011;
            if (am == 0b011)
                return -1;
            return am;
        }

        private void prevPush(int dx, int dy)
        {
            dx &= 0b011;
            dy &= 0b011;
            prev = prev << 4;
            prev |= dy << 2;
            prev |= dx;
        }

        private int prevY(int back)
        {
            int am = (prev >> (back * 4) + 2) & 0b011;
            if (am == 0b011)
                return -1;
            return am;
        }

        private static readonly CharSequence ¤¤Caravan = "¤Caravan";
        static
        {
            D.ts(Caravan);
        }

        public override void hoverInfo(GBox box)
        {
            box.title(¤¤Caravan);
            type.hoverInfo(box, this);
        }
    }
}