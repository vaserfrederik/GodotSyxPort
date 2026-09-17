using System;
using settlement.entity.humanoid;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.sprite;

namespace settlement.stats.service
{
    public abstract class StatService
    {
        public readonly string name;
        public readonly string desc;
        public readonly SPRITE icon;
        public readonly NEED need;
        public double usage = 1;

        public StatService(string name, string desc, SPRITE icon, NEED need)
        {
            this.name = name;
            this.desc = desc;
            this.icon = icon;
            this.need = need;
        }

        public abstract bool access(Humanoid h);
        public abstract void clearAccess(Induvidual i);
        public abstract STAT total();
        public abstract void cheatSetTotal(Induvidual i, double tot);

        public string name(Induvidual i)
        {
            return name;
        }

        public SPRITE icon(Induvidual i)
        {
            return icon;
        }
    }
}