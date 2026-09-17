using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Room.Service.Food.Canteen
{
    public class Constructor : Furnisher
    {
        public FurnisherStat Guests { get; }
        public FurnisherStat Workers { get; }
        public FurnisherStat Tables { get; }
        private readonly ROOM_CANTEEN Blue;
        private readonly RoomSprite SPlate;

        protected Constructor(ROOM_CANTEEN blue, RoomInitData init) : base(init, 2, 3, 88, 44)
        {
            this.Blue = blue;
            Guests = new FurnisherStat.FurnisherStatServices(this, blue, 1);
            Workers = new FurnisherStat.FurnisherStatEmployees(this, 1);
            Tables = new FurnisherStat.FurnisherStatRelative(this, Guests);

            Json sp = init.data().json("SPRITES");

            SPlate = new RoomSprite1x1(sp, "PLATE_1X1");

            final RoomSprite spriteOven = new RoomSpriteCombo(sp, "TABLE_COMBO")
            {
                final RoomSprite1x1 beneath = new RoomSprite1x1(sp, "OVEN_BENEATH_1X1");
                final RoomSprite1x1 beneath_used = new RoomSprite1x1(sp, "OVEN_BENEATH_USED_1X1");
                final RoomSprite1x1 oven = new RoomSprite1x1(sp, "OVEN_1X1")
                {
                    protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        return item.get(rx, ry) == null;
                    }
                };

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    super.render(r, s, data, it, degrade, isCandle);
                    beneath.render(r, s, getData2(it), it, degrade, isCandle);
                    CanteenInstance i = blue.getter.get(it.tile());
                    if (i == null)
                        return false;
                    SWork o = blue.job.get(it.tx(), it.ty());
                    if (o == null)
                        return false;
                    if (o.hasCoal())
                    {
                        beneath_used.render(r, s, getData2(it), it, degrade, isCandle);
                    }
                    if (o.res() != null)
                    {
                        o.res().resource.renderOne(r, it.x(), it.y(), it.ran());
                    }
                    return false;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    oven.render(r, s, getData2(it), it, degrade, false);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return oven.getData(tx, ty, rx, ry, item, itemRan);
                }
            };

            final RoomSprite spriteFood = new RoomSpriteCombo(sp, "TABLE_COMBO")
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    super.renderAbove(r, s, data, it, degrade);
                    beneath.render(r, s, getData2(it), it, degrade, isCandle);
                    CanteenInstance i = blue.getter.get(it.tile());
                    if (i == null)
                        return false;
                    SWork o = blue.job.get(it.tx(), it.ty());
                    if (o == null)
                        return false;
                    if (o.hasCoal())
                    {
                        beneath_used.render(r, s, getData2(it), it, degrade, isCandle);
                    }
                    if (o.res() != null)
                    {
                        o.res().resource.renderOne(r, it.x(), it.y(), it.ran());
                    }
                    return false;
                }
            };

            final RoomSprite spriteMisc = new RoomSpriteCombo(sp, "MISC_COMBO")
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    super.renderAbove(r, s, data, it, degrade);
                    beneath.render(r, s, getData2(it), it, degrade, isCandle);
                    CanteenInstance i = blue.getter.get(it.tile());
                    if (i == null)
                        return false;
                    SWork o = blue.job.get(it.tx(), it.ty());
                    if (o == null)
                        return false;
                    if (o.hasCoal())
                    {
                        beneath_used.render(r, s, getData2(it), it, degrade, isCandle);
                    }
                    if (o.res() != null)
                    {
                        o.res().resource.renderOne(r, it.x(), it.y(), it.ran());
                    }
                    return false;
                }
            };

            new FurnisherItem(new FurnisherItemTile[][] {
                {mm, jj, mm},
                {se, ss, se},
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][] {
                {mm, jj, jj, mm},
                {se, ss, ss, se},
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][] {
                {mm, jj, jj, jj, mm},
                {se, ss, ss, ss, se},
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][] {
                {mm, jj, jj, jj, jj, mm},
                {se, ss, ss, ss, ss, se},
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][] {
                {mm, jj, jj, jj, jj, jj, mm},
                {se, ss, ss, ss, ss, ss, se},
            }, 5);

            flush(1, 3);

            FurnisherItemTile __ = null;

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ts, ta},
                {__, st, __},
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ts, ts, ta},
                {__, st, st, __},
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ts, ts, ts, ta},
                {__, st, st, st, __},
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ts, ts, ts, ts, ta},
                {__, st, st, st, st, __},
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ts, ts, ts, ts, ts, ta},
                {__, st, st, st, st, st, __},
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ts, ts, ts, ts, ts, ts, ta},
                {__, st, st, st, st, st, st, __},
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ts, ts, ts, ts, ts, ts, ts, ta},
                {__, st, st, st, st, st, st, st, __},
            }, 7);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__, st, __},
                {ta, ts, ta},
                {ta, ts, ta},
                {__, st, __},
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__, st, st, __},
                {ta, ts, ts, ta},
                {ta, ts, ts, ta},
                {__, st, st, __},
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__, st, st, st, __},
                {ta, ts, ts, ts, ta},
                {ta, ts, ts, ts, ta},
                {__, st, st, st, __},
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__, st, st, st, st, __},
                {ta, ts, ts, ts, ts, ta},
                {ta, ts, ts, ts, ts, ta},
                {__, st, st, st, st, __},
            }, 8);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__, st, st, st, st, st, __},
                {ta, ts, ts, ts, ts, ts, ta},
                {ta, ts, ts, ts, ts, ts, ta},
                {__, st, st, st, st, st, __},
            }, 10);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__, st, st, st, st, st, st, __},
                {ta, ts, ts, ts, ts, ts, ts, ta},
                {ta, ts, ts, ts, ts, ts, ts, ta},
                {__, st, st, st, st, st, st, __},
            }, 12);

            flush(1, 3);
        }

        public override bool UsesArea()
        {
            return true;
        }

        public override bool MustBeIndoors()
        {
            return true;
        }

        public override RoomBlueprintImp Blue()
        {
            return Blue;
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new CanteenInstance(Blue, area, init);
        }

        public void RenderDish(SPRITE_RENDERER r, ShadowBatch shadowBatch, RESOURCE res, RenderIterator it, int ran)
        {
            SPlate.Render(r, shadowBatch, ran, it, 0, false);
            if (res != null)
            {
                COLOR.WHITE50.Bind();
                res.RenderOne(r, it.X(), it.Y(), ran);
                COLOR.Unbind();
            }
        }

        public override bool IsHeavy()
        {
            return true;
        }
    }
}