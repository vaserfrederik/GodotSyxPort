using System;
using System.Collections.Generic;
using game.boosting;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.info;
using util.keymap;
using util.text;

namespace settlement.stats.colls
{
    public class StatsEducation : StatCollection
    {
        public static readonly int LIMIT_MAX = 100;

        private static readonly CharSequence ¤¤childHood = "¤Childhood";
        private static readonly CharSequence ¤¤adultHood = "¤Adulthood";

        private static readonly CharSequence ¤¤currentPolicy = "¤Current Policy";
        private static readonly CharSequence ¤¤currentLimit = "¤Current Limit";
        private static readonly CharSequence ¤¤currentLimitSpeed = "¤Limit Speed";
        private static readonly CharSequence ¤¤LimitMax = "¤Max Limit";

        private static readonly CharSequence ¤¤name = "Enlightenment";
        private static readonly CharSequence ¤¤desc = "Stats regarding your education levels, and boosts surrounding it.";
        private static readonly CharSequence ¤¤limit = "¤Study Limit";
        private static readonly CharSequence ¤¤limitD = "The maximum enlightenment allowed. High enlightenment gives diminishing returns, meaning a high limit will take longer to achieve.";

        static StatsEducation()
        {
            D.ts(typeof(StatsEducation));
        }

        public readonly LIST<StatEducation> all;
        private readonly RMapInt<HCLASS_RACE> policy = new RMapInt<HCLASS_RACE>(HCLASS_RACE.MAP(), 0, 100);
        public readonly AgeType child;
        public readonly AgeType adult;
        public readonly LIST<AgeType> allAges;

        public StatsEducation(StatsInit init) : base(init, "EDUCATION", ¤¤name, ¤¤desc)
        {
            child = new AgeType(UI.icons().s.reproduction, ¤¤childHood, init, 0);
            adult = new AgeType(UI.icons().s.human, ¤¤adultHood, init, 1);
            allAges = new ArrayList<StatsEducation.AgeType>(child, adult);
            all = new ArrayList<StatsEducation.StatEducation>(
                new StatEducation("EDUCATION", init, UI.icons().l.book.small, 0),
                new StatEducation("INDOCTRINATION", init, UI.icons().l.work.small, 1)
            );

            init.savers.put("EDUCATION_POL", policy);
        }

        public StatEducation policy(HCLASS_RACE cl)
        {
            return policy(cl.cl, cl.race);
        }

        public StatEducation policy(HCLASS cl, Race r)
        {
            if (r == null)
            {
                StatEducation rr = policy(cl, RACES.all().get(0));
                for (int ri = 0; ri < RACES.all().size(); ri++)
                {
                    if (policy(cl, RACES.all().get(ri)) != rr)
                        return null;
                }
                return rr;
            }
            return all.get(policy.get(HCLASS_RACE.clP(r, cl)));
        }

        public void policySet(HCLASS cl, Race r, StatEducation e)
        {
            if (r == null)
            {
                for (int ri = 0; ri < RACES.all().size(); ri++)
                    policySet(cl, RACES.all().get(ri), e);
                return;
            }
            policy.set(HCLASS_RACE.clP(r, cl), e.index);
        }

        public void policySet(HCLASS_RACE cl, StatEducation e)
        {
            policySet(cl.cl, cl.race, e);
        }

        public class AgeType
        {
            public readonly CharSequence name;
            public readonly SPRITE icon;
            private readonly RMapInt<HCLASS_RACE> limit;
            private readonly int typeI;
            public readonly Boostable limitMax;

            public AgeType(SPRITE icon, CharSequence name, StatsInit init, int allI)
            {
                this.name = name;
                this.icon = icon;
                this.limit = new RMapInt<HCLASS_RACE>(HCLASS_RACE.MAP(), 0, LIMIT_MAX, LIMIT_MAX / 6);
                this.typeI = allI;
                init.savers.put("EDU_AGE_TYPE" + allI, limit);
                limitMax = BOOSTING.push("EDUCATION_LIMIT_" + allI, 10, ¤¤limit + ": " + name, ¤¤limitD, icon, BOOSTABLES.CIVICS());
            }

            public int limit(HCLASS_RACE cl)
            {
                if (cl.race == null)
                {
                    int l = 0;
                    for (int ri = 0; ri < RACES.all().size(); ri++)
                        l = Math.Max(l, limit(HCLASS_RACE.clP(RACES.all().get(ri), cl.cl)));
                    return l;
                }

                return limit.get(cl);
            }

            public int limit(HCLASS cl, Race type)
            {
                return limit.get(HCLASS_RACE.clP(type, cl));
            }

