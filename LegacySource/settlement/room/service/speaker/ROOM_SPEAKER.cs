using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using settlement.entity.humanoid;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.service.module;
using view.sett.ui.room;

namespace settlement.room.service.speaker
{
    public sealed class ROOM_SPEAKER : RoomBlueprintIns<SpeakerInstance>, ROOM_SERVICE_NEED_HASER, ROOM_SPECTATOR.ROOM_SPECTATOR_HASER
    {
        private readonly RoomServiceNeed data;
        private readonly SpeakerConstructor constructor;
        private readonly Centre work;

        public ROOM_SPEAKER(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            work = new Centre(this);
            data = new RoomServiceNeed(this, init)
            {
                service = (tx, ty) => work.service(tx, ty)
            };
            constructor = new SpeakerConstructor(this, init);
            employment().setShiftStart(ROOM_SPECTATOR.WORK_STARTSD, false);
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public override Furnisher constructor()
        {
            return constructor;
        }

        public override SFinderRoomService service(int tx, int ty)
        {
            return data.finder;
        }

        protected override void saveP(FilePutter saveFile)
        {
            data.saver.save(saveFile);
        }

        protected override void loadP(FileGetter saveFile)
        {
            data.saver.load(saveFile);
        }

        protected override void clearP()
        {
            data.saver.clear();
        }

        public RoomServiceNeed service()
        {
            return data;
        }

        public void appendView(LISTE<UIRoomModule> mm)
        {
        }

        private readonly ROOM_SPECTATOR spec = new ROOM_SPECTATOR
        {
            coo = new Coo(),
            acts = Alloc.bb(64),

            service = () => ROOM_SPEAKER.this.service(),
            lookAt = (sx, sy) =>
            {
                SpeakerInstance ins = getter.get(sx, sy);
                if (ins == null)
                {
                    coo.set(sx, sy);
                }
                else
                {
                    coo.set(ins.body().cX(), ins.body().cY());
                }
                coo.set(coo.x() * C.TILE_SIZE + C.TILE_SIZEH, coo.y() * C.TILE_SIZE + C.TILE_SIZEH);
                return coo;
            },
            is = (sx, sy) => getter.get(sx, sy) != null,
            shouldCheer = (sx, sy) => activity(sx, sy) == 1,
            shouldBoo = (sx, sy) => activity(sx, sy) == 2,
            doSomeThingExtraWhenAccess = a =>
            {
                // if (STATS.EDUCATION().TOTAL().getD(a.indu()) < 0.15)
                //     STATS.EDUCATION().educate(a.indu(), 0.01);
            },
            isActive = (sx, sy) =>
            {
                SpeakerInstance ins = getter.get(sx, sy);
                if (ins == null || !work.job(sx, sy).jobReservedIs(null))
                    return false;
                return true;
            }
        };

        public ROOM_SPECTATOR spec()
        {
            return spec;
        }

        private int activity(int sx, int sy)
        {
            SpeakerInstance ins = getter.get(sx, sy);
            if (ins == null)
                return 0;
            if (!work.job(sx, sy).jobReservedIs(null))
                return 0;
            int s = ins.off;
            s += (int)(acts.Length * TIME.currentSecond() / TIME.secondsPerDay());
            s %= acts.Length;
            return acts[s];
        }
    }
}