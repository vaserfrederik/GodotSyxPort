using System;
using System.Collections.Generic;
using System.IO;
using game.boosting;
using game.time;
using settlement.entity.humanoid;
using settlement.main;
using settlement.path.finders;
using settlement.room.knowledge.school;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.employment;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.stats;
using settlement.stats.colls;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.data;
using util.text;
using view.sett.ui.room;

namespace settlement.room.knowledge.university
{
    public sealed class ROOM_UNIVERSITY : RoomBlueprintIns<UniversityInstance>
    {
        private readonly UniversityConstructor constructor;
        public readonly double learningSpeed;
        private readonly Job job = new Job(this);

        private readonly RoomEducationHelper helper;

        private static readonly CharSequence ¤¤bonus = "Learning speed of";

        static
        {
            D.ts(typeof(ROOM_UNIVERSITY));
        }

        public readonly EmployerSimple emp = new EmployerSimple(employment());

        public ROOM_UNIVERSITY(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            constructor = new UniversityConstructor(this, init);
            learningSpeed = init.data().d("LEARNING_SPEED", 0, 100);
            clearP();
            pushBo(init.data(), info.name, ¤¤bonus + ": " + info.name, "UNIVERSITY", true);
            helper = new RoomEducationHelper(this, constructor.quality)
            {
                public AgeType type()
                {
                    return STATS.EDUCATION().adult;
                }
            };
        }

        protected override void update(double ds)
        {
        }

        public override SFinderRoomService service(int tx, int ty)
        {
            return null;
        }

        protected override void saveP(FilePutter f)
        {
        }

        protected override void loadP(FileGetter f) throws IOException
        {
        }

        protected override void clearP()
        {
        }

        public override Furnisher constructor()
        {
            return constructor;
        }

        public override void appendView(List<UIRoomModule> mm)
        {
            helper.appendView(mm);
        }

        public BOOLEAN isTime = new BOOLEAN
        {
            public bool is()
            {
                return TIME.days().bitPartOf() > employment().getShiftStart() && TIME.days().bitPartOf() < employment().getShiftStart() + Humanoid.WORK_PER_DAY;
            }
        };

        public double learningSpeed(RoomInstance i, BOOSTABLE_O h)
        {
            return helper.learningSpeed(i, h);
        }

        public bool isLecturer(COORDINATE c)
        {
            return SETT.ROOMS().fData.tileData.get(c) == UniversityConstructor.IWORKE;
        }

        public DIR spotDir(COORDINATE c)
        {
            return DIR.ORTHO.get(SETT.ROOMS().fData.spriteData.get(c) & 0b011);
        }
    }
}