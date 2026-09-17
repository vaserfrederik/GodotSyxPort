using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using game.battle.util;
using game.faction;
using init.constant;
using init.race;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.text;
using world.entity.army;

namespace world.army
{
    public sealed class WDivRegional : ADDiv
    {
        public static int DAYS_TO_TRAIN = 3;
        private static readonly COLOR col = COLOR.ORANGE100.MakeSaturated(0.5).Shade(0.75);
        public static readonly int type = 0;

        private short men;
        private short menTarget;
        private short ri;
        private readonly float[] training;
        private readonly byte[] trainingTarget;
        private float experience;
        private byte trainingDay;
        private short bannerI;
        private readonly byte[] targets;

        public WDivRegional(int index) : base(index)
        {
            training = new float[STATS.BATTLE().TRAINING_ALL.size()];
            trainingTarget = Alloc.bb(STATS.BATTLE().TRAINING_ALL.size());
            targets = Alloc.bb(STATS.EQUIP().BATTLE_ALL().size());
        }

        public void init(Race race, double amount, WArmy a)
        {
            menTarget = (short)CLAMP.i((int)Math.Round(amount * Config.battle().MEN_PER_DIVISION), 0, Config.battle().MEN_PER_DIVISION);
            men = 0;
            ri = (short)race.index;
            training.Fill(0);
            trainingTarget.Fill((byte)0);
            experience = 0;
            trainingDay = 0;
            targets.Fill((byte)0);

            bannerSet(RND.rInt(GAME.ARMIES().banners.size()));
            reassign(a);
        }

        public void randomize(double training, double gear)
        {
            report(-1);

            DivType type = GAME.battle().types.rnd(race(), faction(), RND.rFloat());

            foreach (EquipBattle m in STATS.EQUIP().BATTLE_ALL())
            {
                targets[m.indexMilitary()] = (byte)CLAMP.d(type.equip(m) * gear * m.max(), 0, m.max());
            }

            foreach (StatTraining t in STATS.BATTLE().TRAINING_ALL)
            {
                this.training[t.tIndex] = (byte)CLAMP.d(type.training(t) * training * StatTraining.MAX, 0, StatTraining.MAX);
                trainingTarget[t.tIndex] = (byte)Math.Round(StatTraining.MAX * CLAMP.d(type.training(t) * training, 0, 1));
            }

            bannerSet(RND.rInt(GAME.ARMIES().banners.size()));

            report(1);
        }

        public void copyFrom(DIV_SPEC div)
        {
            report(-1);
            foreach (EquipBattle m in STATS.EQUIP().BATTLE_ALL())
            {
                targets[m.indexMilitary()] = (byte)div.equipI(m);
            }

            foreach (StatTraining t in STATS.BATTLE().TRAINING_ALL)
            {
                this.training[t.tIndex] = (byte)div.training(t) * StatTraining.MAX;
            }
            report(1);
        }

        public override void save(FilePutter file)
        {
            base.save(file);
            file.s(men);
            file.s(menTarget);
            file.s(ri);
            file.bs(trainingTarget);
            file.fs(training);
            file.f(experience);
            file.b(trainingDay);
            file.s(bannerI);
            file.bs(targets);
        }

        public override void load(FileGetter file)
        {
            base.load(file);
            men = file.s();
            menTarget = file.s();
            ri = file.s();
            file.bs(trainingTarget);
            file.fs(training);
            experience = file.f();
            trainingDay = file.b();
            bannerI = file.s();
            file.bs(targets);
        }

        public override int men()
        {
            return men;
        }

        public override int menTarget()
        {
            return menTarget;
        }

        public override void resolve(Induvidual[] hs)
        {
            double exp = 0;
            foreach (Induvidual i in hs)
                exp += STATS.BATTLE().COMBAT_EXPERIENCE.indu().getD(i);
            if (hs.Length > 0)
                exp /= hs.Length;
            resolve(hs.Length, exp);
        }

