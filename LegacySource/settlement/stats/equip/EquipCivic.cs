using System;
using System.IO;
using System.Collections.Generic;
using init.paths;
using init.race;
using init.type;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.keymap;
using util.text;

namespace settlement.stats.equip
{
    public class EquipCivic : Equip
    {
        private readonly RMapIntTwo<HCLASS, Race> tars = new RMapIntTwo<HCLASS, Race>(HCLASSES.MAP(), RACES.map());
        static readonly CharSequence ¤¤more = "We would like to be allowed to wear more {0}.";

        static
        {
            D.ts(typeof(EquipCivic));
        }

        EquipCivic(string key, PATH path, LISTE<Equip> all, LISTE<EquipCivic> type, StatsInit init)
            : base("CIVIC", key, path, all, init)
        {
            type.add(this);

            foreach (HCLASS cl in HCLASSES.ALL())
                foreach (Race race in RACES.all())
                    tars.get(cl).set(race, targetDefault);

            stat.info().setOpinion(¤¤more, null);

            SAVABLE sa = new SAVABLE()
            {
                public void save(FilePutter file)
                {
                    tars.save(file);
                }

                public void load(FileGetter file)
                {
                    tars.load(file);
                }

                public void clear()
                {
                    tars.setAll(targetDefault);
                }
            };
            sa.clear();

            init.savers.put(key + "_TAR", sa);
        }

        public override int target(Induvidual h)
        {
            return CLAMP.i(tars.get(h.clas()).get(h.race()), 0, max());
        }

        public int target(HCLASS c, Race type)
        {
            if (type == null)
            {
                int m = 0;
                for (int ri = 0; ri < RACES.all().size(); ri++)
                {
                    Race r = RACES.all().get(ri);
                    m = Math.Max(m, target(c, r));
                }
                return m;
            }
            return CLAMP.i(tars.get(c).get(type), 0, max());
        }

        public void targetSet(int target, HCLASS c, Race type)
        {
            if (type == null)
            {
                for (int ri = 0; ri < RACES.all().size(); ri++)
                {
                    Race r = RACES.all().get(ri);
                    targetSet(target, c, r);
                }
                return;
            }
            target = CLAMP.i(target, 0, equipMax);
            tars.get(c).set(type, target);
        }

        public int max()
        {
            return equipMax;
        }

        public override int max(Induvidual i)
        {
            return equipMax;
        }

        public override double bValue(double equipped)
        {
            return equipped;
        }
    }
}