using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.knowledge.library
{
    final class Constructor : Furnisher
    {
        public readonly FurnisherStat workers = new FurnisherStat(this)
        {
            Get = (AREA area, double fromItems) => fromItems,
            Format = (GText t, double value) => GFORMAT.i(t, (int)value)
        };

        public readonly FurnisherStat knowledge = new FurnisherStat(this)
        {
            Get = (AREA area, double fromItems) => fromItems,
            Format = (GText t, double value) => GFORMAT.f0(t, value * blue.data.knowledgePerStation),
            GetMulti = (AREA area, double[] fromItems) => base.Get(area, fromItems) * efficiency.Get(area, fromItems)
        };

        public readonly FurnisherStat efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);

        private readonly ROOM_LIBRARY blue;
        public readonly FurnisherItemTile ww;
        public readonly RoomSprite sStool;

        protected Constructor(ROOM_LIBRARY blue, RoomInitData init) : base(init, 2, 3, 88, 44)
        {
            this.blue = blue;

            Json sj = init.data().json("SPRITES");

            final RoomSprite1x1 sUsed = new RoomSprite1x1(sj, "WORK_USED_1x1");

            final RoomSprite sWork = new RoomSpriteCombo(sj, "TABLE_COMBO")
            {
                available = new RoomSprite1x1(sj, "WORK_UNUSED_1x1"),
                dec = new RoomSprite1x1(sj, "TABLE_DECOR_1x1"),

                RenderAbove = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) =>
                {
                    if (blue.is(it.tile()))
                    {
                        it.ranOffset(1, 0);
                        RoomSprite1x1 sp = null;
                        if (blue.job.used(it.tx(), it.ty()))
                        {
                            sp = sUsed;
                        }
                        else if (blue.consumption().ins().size() > 0 && blue.consumption().stored(blue.consumption().ins().get(0)).get(blue.get(it.tx(), it.ty())) > 0)
                        {
                            sp = available;
                        }

                        for (int i = 0; i < DIR.ORTHO.size(); i++)
                        {
                            if (SETT.ROOMS().fData.sprite.is(it.tx(), it.ty(), DIR.ORTHO.get(i), sStool))
                            {
                                if (sp != null)
                                    sp.render(r, s, i, it, degrade, false);
                                if ((it.ran() & 0b011) == 1)
                                {
                                    it.ranOffset(1, 0);
                                    dec.render(r, s, (i + 2) % 4, it, degrade, false);
                                }
                                break;
                            }
                        }
                    }
                }
            };

            sStool = new RoomSprite1x1(sj, "CHAIR_1X1")
            {
                Joins = (int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) => item.sprite(rx, ry) == sWork
            };

            final RoomSprite sShelf = new RoomSprite1x1(sj, "SHELF_1X1")
            {
                ontop = new RoomSprite1x1(sj, "SHELF_DECOR_1x1"),
                RenderAbove = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) =>
                {
                    if (blue.is(it.tile()))
                    {
                        int f = blue.data.usedD & 0x0FF;
                        if (f > (it.ran() & 0x0FF))
                        {
                            it.ranOffset(1, 0);
                            ontop.render(r, s, data, it, degrade, false);
                        }
                    }
                },
                Joins = (int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) =>
                {
                    if (item.width() > 2 && item.height() > 2)
                    {
                        if ((DIR.ORTHO.get(item.rotation).x() * d.x() != 0 || DIR.ORTHO.get(item.rotation).y() * d.y() != 0) && item.sprite(rx, ry) == this)
                            return true;
                        return false;
                    }
                    return DIR.ORTHO.get(item.rotation) == d;
                }
            };

            final RoomSprite sTorch = new RoomSprite1x1(sj, "TORCH_1x1")
            {
                RenderAbove = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) =>
                {
                    if (!SETT.ROOMS().fData.candle.is(it.tile()))
                        sUsed.renderRandom(r, s, it, it.ran(), degrade);
                }
            };
            final RoomSprite sNick = new RoomSprite1x1(sj, "DECOR_1x1");

            final FurnisherItemTile ss = new FurnisherItemTile(this, false, sShelf, AVAILABILITY.ROOM_SOLID, false);
            ww = new FurnisherItemTile(this, false, sWork, AVAILABILITY.ROOM_SOLID, false);
            final FurnisherItemTile st = new FurnisherItemTile(this, true, sStool, AVAILABILITY.AVOID_PASS, false);
            final FurnisherItemTile ca = new FurnisherItemTile(this, false, sTorch, AVAILABILITY.ROOM_SOLID, true);
            final FurnisherItemTile ni = new FurnisherItemTile(this, false, sNick, AVAILABILITY.ROOM_SOLID, false);

            final FurnisherItemTile __ = null;

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss, ss, ww, ca},
                {__, __, st, __},
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss, ss, ss, ww, ww, ca},
                {__, __, __, st, st, __},
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss, ss, ss, ss, ww, ww, ww, ca},
                {__, __, __, st, st, st, st, __},
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss, ss, ss, ss, ss, ww, ww, ww, ww, ni},
                {__, __, __, st, st, st, st, st, st, __},
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss, ss, ss, ss, ss, ss, ww, ww, ww, ww, ww, ca},
                {__, __, __, st, st, st, st, st, st, st, st, __},
            }, 8);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss, ss, ss, ss, ss, ss, ss, ww, ww, ww, ww, ww, ca},
                {__, __, __, st, st, st, st, st, st, st, st, st, __},
            }, 10);

            flush(1, 3);

            FurnisherItemTools.makeUnder(this, sj, "CARPET_COMBO");
        }

        public override bool UsesArea()
        {
            return true;
        }

        public override bool MustBeIndoors()
        {
            return true;
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new LibraryInstance(blue, area, init);
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override bool IsHeavy()
        {
            return true;
        }
    }
}