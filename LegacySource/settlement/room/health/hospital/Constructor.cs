using System;
using System.IO;
using init.sprite.game;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using util.rendering;

namespace settlement.room.health.hospital
{
    final class Constructor : Furnisher
    {
        private readonly ROOM_HOSPITAL blue;

        static readonly int CODE_S = 1;

        final FurnisherStat patients = new FurnisherStat.FurnisherStatI(this);
        final FurnisherStat workers = new FurnisherStat.FurnisherStatEmployees(this);

        protected Constructor(ROOM_HOSPITAL blue, RoomInitData init) : base(init, 1, 2, 88, 44)
        {
            this.blue = blue;

            Json js = init.data().json("SPRITES");

            RoomSprite1x1 sGrime = new RoomSprite1x1(js, "BED_1X1_GRIME");

            RoomSprite sbedA = new SBedSprite(sGrime, js, "BED_TOP_1X1", false);

            RoomSprite sbedB = new SBedSprite(sGrime, js, "BED_BOTTOM_1X1", true)
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (Bed.res2(it.tx(), it.ty()))
                        blue.indus.get(0).ins().get(1).resource.renderLaying(r, it.x(), it.y(), it.ran(), 1);
                }
            };

            RoomSprite stable = new RoomSpriteCombo(js, "TABLE_COMBO")
            {
                private readonly RoomSprite top = new RoomSprite1x1(js, "TABLE_ONTOP_1X1");

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (!SETT.ROOMS().fData.candle.is(it.tile()))
                        top.render(r, s, SETT.ROOMS().fData.spriteData2.get(it.tile()), it, degrade, false);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.getData(tx, ty, rx, ry, item, itemRan);
                };
            };

            FurnisherItemTile bb = new FurnisherItemTile(this, false, sbedA, AVAILABILITY.NOT_ACCESSIBLE, false);
            FurnisherItemTile ss = new FurnisherItemTile(this, true, sbedB, AVAILABILITY.NOT_ACCESSIBLE, false);
            ss.setData(CODE_S);

            FurnisherItemTile tt = new FurnisherItemTile(this, false, stable, AVAILABILITY.ROOM_SOLID, true);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {tt},
                {bb},
                {ss},
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {tt, tt},
                {bb, bb},
                {ss, ss},
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {tt, tt, tt},
                {bb, bb, bb},
                {ss, ss, ss},
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {tt, tt, tt, tt},
                {bb, bb, bb, bb},
                {ss, ss, ss, ss},
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {tt, tt, tt, tt, tt},
                {bb, bb, bb, bb, bb},
                {ss, ss, ss, ss, ss},
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss},
                {bb},
                {tt},
                {bb},
                {ss},
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss, ss},
                {bb, bb},
                {tt, tt},
                {bb, bb},
                {ss, ss},
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss, ss, ss},
                {bb, bb, bb},
                {tt, tt, tt},
                {bb, bb, bb},
                {ss, ss, ss},
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss, ss, ss, ss},
                {bb, bb, bb, bb},
                {tt, tt, tt, tt},
                {bb, bb, bb, bb},
                {ss, ss, ss, ss},
            }, 8);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss, ss, ss, ss, ss},
                {bb, bb, bb, bb, bb},
                {tt, tt, tt, tt, tt},
                {bb, bb, bb, bb, bb},
                {ss, ss, ss, ss, ss},
            }, 10);

            flush(1, 3);
        }

        public override bool usesArea()
        {
            return true;
        }

        public override bool mustBeIndoors()
        {
            return true;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new HospitalInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        private class SBedSprite : RoomSprite1xN
        {
            private Sheets made;
            private RoomSprite1x1 grime;

            public SBedSprite(RoomSprite1x1 grime, Json json, string key, bool master) : base(json, key, master)
            {
                made = new Sheets(SheetType.s1x1, json.json(key + "_UNMADE"));
                this.grime = grime;
            }

            public override Sheets sheet(RenderIterator it)
            {
                int data = SETT.ROOMS().fData.spriteData.get(it.tile());
                int x = it.tx() + offX(data);
                int y = it.ty() + offY(data);

                if (Bed.res1(x, y))
                {
                    return base.sheet(it);
                }
                return made;
            }

            public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
            {
                bool ret = base.render(r, s, data, it, degrade, isCandle);
                int x = it.tx() + offX(data);
                int y = it.ty() + offY(data);
                if (!Bed.made(x, y))
                {
                    grime.renderRandom(r, s, it, it.ran(), 0);
                }
                return ret;
            }
        }

        public override bool isHeavy()
        {
            return true;
        }
    }
}