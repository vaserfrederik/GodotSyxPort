using System;
using System.IO;
using Newtonsoft.Json;
using settlement.environment;
using settlement.main;
using settlement.maintenance;
using settlement.path;
using settlement.room.main;
using settlement.room.main.util;
using settlement.room.sprite;
using settlement.room.water;
using util;
using util.rendering;
using snake2d;

namespace settlement.room.water
{
    public class Drain : RoomBlueprintImp, ROOM_PUMPABLE
    {
        public readonly Constructor constructor;
        public readonly DrainInstance instance;

        public Drain(RoomInitData init, RoomCategorySub cat) : base(init, 0, "_WATERDRAIN", cat)
        {
            this.instance = new DrainInstance(init.m, this);
            constructor = new Constructor(init);
        }

        public override SFinderFindable service(int tx, int ty)
        {
            return null;
        }

        public override Furnisher constructor()
        {
            return constructor;
        }

        protected override void save(FilePutter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void load(FileGetter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void clear()
        {
            // TODO Auto-generated method stub
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        private sealed class Constructor : Furnisher
        {
            protected Constructor(RoomInitData init) : base(init, 1, 0)
            {
                Json jj = init.data().json("SPRITES");
                final RoomSprite dd = new RoomSprite1x1(jj, "DRAIN_1X1");

                RoomSprite sp = new WSprite.RSprite(((Drain)RoomBlueprintImp.this), instance.pump, false)
                {
                    public override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                    {
                        dd.render(r, s, data, it, degrade, false);
                        base.renderBelow(r, s, data, it, degrade);
                    }
                };

                new FurnisherItem(new FurnisherItemTile[][] {
                    {new FurnisherItemTile(this, false, sp, AVAILABILITY.AVOID_PASS, false)}
                }, 1);

                flush(1, 0);
            }

            public override bool usesArea()
            {
                return false;
            }

            public override bool mustBeIndoors()
            {
                return false;
            }

            public override Room create(TmpArea area, RoomInit init)
            {
                int tx = area.mX();
                int ty = area.my();
                instance.place(area);
                SETT.ROOMS().fData.spriteData2.set(tx, ty, 1);
                foreach (DIR d in DIR.ORTHO)
                {
                    if (((Drain)RoomBlueprintImp.this).is(tx, ty, d))
                    {
                        SETT.ROOMS().fData.spriteData2.set(tx, ty, d, 1);
                    }
                }

                return SETT.ROOMS().map.get(tx, ty);
            }

            public override void renderExtra(SPRITE_RENDERER r, int x, int y, int tx, int ty, int rx, int ry, FurnisherItem item)
            {
                int i = 0;
                while (GUTIL.circle().radius(i) < DrainInstance.radius - 2)
                {
                    int dx = GUTIL.circle().get(i).x() + tx;
                    int dy = GUTIL.circle().get(i).y() + ty;
                    int rrx = x + GUTIL.circle().get(i).x() * C.TILE_SIZE;
                    int rry = y + GUTIL.circle().get(i).y() * C.TILE_SIZE;
                    if (SETT.ROOMS().WATER.pumpable.get(dx, dy) == instance.pump)
                    {
                        SPRITES.cons().BIG.dashed.render(r, 0, rrx, rry);
                    }
                    else if (GUTIL.circle().radius(i) == DrainInstance.radius - 3)
                    {
                        SPRITES.cons().BIG.outline.render(r, 0, rrx, rry);
                    }

                    i++;
                }
                base.renderExtra(r, x, y, tx, ty, rx, ry, item);
            }

            public override bool envValue(SettEnv e)
            {
                return e == SETT.ENV().map.WATER_SWEET;
            }

            public override bool envValue(SettEnv e, SettEnvValue v, int tx, int ty)
            {
                if (blue().is(tx, ty) && SETT.ROOMS().data.get(tx, ty) != 0 && e == SETT.ENV().map.WATER_SWEET)
                {
                    v.value = 1;
                    v.radius = 1;
                    return true;
                }
                return false;
            }

            public override RoomBlueprintImp blue()
            {
                return (Drain)RoomBlueprintImp.this;
            }
        }

        private sealed class DrainInstance : RoomSingleton
        {
            private static int radius = 10;
            private readonly Pump pump = new Pump();
            private static readonly long serialVersionUID = 1L;
            private static readonly RoomAreaWrapper wrap = new RoomAreaWrapper();

            public DrainInstance(ROOMS m, RoomBlueprint p) : base(m, p)
            {
            }

            protected override object readResolve()
            {
                return blueprintI().instance;
            }

            public override Drain blueprintI()
            {
                return (Drain)blueprint();
            }

            public override ROOM_DEGRADER degrader(int tx, int ty)
            {
                return null;
            }

            public override void updateTileDay(int tx, int ty)
            {
            }

            protected override void removeAction(ROOMA ins)
            {
                base.removeAction(ins);
                RoomPumpable.reportChange(ins.mX(), ins.mY(), radius);
            }

            protected override void addAction(ROOMA ins)
            {
                base.removeAction(ins);
                RoomPumpable.reportChange(ins.mX(), ins.mY(), radius);
            }

            private class Pump : RoomPumpable
            {
                protected override void drain(int tx, int ty)
                {
                    wrap.init(this, tx, ty);
                    SETT.ROOMS().data.set(wrap.area(), tx, ty, 0);
                    wrap.done();
                }

                protected override void pump(int tx, int ty, DIR d, int dirmask)
                {
                    wrap.init(this, tx, ty);
                    int da = SETT.ROOMS().data.get(tx, ty);
                    da |= d.mask();
                    SETT.ROOMS().data.set(wrap.area(), tx, ty, da);
                    wrap.done();
                }

                protected override int dirmask(int tx, int ty)
                {
                    return SETT.ROOMS().data.get(tx, ty) & 0x0F;
                }

                protected override int radius()
                {
                    return radius;
                }

                protected override bool pumpsTo(int fromX, int fromY, int tx, int ty)
                {
                    return true;
                }

                public override double irrigation(int tx, int ty)
                {
                    return SETT.ROOMS().data.get(tx, ty) == 0 ? 0 : 1;
                }
            }
        }

        public override RoomPumpable pumpable(int tx, int ty)
        {
            if (is(tx, ty))
                return instance.pump;
            return null;
        }
    }
}