using System;
using init.race;
using init.type;
using settlement.stats;
using settlement.thing.ThingsCorpses;
using snake2d.util.bit;
using snake2d.util.misc;

namespace settlement.room.spirit.grave
{
    public sealed class GraveInfo
    {
        private static readonly GraveInfo self = new GraveInfo();

        private BitsLong gender = new BitsLong(0x000000000000000Fl);
        private BitsLong race = new BitsLong(0x0000000000FF0000L);
        private BitsLong type = new BitsLong(0x00000000FF000000L);
        private BitsLong cause = new BitsLong(0x000000FF00000000L);
        private BitsLong age = new BitsLong(0x00FFFF0000000000L);
        private BitsLong has = new BitsLong(0x8000000000000000L);

        private GraveInstance ins;
        private int id;
        private long dataD;
        private int nameD;

        private GraveInfo()
        {
            if (race.mask < RACES.all().Count)
                throw new RuntimeException();
            if (type.mask < HTYPES.ALL().Count)
                throw new RuntimeException();
            if (cause.mask < CAUSE_LEAVES.ALL().Count)
                throw new RuntimeException();
        }

        public static GraveInfo Get(GraveInstance instance, int id)
        {
            self.ins = instance;
            self.id = id;
            self.dataD = instance.datas[id];
            self.nameD = instance.names[id];
            return self;
        }

        public CharSequence Name()
        {
            return STATS.APPEARANCE().Name(Race(), Type(), gender.Get(dataD), nameD);
        }

        public bool HasBody()
        {
            return has.Get(dataD) > 0;
        }

        public Race Race()
        {
            return RACES.all().ElementAt((int)race.Get(dataD));
        }

        public HTYPE Type()
        {
            return HTYPES.ALL().ElementAt((int)type.Get(dataD));
        }

        public CAUSE_LEAVE Cause()
        {
            return CAUSE_LEAVES.ALL().ElementAt((int)cause.Get(dataD));
        }

        public void Clear()
        {
            dataD = has.Set(dataD, 0);
            ins.datas[id] = dataD;
        }

        public int Years()
        {
            return (int)age.Get(dataD);
        }

        public void Bury(Corpse c)
        {
            dataD = has.Set(dataD, 1);
            dataD = gender.Set(dataD, STATS.APPEARANCE().gender.Get(c.indu()));
            nameD = STATS.APPEARANCE().nameData.Get(c.indu());
            dataD = type.Set(dataD, c.indu().hType().index());
            dataD = race.Set(dataD, c.indu().race().index);
            dataD = cause.Set(dataD, c.cause().index());
            int a = (int)Math.Ceiling(STATS.POP().age.years.GetD(c.indu()));
            a = CLAMP.i(a, 0, (int)age.mask);
            dataD = age.Set(dataD, a);
            ins.datas[id] = dataD;
            ins.names[id] = nameD;
        }
    }
}