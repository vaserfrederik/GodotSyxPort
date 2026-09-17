using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Industry.Module
{
    using Game.Boosting;
    using Init.Sprite.UI;
    using Init.Type;
    using Settlement.Entity.Humanoid;
    using Settlement.Room.Industry.Module.Consumption;
    using Settlement.Room.Main;
    using Settlement.Stats;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Gui;
    using Snake2D.Util.Sets;
    using Util.Gui.Misc;
    using Util.Info;
    using Util.Text;

    public static class IndustryUtil
    {
        private static readonly CharSequence¤¤pRate = "Production speed";
        private static readonly CharSequence¤¤cRate = "Consumption Rate";
        private static readonly CharSequence¤¤cBonus = "Consumption Bonus";

        static IndustryUtil()
        {
            D.ts(typeof(IndustryUtil));
        }

        private IndustryUtil() { }

        public static double CalcConsumptionRate(double baseRate, Humanoid h, RoomInstance ins, RoomConsumptionAbs industry)
        {
            return CalcProductionRate(baseRate, h, industry, industry.bonus(), ins) / industry.conBonus(h.indu());
        }

        public static double CalcConsumptionRate(double baseRate, RoomInstance ins, RoomConsumptionAbs industry)
        {
            double mul = 1;

            Boostable bonus = industry.conBonus;

            if (bonus != null)
            {
                mul = bonus.get(HCLASS_RACE.clP());
            }

            return CalcProductionRate(baseRate, industry, industry.bonus(), ins) / mul;
        }

        public static double RoomBonus(RoomInstance ins, IndustryRate rate)
        {
            double r = 1;
            r *= 0.25 + 0.75 - 0.75 * (ins).getDegrade();
            if (rate != null)
            {
                foreach (RoomBoost b in rate.boosts())
                    r *= b.get(ins);
            }

            return r;
        }

        public static double CalcProductionRate(double baseRate, Humanoid h, IndustryRate rate, RoomInstance ins)
        {
            return CalcProductionRate(baseRate, h, rate, rate.bonus(), ins);
        }

        public static double CalcProductionRate(double baseRate, Humanoid h, IndustryRate rate, Boostable bonus, RoomInstance ins)
        {
            double r = roomBonus(ins, rate);
            r *= baseRate;
            if (bonus != null)
                r *= bonus.get(h.indu());
            return r;
        }

        public static double CalcProductionRate(double baseRate, IndustryRate rate, RoomInstance ins)
        {
            return CalcProductionRate(baseRate, rate, rate.bonus(), ins);
        }

        public static double CalcProductionRate(double baseRate, IndustryRate rate, Boostable bonus, RoomInstance ins)
        {
            double r = roomBonus(ins, rate);
            r *= ins.employees().totEfficiency();

            double am = 0;
            double mul = 0;

            if (bonus != null)
            {
                foreach (Humanoid a in ins.employees().employees())
                {
                    mul += bonus.get(a.indu());
                    am++;
                }
            }

            if (am > 0)
            {
                mul /= am;
            }
            else
            {
                mul = 1;
            }

            return r * mul * baseRate;
        }

        private static double[] values = new double[100];

        public static void HoverProductionRate(GUI_BOX text, double baseRate, IndustryRate rate, RoomInstance ins)
        {
            HoverProductionRate(text, baseRate, rate, rate.bonus(), ins);
        }

        public static void HoverProductionRate(GUI_BOX text, double baseRate, IndustryRate rate, Boostable bonus, RoomInstance ins)
        {
            GBox b = (GBox)text;

            b.NL(4);

            b.textLL(Dic.¤¤Base);
            b.NL();
            b.text(Dic.¤¤Rate);
            b.tab(6);
            b.add(GFORMAT.f(b.text(), baseRate));
            b.NL();

            b.text(Dic.¤¤Employees);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), ins.employees().employed()));
            b.NL();

            b.text(RoomEmploymentIns.¤¤Workload);
            b.tab(6);
            b.add(GFORMAT.f1(b.text(), ins.employees().efficiency()));
            b.NL();

            b.text(RoomEmploymentIns.¤¤Proximity);
            b.tab(6);
            b.add(GFORMAT.f1(b.text(), ins.employees().proximity()));
            b.NL();

            if (ins.blueprintI().employment().countInput())
            {
                b.text(RoomEmploymentIns.¤¤ProximityInput);
                b.tab(6);
                b.add(GFORMAT.f1(b.text(), ins.employees().fetchProximity()));
                b.NL();
            }

            b.tab(6);
            b.add(GFORMAT.fRel(b.text(), baseRate * ins.employees().employed() * ins.employees().totEfficiency(), baseRate * ins.employees().employed()));
            b.NL(8);

            HoverBoosts(text, baseRate * ins.employees().employed(), rate, bonus, ins, ins.employees().totEfficiency());

            b.NL();
        }

        public static void HoverBoosts(GUI_BOX text, double baseRate, IndustryRate rate, Boostable bonus, RoomInstance ins, double totEfficiency)
        {
            GBox b = (GBox)text;

            double mul = 1.0;
            double add = 0;

            int tot = 0;
            Array.Fill(values, 0);
            foreach (Humanoid a in ins.employees().employees())
            {
                tot++;

                int vi = 0;

                if (STATS.WORK().EMPLOYED.get(a) == ins)
                {
                    foreach (Booster s in bonus.all())
                    {
                        if (s.isMul)
                        {
                            values[vi] += s.get(a.indu()) - 1.0;
                        }
                        else
                        {
                            values[vi] += s.get(a.indu());
                        }

                        vi++;
                    }
                }
            }

            if (tot > 0)
            {
                int vi = 0;

                foreach (Booster s in bonus.all())
                {
                    if (s.isMul)
                    {
                        values[vi] += s.get(a.indu()) - 1.0;
                    }
                    else
                    {
                        values[vi] += s.get(a.indu());
                    }

                    vi++;
                }
            }

            if (tot > 0)
            {
                int vi = 0;

                foreach (Booster s in bonus.all())
                {
                    if (s.isMul)
                    {
                        mul *= 1 + values[vi];
                    }
                    else
                    {
                        add += values[vi];
                    }

                    vi++;
                }
            }

            b.NL(4);
            b.textL(¤¤pRate);
            b.tab(6);
            GText t = b.text();
            b.add(GFORMAT.f1(t, mul));
            b.NL();

            b.NL();
            b.textL(Dic.¤¤Employees);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), ins.employees().employed()));
            b.NL();

            b.NL();
            b.textL(¤¤cBonus);
            b.tab(6);
            b.add(GFORMAT.f(b.text(), industry.conBonus(HCLASS_RACE.clP())));
            b.NL();

            b.NL();
            b.textLL(Dic.¤¤Total);
            b.tab(6);
            b.add(b.text().add('(').add(baseRate).s().add('*').s().add(mul).s().add('*').s().add(ins.employees().employed()).add(')').s().add('/').s().add(industry.conBonus(HCLASS_RACE.clP())).s().add('=').s().add(ins.employees().employed() * baseRate * mul / industry.conBonus(HCLASS_RACE.clP())));
            b.NL();
        }

        public static void HoverConsumptionRate(GUI_BOX text, double baseRate, RoomInstance ins, RoomConsumptionAbs industry)
        {
            GBox b = (GBox)text;

            b.NL(4);
            b.textL(¤¤cRate);
            b.tab(6);
            b.add(GFORMAT.f(b.text(), -baseRate));
            b.NL();

            double rr = CalcProductionRate(1, industry, ins);

            b.NL(4);
            b.textL(¤¤pRate);
            b.tab(6);
            GText t = b.text();
            b.add(GFORMAT.f1(t, rr));
            b.NL();

            b.NL();
            b.textL(Dic.¤¤Employees);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), ins.employees().employed()));
            b.NL();

            b.NL();
            b.textL(¤¤cBonus);
            b.tab(6);
            b.add(GFORMAT.f(b.text(), industry.conBonus(HCLASS_RACE.clP())));
            b.NL();

            b.NL();
            b.textLL(Dic.¤¤Total);
            b.tab(6);
            b.add(b.text().add('(').add(baseRate).s().add('*').s().add(rr).s().add('*').s().add(ins.employees().employed()).add(')').s().add('/').s().add(industry.conBonus(HCLASS_RACE.clP())).s().add('=').s().add(ins.employees().employed() * baseRate * rr / industry.conBonus(HCLASS_RACE.clP())));
            b.NL();
        }

        public static void Save(FilePutter p, LIST<Industry> ins)
        {
            p.i(ins.size());
            foreach (Industry i in ins)
                i.save(p);
        }

        public static void Load(FileGetter p, LIST<Industry> ins) throws IOException
        {
            int am = p.i();
            if (ins.size() != am)
            {
                for (int i = 0; i < am; i++)
                    ins.get(0).load(p);
            }
            else
            {
                foreach (Industry i in ins)
                    i.load(p);
            }
        }

        public static void Clear(LIST<Industry> ins)
        {
            foreach (Industry i in ins)
                i.clear();
        }
    }
}