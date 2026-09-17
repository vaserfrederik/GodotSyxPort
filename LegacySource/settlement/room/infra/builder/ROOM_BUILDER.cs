using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.job;
using settlement.room.sprite;
using snake2d.util.file;
using view.tool;

namespace settlement.room.infra.builder
{
    public sealed class ROOM_BUILDER : RoomBlueprintIns<BuilderInstance>, ROOM_RADIUSE
    {
        private readonly Furnisher constructor;

        public ROOM_BUILDER(RoomInitData init, RoomCategorySub cat) : base(0, init, "_BUILDER", cat)
        {
            constructor = new Furnisher(init, 0, 0, 88, 72)
            {
                {
                    Json sp = init.data().json("SPRITES");
                    RoomSprite sprite = new RoomSprite1x1(sp, "1x1");
                    FurnisherItemTile t = new FurnisherItemTile(this, sprite, AVAILABILITY.AVOID_PASS, false);
                    new FurnisherItem(new FurnisherItemTile[][]
                    {
                        { t },
                    }, 0);

                    flushSingle(info);
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
                    return new BuilderInstance(ROOM_BUILDER.this, area, init);
                }

                public override CharSequence placable(int tx, int ty, FurnisherItem item, FurnisherItemTile tile)
                {
                    if (TERRAIN().get(tx, ty).roofIs())
                        return base.placable(tx, ty, item, tile);
                    if (TERRAIN().get(tx, ty) != TERRAIN().NADA && !TERRAIN().get(tx, ty).clearing().isEasilyCleared())
                    {
                        return PlacableMessages.¤¤TERRAIN_BLOCK;
                    }
                    return base.placable(tx, ty, item, tile);
                }

                public override RoomBlueprintImp blue()
                {
                    return ROOM_BUILDER.this;
                }

                public override bool needFlooring()
                {
                    return false;
                }
            };
        }

        protected override void update(double ds)
        {
        }

        public override Furnisher constructor()
        {
            return constructor;
        }

        public override SFinderRoomService service(int tx, int ty)
        {
            // TODO Auto-generated method stub
            return null;
        }

        protected override void saveP(FilePutter saveFile)
        {
        }

        protected override void loadP(FileGetter saveFile)
        {
        }

        protected override void clearP()
        {
        }

        public override bool degrades()
        {
            return false;
        }

        public override ROOM_RADIUS_INSTANCE radiusInstance(Room t)
        {
            return (BuilderInstance)t;
        }

        public void reset(RoomInstance ins)
        {
            if (ins == null)
                return;
            if (ins is BuilderInstance)
            {
                BuilderInstance b = (BuilderInstance)ins;
                b.failHour = (byte)(TIME.hours().bitCurrent() - 1);
            }
            return;
        }
    }
}