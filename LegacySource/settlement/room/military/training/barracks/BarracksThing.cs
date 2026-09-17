using System;
using settlement.main;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.military.training.barracks
{
    internal class BarracksThing : SETT_JOB
    {
        private readonly Coo coo = new Coo();
        private readonly Coo cooMan = new Coo();
        private int data;
        private int dataManikin;
        private readonly ROOM_BARRACKS b;
        private BarracksInstance ins;

        static readonly Bit reserved = new Bit(0b00001);
        static readonly Bit used = new Bit(0b00010);

        public BarracksThing(ROOM_BARRACKS b)
        {
            this.b = b;
        }

        private void save()
        {
            ROOMS().data.set(ins, cooMan, dataManikin);
            ROOMS().data.set(ins, coo, data);
        }

        public BarracksThing init(int tx, int ty)
        {
            coo.set(tx, ty);
            if (ROOMS().fData.tile.is(coo, b.constructor.work))
            {
                ins = b.get(coo.x(), coo.y());
                for (int di = 0; di < DIR.ORTHO.size; di++)
                {
                    DIR d = DIR.ORTHO.get(di);
                    if (ins.is(tx, ty, d) && ROOMS().fData.tile.is(coo, d, b.constructor.manikin))
                    {
                        ins = b.get(coo.x(), coo.y());
                        cooMan.set(coo);
                        cooMan.increment(d.x, d.y);
                        data = ROOMS().data.get(coo);
                        dataManikin = ROOMS().data.get(cooMan);
                        return this;
                    }
                }
                throw new Exception();
            }
            return null;
        }

        public override void jobReserve(RESOURCE r)
        {
            data = reserved.set(data);
            save();
        }

        public override bool jobReservedIs(RESOURCE r)
        {
            return reserved.is(data);
        }

        public override void jobReserveCancel(RESOURCE r)
        {
            data = reserved.clear(data);
            dataManikin = used.clear(dataManikin);
            save();
        }

        public override bool jobReserveCanBe()
        {
            return !jobReservedIs(null);
        }

        public override RBIT jobResourceBitToFetch()
        {
            return null;
        }

        public override double jobPerformTime(Humanoid a)
        {
            return 0;
        }

        public override void jobStartPerforming()
        {
            dataManikin = used.set(dataManikin);
            save();
        }

        public override RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm)
        {
            jobReserveCancel(r);
            return null;
        }

        public override COORDINATE jobCoo()
        {
            return coo;
        }

        public override string jobName()
        {
            return ins.blueprintI().employment().verb;
        }

        public override bool jobUseTool()
        {
            return false;
        }

        public override SoundRace jobSound()
        {
            return ins.blueprintI().employment().sound();
        }
    }
}