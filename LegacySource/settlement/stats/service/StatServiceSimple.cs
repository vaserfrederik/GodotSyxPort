using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.sets;
using snake2d.util.sprite;

namespace settlement.stats.service
{
    public class StatServiceSimple : StatServiceImp
    {
        private readonly STAT access;

        public StatServiceSimple(string key, LISTE<StatServiceImp> all, StatsInit init, ICharSequence name, ICharSequence desc, SPRITE icon, NEED need)
            : base(key, all, init, name, desc, icon, need)
        {
            access = new STATData(key, init, init.count.new DataBit("SERVICEA_" + key), new StatInfo(name, "¤¤TotalDesc"));
            access.info().setMatters(false, true);
            access.info().icon = icon;
            init.onArrivalStats.add(access);
        }

        public override bool access(Humanoid h)
        {
            return access.indu().get(h.indu()) == 1;
        }

        public void setAccess(Humanoid h, bool access)
        {
            setAccess(h.indu(), access);
        }

        public void setAccess(Induvidual i, bool access)
        {
            this.access.indu().set(i, access ? 1 : 0);
        }

        public override STAT total()
        {
            return access;
        }

        public override void clearAccess(Induvidual i)
        {
            access.indu().set(i, 0);
        }

        public override void cheatSetTotal(Induvidual i, double tot)
        {
            access.indu().set(i, tot > 0 ? 1 : 0);
        }
    }
}