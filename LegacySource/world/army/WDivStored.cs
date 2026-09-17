using System;
using System.Collections.Generic;
using System.IO;

namespace World.Army
{
    class WDivStored : ADDiv
    {
        private const int type = 1;
        private static readonly COLOR col = COLOR.BLUE100.MakeSaturated(0.5).Shade(0.75);
        private readonly int[] stats = Alloc.Ii(STATS.All().Count);
        private readonly List<Induvidual> all = new List<Induvidual>(Config.Battle().MEN_PER_DIVISION);
        private double returnSecond = 0;

        public WDivStored(int index) : base(index)
        {
        }

        public override void Save(FilePutter file)
        {
            base.Save(file);
            file.I(all.Count);
            foreach (Induvidual s in all)
                s.Save(file);
            file.D(returnSecond);
        }

        public override void Load(FileGetter file)
        {
            base.Load(file);

            all.Clear();
            int am = file.I();
            for (int i = 0; i < am; i++)
            {
                Induvidual h = new Induvidual(file);
                all.Add(h);
                for (int si = 0; si < STATS.All().Count; si++)
                {
                    stats[si] += STATS.All()[si].Indu().Get(h);
                }
            }
            Report(1);
            returnSecond = file.D();
        }

        public override double Equip(EquipBattle e)
        {
            return e.Target(Div()) * AD.Supplies().Get(e).AmountValue(Army()) / e.EquipMax;
        }

        private Div Div()
        {
            return GAME.ARMIES().Player().Divisions().Get(index);
        }

        public override int Men()
        {
            return all.Count;
        }

        public override Race Race()
        {
            return Div().Info.Race();
        }

        public override int MenTarget()
        {
            return GAME.ARMIES().Player().Divisions().Get(index).Info.Men();
        }

        public override double Training(StatTraining tr)
        {
            return Stat(tr.Stat);
        }

        public double Stat(STAT stat)
        {
            if (all.Count == 0)
                return 0;
            return (double)stats[stat.Index()] / (stat.Indu().Max(null) * all.Count);
        }

        public override double Experience()
        {
            return Stat(STATS.BATTLE().COMBAT_EXPERIENCE);
        }

        public override string Name()
        {
            return Div().Info.Name();
        }

        protected override void ArmyChange(WArmy old, WArmy newW)
        {
            returnSecond = TIME.CurrentSecond();
            if (old != null && old.Region() != null)
            {
                returnSecond += TIME.SecondsPerDay() * (1 + RD.DIST().Distance().Get(old.Region()) / 20.0);
            }
            else
                base.ArmyChange(old, newW);
        }

        public double ReturnSecond()
        {
            return returnSecond;
        }

        public int Index()
        {
            return index;
        }

        public void Add(Humanoid indu)
        {
            Add(indu.Indu());
        }

        public void Add(Induvidual n)
        {
            Report(-1);
            if (all.Count == 0)
                Array.Fill(stats, 0);

            for (int si = 0; si < STATS.All().Count; si++)
            {
                stats[si] += STATS.All()[si].Indu().Get(n);
            }

            Induvidual inu = new Induvidual(n.HType(), n.Race());

            inu.CopyFrom(n);
            STATS.NEEDS().Clear(inu);
            all.Add(inu);

            STATS.REL().SetSoldier(inu, WDivStoredAll.GetSoldierId(all.Count - 1, Div().IndexArmy()));

            Report(1);
        }

        private void Remove(Induvidual n)
        {
            Report(-1);
            for (int si = 0; si < STATS.All().Count; si++)
            {
                stats[si] -= STATS.All()[si].Indu().Get(n);
            }
            STATS.REL().SetDeath(n, CAUSE_LEAVES.SLAYED());
            all.Remove(n);
            for (int i = 0; i < all.Count; i++)
                STATS.REL().SetSoldier(all[i], WDivStoredAll.GetSoldierId(i, Div().IndexArmy()));
            Report(1);
        }

        public override int DaysUntilMenArrives()
        {
            return 0;
        }

