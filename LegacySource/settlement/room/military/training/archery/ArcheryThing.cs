using settlement.main;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.military.training.archery
{
    public sealed class ArcheryThing : SETT_JOB
    {
        private readonly Coo coo = new Coo();
        private int data;
        private readonly ROOM_ARCHERY b;
        private ArcheryInstance ins;

        public static readonly Bit reserved = new Bit(0b00001);
        public static readonly Bit used = new Bit(0b00010);

        public ArcheryThing(ROOM_ARCHERY b)
        {
            this.b = b;
        }

        private void Save()
        {
            ROOMS().Data.Set(ins, coo, data);
        }

        public ArcheryThing Init(int tx, int ty)
        {
            coo.Set(tx, ty);
            if (ROOMS().FData.Tile.Is(coo, b.Constructor.Plat))
            {
                ins = b.Get(coo.X, coo.Y);
                data = ROOMS().Data.Get(coo);
                return this;
            }
            return null;
        }

        public override void JobReserve(RESOURCE r)
        {
            data = reserved.Set(data);
            Save();
        }

        public override bool JobReservedIs(RESOURCE r)
        {
            return reserved.Is(data);
        }

        public override void JobReserveCancel(RESOURCE r)
        {
            data = reserved.Clear(data);
            Save();
        }

        public override bool JobReserveCanBe()
        {
            return !JobReservedIs(null);
        }

        public override RBIT JobResourceBitToFetch()
        {
            return null;
        }

        public override double JobPerformTime(Humanoid a)
        {
            return 0;
        }

        public override void JobStartPerforming()
        {
            // TODO Auto-generated method stub
        }

        public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
        {
            JobReserveCancel(r);
            return null;
        }

        public override COORDINATE JobCoo()
        {
            return coo;
        }

        public override System.CharSequence JobName()
        {
            return ins.BlueprintI().Employment().Verb;
        }

        public override bool JobUseTool()
        {
            return false;
        }

        public override SoundRace JobSound()
        {
            return ins.BlueprintI().Employment().Sound();
        }
    }
}