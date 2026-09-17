using System;
using System.IO;
using settlement.constant;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.rendering;

namespace settlement.room.service.speaker
{
    internal sealed class SpeakerConstructor : Furnisher
    {
        private readonly ROOM_SPEAKER blue;
        public readonly FurnisherStat workers;
        public readonly FurnisherStat spectators;

        protected SpeakerConstructor(ROOM_SPEAKER blue, RoomInitData init) : base(init, 1, 2, 88, 44)
        {
            this.blue = blue;
            workers = new FurnisherStat.FurnisherStatEmployees(this);
            spectators = new FurnisherStat.FurnisherStatServices(this, blue);

            Json sp = init.data().json("SPRITES");

            RoomSprite sSprite = new RoomSpriteCombo(sp, "CENTER_COMBO");

            RoomSprite sFrame = new RoomSpriteCombo(sp, "FRAME_COMBO")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    if ((data & 0x0F) == 0x0F)
                    {
                        foreach (DIR d in DIR.NORTHO)
                        {
                            it.setOff(d.x() * C.TILE_SIZEH, d.y() * C.TILE_SIZEH);
                            d = d.perpendicular();
                            int m = d.next(-1).mask() | d.next(1).mask();

                            sSprite.render(r, s, m, it, degrade, isCandle);
                        }
                    }
                    return false;
                }
            };

            FurnisherItemTile bb = new FurnisherItemTile(
                this,
                sFrame,
                AVAILABILITY.PENALTY4,
                false
            );
            final FurnisherItemTile b1 = new FurnisherItemTile(
                this,
                sFrame,
                AVAILABILITY.ROOM,
                false
            );

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { b1, b1, b1 },
                { b1, bb, b1 },
                { b1, b1, b1 },
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
            return new SpeakerInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }
    }
}