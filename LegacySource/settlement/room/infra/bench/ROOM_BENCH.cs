using System;
using System.Collections.Generic;
using System.IO;
using settlement.main;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using util.gui.misc;
using util.info;
using util.rendering;
using util.text;
using view.sett.ui.room;

namespace settlement.room.infra.bench
{
    public sealed class ROOM_BENCH : RoomBlueprintImp, RoomFinderHaser
    {
        private readonly MConstructor constructor;
        private readonly Instance instance;

        public readonly SFinderRoomService finder = new SFinderRoomService("Bench")
        {
            private int x, y;

            private readonly FSERVICE s = new FSERVICE()
            {
                public override int y() => y;

                public override int x() => x;

                public override bool findableReservedIs() => SETT.ROOMS().fData.spriteData2.get(x, y) == 1;

                public override bool findableReservedCanBe() => SETT.ROOMS().fData.spriteData2.get(x, y) == 0;

                public override void findableReserveCancel()
                {
                    if (findableReservedIs())
                    {
                        finder.report(x, y, 1);
                    }
                    SETT.ROOMS().fData.spriteData2.set(x, y, 0);
                }

                public override void findableReserve()
                {
                    if (!findableReservedIs())
                    {
                        finder.report(x, y, -1);
                    }
                    SETT.ROOMS().fData.spriteData2.set(x, y, 1);
                }

                public override void consume() => findableReserveCancel();
            };

            public override FSERVICE get(int tx, int ty) => ROOM_BENCH.this.is(tx, ty) ? (s.x = tx, s.y = ty, s) : null;
        };

        public ROOM_BENCH(RoomInitData init, RoomCategorySub cat) : base(init, 0, "_BENCH", cat)
        {
            this.constructor = new MConstructor(this, init);
            this.instance = new Instance(init.m, this);
        }

        protected override void save(FilePutter f) { }

        protected override void load(FileGetter f) { }

        protected override void clear() { }

        public override Room get(int tx, int ty) => ROOMS().map.get(tx, ty) == instance ? instance : null;

        protected override void update(double ds) { }

        public override SFinderRoomService service(int tx, int ty) => finder;

        public override Furnisher constructor() => constructor;

        public override void appendView(List<UIRoomModule> mm)
        {
            mm.Add(new UIRoomModule()
            {
                public override void hover(GBox box, Room room, int rx, int ry)
                {
                    if (upgrades().max() > 0)
                    {
                        box.NL();
                        box.text(Dic.¤¤Upgrade);
                        box.tab(6);
                        box.add(GFORMAT.iofkInv(box.text(), room.upgrade(rx, ry), upgrades().max()));
                        box.NL();
                    }
                }
            });
        }

        private sealed class Instance : RoomSingleton
        {
            private static readonly long serialVersionUID = 1L;

            Instance(ROOMS m, RoomBlueprint p) : base(m, p) { }

            protected override object readResolve() => blueprintI().instance;

            public override ROOM_BENCH blueprintI() => (ROOM_BENCH)blueprint();

            protected override void addAction(ROOMA ins)
            {
                foreach (COORDINATE c in ins.body())
                {
                    if (ins.is(c))
                    {
                        SETT.ROOMS().fData.spriteData2.set(c.x(), c.y(), 0);
                        blueprintI().finder.report(blueprintI().finder.get(c), 1);
                    }
                }
            }

            protected override void removeAction(ROOMA ins)
            {
                foreach (COORDINATE c in ins.body())
                {
                    if (ins.is(c) && SETT.ROOMS().fData.spriteData2.get(c) == 0)
                    {
                        blueprintI().finder.report(blueprintI().finder.get(c), -1);
                    }
                }
                base.removeAction(ins);
            }

            public override int upgrade(int tx, int ty) => CLAMP.i(SETT.ROOMS().extraBit.get(mX(tx, ty), mY(tx, ty)), 0, blueprintI().upgrades().max());

            public override void upgradeSet(int tx, int ty, int upgrade)
            {
                int up = CLAMP.i(upgrade, 0, blueprintI().upgrades().max());
                SETT.ROOMS().extraBit.set(tx, ty, up);
                ROOMA a = SETT.ROOMS().map.rooma.get(tx, ty);
                foreach (COORDINATE c in a.body())
                {
                    if (a.is(c))
                    {
                        SETT.MAINTENANCE().setChanged(c.x(), c.y());
                        constructor().floor(up).placeFixed(c.x(), c.y());
                    }
                }
            }
        }

        private class MConstructor : Furnisher
        {
            private readonly ROOM_BENCH blue;

            MConstructor(ROOM_BENCH blue, RoomInitData init) : base(init, 1, 0, 88, 44)
            {
                this.blue = blue;

                Json sData = init.data().json("SPRITES");
                RoomSprite ssmall = new RoomSprite1x1(sData, "BENCH_1X1")
                {
                    public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                    {
                        DIR rot = rot(data);
                        data &= ~0x03;
                        data |= rot.perpendicular().orthoID();
                        base.render(r, s, data, it, degrade, isCandle);
                        return false;
                    }

                    protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) => d == DIR.ORTHO.get(item.rotation);

                    public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
                    {
                        SheetType.s1x1.renderOverlay(
                            x, y, r, item.get(rx, ry).availability, 
                            0, rotates ? data : -1, true);
                    }
                };

                FurnisherItemTile tt = new FurnisherItemTile(
                    this,
                    true,
                    ssmall,
                    AVAILABILITY.AVOID_LIKE_FUCK,
                    false);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { tt },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { tt, tt },
                }, 2);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { tt, tt, tt },
                }, 3);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { tt, tt, tt, tt },
                }, 4);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { tt, tt, tt, tt, tt },
                }, 5);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { tt, tt, tt, tt, tt, tt },
                }, 6);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { tt, tt, tt, tt, tt, tt, tt },
                }, 7);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { tt, tt, tt, tt, tt, tt, tt, tt },
                }, 8);

                flush(3);
            }

            public override bool usesArea() => false;

            public override bool mustBeIndoors() => false;

            public override Room create(TmpArea area, RoomInit init) => blue.instance.place(area);

            public override RoomBlueprintImp blue() => blue;
        }

        public DIR benchDir(int tx, int ty, DIR d)
        {
            FurnisherItem it = SETT.ROOMS().fData.item.get(tx, ty);
            return it == null ? d : DIR.ORTHO.get(it.rotation);
        }

        public override SFinderFindable finder() => finder;

        public override int radius() => 64;

        public override bool registersEnvironment() => true;
    }
}