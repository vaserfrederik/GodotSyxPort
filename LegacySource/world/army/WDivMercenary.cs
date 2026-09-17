using System;
using System.IO;
using System.Linq;
using game.battle.util;
using game.faction;
using game.time;
using init.constant;
using init.race;
using init.resources;
using init.trade;
using init.type;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using world.entity.army;
using world.region;
using world.region.pop;

namespace world.army
{
    public sealed class WDivMercenary : ADDiv
    {
        private static readonly int Type = 2;
        private static readonly COLOR Col = COLOR.ORANGE100.MakeSaturated(0.5).Shade(0.75);
        private byte Race;
        private short Men;
        private short MenTarget;
        private readonly DIV_SETTINGImp Spec = new DIV_SETTINGImp();
        private float Exp;
        private int CostPerMan;
        private short NameI;
        private short BannerI;
        private Induvidual Indu;
        private float DisbandTime = 0;
        private byte MissedPayments;

        public WDivMercenary(int index) : base(index)
        {
        }

        private void Randomize()
        {
            Report(-1);

            Race = (byte)RACES.All().Rnd().Index;

            double am = 0;
            foreach (RDRace r in RD.RACES().All)
            {
                am += (0.1 + r.Race.Physics.Raiding) * r.Pop.Faction().Get(FACTIONS.Player()) + 1;
            }
            double ri = RND.rFloat() * am;
            foreach (RDRace r in RD.RACES().All)
            {
                ri -= (0.1 + r.Race.Physics.Raiding) * r.Pop.Faction().Get(FACTIONS.Player()) + 1;
                if (ri <= 0)
                {
                    Race = (byte)r.Race.Index;
                    break;
                }
            }

            int dmen = 15;
            int ma = (int)Math.Ceiling((double)Config.Battle().MenPerDivision / dmen);
            Men = (short)CLAMP.I(dmen * RND.rInt(ma), dmen, Config.Battle().MenPerDivision);
            MenTarget = Men;

            Exp = (float)CLAMP.D(Math.Pow(RND.rFloat(), 1.5), 0, 1);

            NameI = (short)RND.rInt(Race().Info.ArmyNames.Count);

            DivType type = GAME.Battle().Types.Rnd(Race(), null, RND.rFloat());

            Spec.CopySettings(type, MenTarget, 0.5 + 0.5 * RND.rFloat(), 0.1 + 0.9 * RND.rFloat());

            BannerSet(RND.rShort());

            {
                double costBase = Race().Physics.AdultDay * FACTIONS.PRICE().Edible();
                foreach (StatTraining t in STATS.BATTLE().TRAINING_ALL)
                {
                    costBase += t.Room.TRAINING_DAYS * FACTIONS.PRICE().Edible() * Spec.Training(t);
                }
                costBase *= 0.1;
                costBase *= 0.5 + Exp;

                double costEquip = 0;

                foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
                {
                    costEquip += (e.WearRate() / 16.0) * Spec.EquipI(e) * FACTIONS.PRICE().Get(TR.Get(e.Resource));
                }

                double cost = costBase + costEquip;

                foreach (ResSupply s in RESOURCES.SUP().ALL)
                {
                    cost += 1.25 * FACTIONS.PRICE().Get(TR.Get(s.Resource)) * s.ConsumptionPerPersonday * s.ConsumedMulPerDay(Race()) * s.ConsumptionPerPersonday;
                }

                CostPerMan = (int)Math.Ceiling(cost);
            }

            Indu = new Induvidual(HTYPES.SOLDIER(), Race());
            STATS.NEEDS().DIRTINESS.SetD(Indu, RND.rExpo() * 0.5);

            Report(1);
        }

        protected override void ArmyChange(WArmy old, WArmy newW)
        {
            if (newW == null)
            {
                DisbandTime = TIME.SecondsPerDay() * 16 * 2;
            }
            else if (old == null && newW != null && newW.Faction() == FACTIONS.Player())
            {
                MissedPayments = -1; //lastPaymentTime = (int) TIME.currentSecond();
            }
            base.ArmyChange(old, newW);
        }

        public bool Disbanded()
        {
            return DisbandTime > 0;
        }

        public override void Save(FilePutter file)
        {
            base.Save(file);
            file.B(Race);
            file.S(Men);
            file.S(MenTarget);
            file.F(Exp);
            file.F(DisbandTime);
            file.I(CostPerMan);
            file.S(NameI);
            file.S(BannerI);
            Spec.Save(file);
            file.Bool(Indu != null);
            if (Indu != null)
                Indu.Save(file);
            file.B(MissedPayments);
        }

        public override void Load(FileGetter file)
        {
            base.Load(file);
            Race = file.B();
            Men = file.S();
            MenTarget = file.S();
            Exp = file.F();
            DisbandTime = file.F();
            CostPerMan = file.I();
            NameI = file.S();
            BannerI = file.S();
            Spec.Load(file);
            if (file.Bool())
                Indu = new Induvidual(file);
            MissedPayments = file.B();
        }

        public Induvidual Cheif()
        {
            return Indu;
        }

        public override int Men()
        {
            return Men;
        }

        public override void MenSet(int m)
        {
            Report(-1);
            Men = (short)CLAMP.I(m, 0, MenTarget());
            Report(1);
        }

        public override void Resolve(Induvidual[] hs)
        {
            double exp = 0;
            foreach (Induvidual i in hs)
                exp += STATS.BATTLE().COMBAT_EXPERIENCE.Indu().GetD(i);
            if (hs.Length > 0)
                exp /= hs.Length;
            Resolve(hs.Length, exp);
        }

        public override void Resolve(int surviviors, double experiencePerMan)
        {
            MenSet(surviviors);
            Report(-1);
            Exp = (float)CLAMP.D(experiencePerMan, 0, 1);
            Report(1);
        }

        public override int MenTarget()
        {
            return MenTarget;
        }

        public override double Training(StatTraining tr)
        {
            return Spec.Training[tr.TIndex];
        }

        public override double Experience()
        {
            return Exp;
        }

        public override Race Race()
        {
            return RACES.All().Get(Race & 0x0FF);
        }

        public override int DaysUntilMenArrives()
        {
            return 1;
        }

        public override int CostPerMan()
        {
            return CostPerMan;
        }

        public override int Type()
        {
            return Type;
        }

        public override string Name()
        {
            return Race().Info.ArmyNames[NameI].ToString();
        }

        public override double Equip(EquipBattle e)
        {
            return Spec.Equip[e.IndexMilitary()];
        }

        public override bool NeedSupplies()
        {
            return false;
        }

        public override DivGeneration Generate()
        {
            return new DivGeneration(this, Target);
        }

        public override int BannerI()
        {
            return BannerI;
        }

        public override void BannerSet(int bi)
        {
            BannerI = (short)bi;
        }

        public override COLOR Color()
        {
            return Col;
        }

        private readonly DIV_SETTING Target = new DIV_SETTING()
        {
            public double Training(StatTraining tr)
            {
                return WDivMercenary.this.Training(tr);
            }

            public int Men()
            {
                return MenTarget;
            }

            public double Equip(EquipBattle e)
            {
                return WDivMercenary.this.Equip(e);
            }
        };

        public override DIV_SETTING Target()
        {
            return Target;
        }
    }
}