        public override void resolve(int surviviors, double experiencePerMan)
        {
            int death = men - surviviors;
            menSet(surviviors);

            AD.conscripts().kill(race(), faction(), death);
            report(-1);
            this.experience = (float)CLAMP.d(experiencePerMan, 0, 1);
            report(1);
        }

        public override void menSet(int amount)
        {
            report(-1);
            double exp = experience * men;
            amount = CLAMP.i(amount, 0, Config.battle().MEN_PER_DIVISION);
            men = (short)amount;
            experience = 0;
            if (men > 0)
                experience = (float)CLAMP.d(exp / men, 0, 1);
            report(1);
        }

        protected override void armyChange(WArmy old, WArmy newW)
        {
            if (newW == null)
                AD.regional().retire(this);
        }

        public override Race race()
        {
            return RACES.all().get(ri);
        }

        public override double training(StatTraining tr)
        {
            return training[tr.tIndex] * StatTraining.MAXI;
        }

        public override double equip(EquipBattle e)
        {
            if (army() == null)
                return 0;
            return AD.supplies().get(e).amountValue(army()) * targets[e.indexMilitary()] / e.equipMax;
        }

        public void equipTargetset(EquipBattle e, int t)
        {
            report(-1);
            targets[e.indexMilitary()] = (byte)t;
            report(1);
        }

        public override int type()
        {
            return type;
        }

        public override CharSequence name()
        {
            return Str.TMP.clear().add(Dic.¤¤Regional).insert(0, Dic.¤¤Division);
        }

        public override bool needSupplies()
        {
            return true;
        }

        public override DivGeneration generate()
        {
            return new DivGeneration(this, target);
        }

        public override bool needConscripts()
        {
            return true;
        }

        public override int bannerI()
        {
            return bannerI;
        }

        public override void bannerSet(int bi)
        {
            this.bannerI = (short)bi;
        }

        public override COLOR color()
        {
            return col;
        }

        public final DIV_SPECE target = new DIV_SPECE()
        {
            public double training(StatTraining tr)
            {
                return trainingTarget[tr.tIndex] * StatTraining.MAXI;
            }

            public int men()
            {
                return menTarget;
            }

            public double equip(EquipBattle e)
            {
                return (double)targets[e.indexMilitary()] / e.equipMax;
            }

            public void trainingSet(StatTraining tr, double d)
            {
                trainingTarget[tr.tIndex] = (byte)Math.Round(StatTraining.MAX * CLAMP.d(d, 0, 1));
            }

            public void equipSet(EquipBattle tr, double am)
            {
                report(-1);
                targets[tr.indexMilitary()] = (byte)Math.Round(am * tr.max());
                report(1);
            }

            public void menSet(int am)
            {
                report(-1);
                menTarget = (short)CLAMP.i(am, 0, Config.battle().MEN_PER_DIVISION);
                trainingDay = 0;
                men = (short)CLAMP.i(men, 0, menTarget);
                report(1);
            }

            public double experience()
            {
                return experience;
            }

            public Faction faction()
            {
                return army() == null ? null : army().faction();
            }

            public CharSequence name()
            {
                return WDivRegional.this.name();
            }

            public int bannerI()
            {
                return WDivRegional.this.bannerI();
            }

            public Race race()
            {
                return WDivRegional.this.race();
            }

            public void raceSet(Race race)
            {
                report(-1);
                ri = (short)race.index;
                report(1);
            }

            public void experienceSet(double experience)
            {
                WDivRegional.this.experience = (float)experience;
            }

            public Str nameE()
            {
                return null;
            }

            public void bannerISet(int bannerI)
            {
                WDivRegional.this.bannerI = (short)bannerI;
            }

            public void factionSet(Faction faction)
            {
                // TODO Auto-generated method stub
            }
        };

        public override DIV_SETTING target()
        {
            return target;
        }
    }
}