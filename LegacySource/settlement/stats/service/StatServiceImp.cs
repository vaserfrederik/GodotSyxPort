using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.data;
using util.info;
using util.text;

namespace settlement.stats.service
{
    public abstract class StatServiceImp : StatService
    {
        public const int TARGET_MAX = 16;
        private readonly Perm permission;

        static readonly CharSequence ¤¤Access = "¤Access";
        static readonly CharSequence ¤¤AcessDesc = "¤The level of access this subject has to a service. Can be improved by building more service facilities and making sure they are close enough for your people to utilize.";
        static readonly CharSequence ¤¤Quality = "¤Quality";
        static readonly CharSequence ¤¤QualityDesc = "¤The quality of a subject's last visit to this facility. Often improved by placing special items in the rooms in question.";
        static readonly CharSequence ¤¤Distance = "¤Proximity";
        static readonly CharSequence ¤¤DistanceDesc = "¤Whenever a subject wants to use a service, this is the distance the subject has had to walk to reach it. This value only reaches 100% if subjects are right next to the service at all times. You must yourself find a good balance.";
        static readonly CharSequence ¤¤TotalDesc = "¤The access and quality this subject group has. Can be improved by building more facilities, keeping them maintained, and also in some cases building them well.";
        static readonly CharSequence ¤¤UpDesc = "¤The current upgrade boost from the rooms visited. Some rooms don't have upgrades.";
        static readonly CharSequence ¤¤perm = "¤Permission";

        static StatServiceImp()
        {
            D.ts(typeof(StatServiceImp));
        }

        protected StatServiceImp(string key, LISTE<StatServiceImp> all, StatsInit init, CharSequence name, CharSequence desc, SPRITE icon, NEED need)
            : base(name, desc, icon, need)
        {
            all.add(this);
            permission = new Perm(¤¤perm, ¤¤perm + ": " + name);
            init.savers.put("SER_PERM_" + key, permission);
        }

        public BOOLEAN_OE<HCLASS_RACE> permission()
        {
            return permission;
        }

        public double getBasePriority(Humanoid h)
        {
            return h.indu().race().stats().defNormalized(h.indu().hType().CLASS, total().standing());
        }

        public bool accessRequest(Humanoid h)
        {
            if (h.indu().hType() == HTYPES.NOBILITY())
                return true;
            if (h.indu().hType() == HTYPES.TOURIST())
                return true;
            if (h.indu().hType().parent() != h.indu().hType())
            {
                if (!total().standing.definition(h.race()).child)
                    return false;
                return permission.is(HCLASS_RACE.clP(h.race(), h.indu().hType().parent().parentClass()));
            }
            return permission.is(h.indu().popCL());
        }

        private class Perm : BOOLEAN_OE<HCLASS_RACE>, SAVABLE
        {
            private readonly Bitmap1D access;
            private readonly INFO info;

            public Perm(CharSequence name, CharSequence desc)
            {
                this.access = new Bitmap1D(HCLASS_RACE.ALL().size(), false);
                this.info = new INFO(name, desc);
            }

            public INFO info()
            {
                return info;
            }

            public void save(FilePutter file)
            {
                access.save(file);
            }

            public void load(FileGetter file)
            {
                access.load(file);
            }

            public void clear()
            {
                foreach (HCLASS_RACE p in HCLASS_RACE.ALL())
                {
                    if (this is StatServiceChild)
                        access.set(p.index, true);
                    if (p.cl != null && p.race != null)
                    {
                        bool b = p.cl != HCLASSES.SLAVE() && total().boosters.all().size() > 0;
                        b |= total().standing().max(p.cl, p.race) != 0;
                        access.set(p.index, !b);
                    }
                }
            }

            public bool is(HCLASS_RACE t)
            {
                if (t.race == null)
                {
                    bool m = false;
                    foreach (Race r in RACES.all())
                    {
                        m |= is(t.cl.get(r));
                    }
                    return m;
                }
                return !access.get(t.index());
            }

            public BOOLEAN_OE<HCLASS_RACE> set(HCLASS_RACE t, bool b)
            {
                if (t.race == null)
                {
                    foreach (Race r in RACES.all())
                    {
                        set(t.cl.get(r), b);
                    }
                }
                else
                    access.set(t.index(), !b);
                return this;
            }
        }
    }
}