        protected override void Report(int d)
        {
            AD.CityDivs().Amount += d * all.Count;
            AD.CityDivs().Ramounts[Race().Index] += d * all.Count;
            base.Report(d);

        }

        public override void Resolve(Induvidual[] hs)
        {
            Report(-1);
            for (int i = 0; i < all.Count; i++)
            {
                STATS.REL().SetDeath(all[i], CAUSE_LEAVES.SLAYED());
            }

            Array.Fill(stats, 0);
            all.Clear();

            Report(1);
            foreach (Induvidual i in hs)
                Add(i);
        }

        public override void Resolve(int surviviors, double experiencePerMan)
        {
            double dExperience = experiencePerMan - Experience();
            dExperience *= surviviors;
            List<Induvidual> all = new List<Induvidual>(surviviors);
            for (int i = 0; i < surviviors; i++)
            {
                Induvidual s = this.all[i];
                STATS.REL().SetDeath(s, CAUSE_LEAVES.SLAYED());
                int a = (int)dExperience;
                if (dExperience - a > RND.RFloat())
                    a++;
                STATS.BATTLE().COMBAT_EXPERIENCE.Indu().Inc(s, a);
                all.Add(s);
            }
            Report(-1);
            this.all.Clear();
            Report(1);
            foreach (Induvidual h in all)
                Add(h);
        }

        public override void MenSet(int amount)
        {
            amount = CLAMP.I(amount, 0, Men());
            while (amount < Men())
            {
                Induvidual t = all[all.Count - 1];
                Remove(t);
            }
            while (amount > Men())
            {
                Induvidual i = new Induvidual(HTYPES.SUBJECT(), Race());
                Add(i);
            }
        }

        public Humanoid PopSoldier(int tx, int ty, HTYPE type)
        {
            Induvidual t = all[all.Count - 1];
            Humanoid h = SETT.HUMANOIDS().Create(t.Race(), tx, ty, type, CAUSE_ARRIVES.SOLDIER_RETURN());
            if (!h.IsRemoved())
            {
                STATS.Arrive(h);
                h.Indu().CopyFrom(t);
                for (int i = 0; i < STATS.EQUIP().AllE().Count; i++)
                {
                    Equip e = STATS.EQUIP().AllE()[i];
                    e.Set(h.Indu(), 0);
                }
            }

            Remove(t);
            for (int i = 0; i < STATS.EQUIP().AllE().Count; i++)
            {
                Equip e = STATS.EQUIP().AllE()[i];
                e.Set(t, 0);
            }

            STATS.NEEDS().INJURIES.COUNT.Indu().Set(t, 0);

            return h;
        }

        public override int Type()
        {
            return type;
        }

        public void Age()
        {
            for (int k = 0; k < all.Count; k++)
            {
                Induvidual i = all[k];
                STATS.POP().age.DAYS.Inc(i, 1);
                if (STATS.POP().age.ShouldDieOfOldAge(i))
                {
                    Remove(i);
                }
                else if (STATS.WORK().RET.ShouldRetire(i))
                {
                    Remove(i);
                    COORDINATE c = SETT.ENTRY().Points.RandomReachable();
                    if (c != null)
                    {
                        Humanoid h = SETT.HUMANOIDS().Create(i.Race(), c.X, c.Y, HTYPES.RETIREE(), CAUSE_ARRIVES.SOLDIER_RETURN());
                        if (h != null)
                        {
                            h.Indu().CopyFrom(i);
                            STATS.REL().SetHumanoid(h);
                        }
                    }
                }
            }
        }

        public override bool NeedSupplies()
        {
            return true;
        }

        public override int BannerI()
        {
            return Div().Info.BannerI();
        }

        public override void BannerSet(int bi)
        {
            Div().Info.BannerISet(bi);
        }

        public override DivGeneration Generate()
        {
            DivGeneration res = new DivGeneration(this, all, Target());
            return res;
        }

        public override COLOR Color()
        {
            return col;
        }

        public override DIV_SETTING Target()
        {
            return Div().Info;
        }
    }
}