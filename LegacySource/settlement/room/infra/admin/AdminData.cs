using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace settlement.room.infra.admin
{
    public class AdminData : SAVABLE
    {
        private static string ¤¤degradeD = "Degrade per year. Each worker can only produce and maintain so much {0}. Without worker effort, the value will start to degrade with time.";
        private static string ¤¤TargetD = "¤Estimation of how much {0} will be produced.";

        static AdminData()
        {
            D.ts(typeof(AdminData));
        }

        public readonly HistoryInt utilizedHistory = new HistoryInt(64, TIME.days(), true);
        public readonly Boostable target;

        public readonly double knowledgePerStation;
        private readonly double degradeValue;
        private readonly double workSpeed;
        private readonly double workValue;
        private readonly RoomEmployment emps;
        readonly Boostable boost;
        private int stations;
        private double prev = 0;
        private double dayGain = 0;
        private int day = -1;

        public byte usedD = 0;
        private double progress = 0;
        private double workProg = 1;

        private double skill;
        private double skillAmount;
        private int skillUpI = -1;

        public AdminData(RoomEmployment emps, Json json, Boostable boost)
        {
            this.emps = emps;
            this.boost = boost;
            knowledgePerStation = json.d("VALUE_PER_WORKER", 0, 100000);
            double degrade = json.d("VALUE_DEGRADE_PER_YEAR", 0, 10);
            degradeValue = degrade / TIME.years().bitConversion(TIME.days());
            workSpeed = json.d("VALUE_WORK_SPEED", 0, 1000);

            double work = knowledgePerStation * degrade;
            work /= TIME.years().bitSeconds();
            work *= Humanoid.WORK_PER_DAYI;

            workValue = work;

            if (json.has(BOOSTING.MAP().key))
            {
                target = BOOSTING.MAP().read(json);
            }
            else
                target = null;

            if (target != null)
            {
                double max = knowledgePerStation * 1000000;
                double maxI = 1.0 / max;

                BValue v = new BValue.BValuePlayerOnly()
                {
                    public double vGet(Player f) => value() * maxI,
                    public double vGet(FactionNPC f) => 0
                };

                new BoosterValue(v, new BSourceInfo(emps.blueprint().info.names, emps.blueprint().iconBig().small), 0, max, false).add(target);
            }
        }

        public override void save(FilePutter file)
        {
            utilizedHistory.save(file);
            file.d(prev);
            file.d(dayGain);
            file.i(day);
            file.d(skill);
            file.d(skillAmount);
            file.i(skillUpI);
            file.i(stations);
        }

        public override void load(FileGetter file)
        {
            utilizedHistory.load(file);
            prev = file.d();
            dayGain = file.d();
            day = file.i();
            skill = file.d();
            skillAmount = file.d();
            skillUpI = file.i();
            stations = file.i();
            setProgress();
        }

        public override void clear()
        {
            utilizedHistory.clear();
            prev = 0;
            dayGain = 0;
            day = 0;
            stations = 0;
        }

        public void incStations(int am)
        {
            this.stations += am;
        }

        public void perform(double time, double skill)
        {
            double progSpeed = workProg;
            progSpeed = (progSpeed) * workSpeed;
            double am = time * skill * progSpeed * workValue;
            dayGain += am;
            this.skill += skill * emps.proximity();

            skillAmount++;

            int emp = emps.employed();

            if (skillAmount > emp)
            {
                skillUpI = GAME.updateI();
                skillAmount /= 2;
                this.skill /= 2;
            }

            utilizedHistory.set((int)value());
            setProgress();
        }

        public void inc(double am)
        {
            dayGain += am;
            utilizedHistory.set((int)value());
        }

        public void update()
        {
            if (day != TIME.days().bitsSinceStart())
            {
                day = TIME.days().bitsSinceStart();
                prev += dayGain;
                prev *= (1 - degradeValue);
                dayGain = 0;
                utilizedHistory.set((int)value());
            }
        }

        public double value()
        {
            double vv = (prev + dayGain) * (1 - degradeValue);
            vv = Math.Max(prev, vv);
            return vv;
        }

        public double projection()
        {
            return skill() * emps.employed() * knowledgePerStation * emps.totEff() / emps.proximity();
        }

        public double perEmployee()
        {
            if (emps.employed() == 0)
                return knowledgePerStation;
            return skill() * knowledgePerStation * emps.totEff() / emps.proximity();
        }

        public double skill()
        {
            if (skillAmount == 0)
                return 1;
            return this.skill / skillAmount;
        }

        private void setProgress()
        {
            progress = value() / (skill() * (emps.neededWorkers()) * knowledgePerStation);
            progress = CLAMP.d(progress, 0, 1);
            workProg = 1 - progress * progress;
            usedD = (byte)(progress * 255);
        }

        public static class Gui<A, B> : UIRoomModuleImp<A, B> where A : RoomInstance where B : RoomBlueprintIns<A>
        {
            private readonly GChart chart = new GChart();
            private readonly AdminData data;
            private readonly IndustryRate indu;
            private readonly string name;
            private readonly string targetD;

            public Gui(B s, AdminData data, IndustryRate out, string name, string targetD) : base(s)
            {
                this.data = data;
                this.indu = out;
                this.name = name;
                this.targetD = targetD;
            }

            public override void appendTableFilters(List<GTFilter<RoomInstance>> filters, List<GTSort<RoomInstance>> sorts, List<UIRoomBulkApplier> appliers)
            {
            }

            public override void hover(GBox box, A i)
            {
                base.hover(box, i);
                box.NL(8);
                box.textLL(name);
                box.add(GFORMAT.i(box.text(), (int)data.value()));
            }

            public override void appendMain(GGrid r, GGrid text, GuiSection sExtra)
            {
                GuiSection ss = new GuiSection();

                ss.add(new GStat()
                {
                    public void update(GText text) => GFORMAT.f0(text, data.value(), 2),

                    public void hoverInfoGet(GBox b)
                    {
                        chart.clear();
                        chart.add(data.utilizedHistory);
                        b.add(chart);
                    }
                }.hh(name));

                ss.addDown(2, new GStat()
                {
                    public void update(GText text) => GFORMAT.perc(text, data.degradeValue * TIME.years().bitConversion(TIME.days())),

                    public void hoverInfoGet(GBox b)
                    {
                        GText t = b.text();
                        t.add(¤¤degradeD);
                        t.insert(0, name);
                        b.add(t);
                    }
                }.hh(Dic.¤¤Degrade));

                ss.addDown(2, new GStat()
                {
                    public void update(GText text) => GFORMAT.f0(text, data.projection(), 2),

                    public void hoverInfoGet(GBox b)
                    {
                        b.text(targetD);
                        b.add(GFORMAT.i(b.text(), (int)data.projection()));
                    }
                }.hh(Dic.¤¤Target));

                if (S.get().developer)
                {
                    ss.addDown(2, new GButt.ButtPanel("++")
                    {
                        protected override void clickA()
                        {
                            data.cheatAdd(50);
                            base.clickA();
                        }
                    });

                    ss.addDown(0, new GButt.ButtPanel("--")
                    {
                        protected override void clickA()
                        {
                            data.clear();
                            base.clickA();
                        }
                    });
                }

                ss.body().incrW(64);
                text.add(ss);
            }
        }

        public void cheatAdd(int baseUnits)
        {
            inc(TIME.secondsPerDay() * baseUnits * workValue);
        }

        public interface ROOM_ADMIN_HOLDER
        {
            AdminData admin();
        }
    }
}