            public double limitSpeed(HCLASS_RACE cl, Race type)
            {
                return (double)limit.get(HCLASS_RACE.clP(type, cl)) / LIMIT_MAX;
            }

            public void hover(GUI_BOX text, HCLASS cl, Race type)
            {
                GBox b = (GBox)text;
                b.textLL(name);
                b.tab(6);
                b.add(GFORMAT.perc(b.text(), (double)limit.get(HCLASS_RACE.clP(type, cl)) / LIMIT_MAX));
                b.NL();
                b.textL(¤¤currentLimit);
                b.tab(6);
                b.add(GFORMAT.perc(b.text(), limitSpeed(cl, type)));
                b.NL();
            }

            public void hover(GUI_BOX text, Induvidual indu)
            {
                GBox b = (GBox)text;
                b.textLL(name);
                b.tab(6);
                b.add(GFORMAT.perc(b.text(), (double)limit.get(HCLASS_RACE.clP(indu.race, indu.clas())) / LIMIT_MAX));
                b.NL();
            }
        }

        public class StatEducation
        {
            private double dAmount = 0;

            private readonly STAT[] allT;
            private readonly int index;
            public STAT total;

            public StatEducation(string key, StatsInit init, SPRITE icon, int index)
            {
                allT = new STAT[allAges.size()];
                for (int i = 0; i < allT.Length; i++)
                {
                    allT[i] = new STATData(null, key + "DC" + i, init, init.count.new DataByte(key + "DC" + i, LIMIT_MAX));
                    init.copier.add(allT[i].indu());
                }

                this.index = index;
                total = new STATFake(key, init)
                {
                    protected override double getDD(HCLASS s, Race r, int daysBack)
                    {
                        double res = 0;
                        foreach (STAT t in allT)
                        {
                            res += t.data(s).getD(r, daysBack);
                        }
                        return res / allT.Length;
                    }

                    public override void hover(GUI_BOX text, HCLASS cl, Race type)
                    {
                        GBox b = (GBox)text;
                        foreach (AgeType t in allAges)
                        {
                            b.textLL(t.name);
                            b.tab(6);
                            b.add(GFORMAT.perc(b.text(), allT[t.typeI].data(cl).getD(type)));
                            b.NL();
                            b.textL(¤¤currentLimit);
                            b.tab(6);
                            b.add(GFORMAT.perc(b.text(), (double)t.limit(cl, type) / LIMIT_MAX));
                            b.NL();
                            b.textL(¤¤currentLimitSpeed);
                            b.tab(6);
                            b.add(GFORMAT.f1(b.text(), (double)t.limitSpeed(cl, type) / LIMIT_MAX, 2));
                            b.sep();
                        }

                        b.textLL(Dic.¤¤Total);
                        b.tab(6);
                        b.add(GFORMAT.perc(b.text(), getDD(cl, type, 0)));
                        b.sep();
                        base.hover(text, cl, type);
                    }

                    protected override double induGet(Induvidual i)
                    {
                        double res = 0;
                        foreach (STAT t in allT)
                        {
                            res += t.indu().getD(i);
                        }
                        return res / allT.Length;
                    }

                    public override void hover(GUI_BOX text, Induvidual indu)
                    {
                        GBox b = (GBox)text;
                        b.textLL(¤¤currentPolicy);
                        b.tab(6);
                        b.add(b.text().add(policy(indu.clas(), indu.race()).total.info().name));
                        b.NL();
                        foreach (AgeType t in allAges)
                        {
                            b.textLL(t.name);
                            b.tab(6);
                            b.add(GFORMAT.perc(b.text(), allT[t.typeI].indu().getD(indu)));
                            b.NL();
                        }
                        base.hover(text, indu);
                    }
                };
                total.info().icon = icon;
            }

            private void educate(Induvidual i, double amount, STAT toIncrease)
            {
                double dam = amount + dAmount;
                int am = (int)dam;
                dAmount = dam - am;
                if (am == 0)
                    return;

                am = decrease(i, am);

                if (am == 0)
                    return;

                int max = toIncrease.indu().max(i) - toIncrease.indu().get(i);
                if (am > max)
                {
                    dAmount += am - max;
                    am = max;
                }
                toIncrease.indu().inc(i, am);
            }

            private int decrease(Induvidual i, int am)
            {
                if (am == 0)
                    return am;

                foreach (StatEducation o in all)
                {
                    if (o == this)
                        continue;

                    foreach (STAT os in o.allT)
                    {
                        int a = os.indu().get(i);
                        if (am > a)
                        {
                            os.indu().inc(i, -a);
                            am -= a;
                        }
                        else
                        {
                            os.indu().inc(i, -am);
                            return 0;
                        }
                        if (am <= 0)
                            return 0;
                    }
                }

                return am;
            }
        }
    }
}