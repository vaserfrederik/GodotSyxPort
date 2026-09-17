using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game.Battle.Thread.Order
{
    using static BattleOrderUpdater.Plan.div;

    class BattleOrderUpdater : SAVABLE
    {
        private readonly PlanData[] datas = new PlanData[Config.battle().DIVISIONS_PER_BATTLE];
        private readonly Tools tools = new Tools(datas);

        public readonly ArrayListGrower<Plan> all = new ArrayListGrower<Plan>();
        public readonly PlanWalkToDest walk_to_dest;
        public readonly PlanAttackDiv attack;
        public readonly PlanAttackTile attackTile;
        public readonly PlanCharge charge;
        public readonly PlanFireDiv range;
        public readonly PlanStop stop;

        public readonly Plan[] planmap = new Plan[DIVTASK.all.size()];

        public BattleOrderUpdater()
        {
            Data d = null;
            int lc = 0;

            d = new Data();
            walk_to_dest = new PlanWalkToDest(tools, all, d);
            if (d.longCount() > lc)
                lc = d.longCount();

            d = new Data();
            attack = new PlanAttackDiv(tools, all, d);
            if (d.longCount() > lc)
                lc = d.longCount();

            d = new Data();
            range = new PlanFireDiv(tools, all, d);
            if (d.longCount() > lc)
                lc = d.longCount();

            d = new Data();
            stop = new PlanStop(tools, all, d);
            if (d.longCount() > lc)
                lc = d.longCount();

            d = new Data();
            attackTile = new PlanAttackTile(tools, all, d);
            if (d.longCount() > lc)
                lc = d.longCount();

            d = new Data();
            charge = new PlanCharge(tools, all, d);
            if (d.longCount() > lc)
                lc = d.longCount();

            for (int i = 0; i < datas.Length; i++)
                datas[i] = new PlanData(lc);

            foreach (Plan p in all)
            {
                if (planmap[p.divtask.ordinal()] != null)
                    throw new RuntimeException();
                planmap[p.divtask.ordinal()] = p;
            }

            foreach (Plan p in planmap)
            {
                if (p == null)
                    throw new RuntimeException();
            }
        }

        public override void save(FilePutter file)
        {
            foreach (PlanData d in datas)
                d.save(file);
        }

        public override void load(FileGetter file)
        {
            foreach (PlanData d in datas)
                d.load(file);
        }

        public override void clear()
        {
            foreach (PlanData d in datas)
                d.clear();
        }

        static int inter;

        public DivFormationImp update(Div div, BattleOrder o, DivFormationImp prev)
        {
            if (div.index() == 0)
                PlanWalkAbs.amountOfPaths = 0;

            o.dest.get(Plan.dest);
            if (!div.active())
            {
                if (Plan.dest.deployed() == 0)
                {
                    Plan.task.stop(div);
                    o.task.set(Plan.task);
                    Plan.dest.clear();
                    return Plan.dest;
                }
                else
                {
                    Plan.task.move(div);
                    o.task.set(Plan.task);
                    return Plan.dest;
                }
            }

            Plan.m = datas[div.index()];
            Plan.order = o;
            Plan.div = div;
            Plan.men = div.menNrOf();
            Plan.unreachable = div.reporter.unreachable();
            Plan.a = div.army();

            Plan.current.copyposition(div.current());
            o.path.get(Plan.path);
            o.task.get(Plan.task);
            Plan.prev.copy(prev);
            Plan.nextPos = null;
            Plan.charging = false;
            Plan.shouldBreak = false;
            Plan.chargeSpeed = false;

            Plan p = plan(Plan.m);

            if (p != Plan.m.plan())
            {
                p.init();
                Plan.m.planI = p.index();
            }

            long now = (long)(TIME.currentSecond() * 1000);
            now &= 0x00FFFFFFF;
            int millis = (int)(now - Plan.m.lastUpdate);

            Plan.m.lastUpdate = (int)now;
            p.update(millis);

            div.settings().charging = Plan.charging;
            div.settings().shouldbreak = Plan.shouldBreak;
            div.settings().chargeSpeed = Plan.chargeSpeed;
            inter++;

            return Plan.nextPos;
        }

        private Plan plan(PlanData d)
        {
            if (div.status().isFighting() && tools.div.isCloseToFighting() && !Plan.task.orderedWhenFighting())
            {
                if (!planmap[Plan.task.task().ordinal()].continueWhenFighting())
                {
                    Plan.task.stop(div);
                    Plan.order.task.set(Plan.task);
                }
            }

            return planmap[Plan.task.task().ordinal()];
        }

        static class Data : DataOSimple<PlanData>
        {
            protected override long[] data(PlanData t)
            {
                return t.data;
            }
        }

        static abstract class Plan : INDEXED
        {
            public readonly Tools t;
            private readonly int index;

            public readonly DIVTASK divtask;

            public static PlanData m;
            public static BattleOrder order;
            public static Div div;
            public static int men;
            public static int unreachable;
            public static Army a;

            public static readonly DivPositionCopyable current = new DivPositionCopyable();
            public static readonly DivFormationImp dest = new DivFormationImp();
            public static readonly BattleOrderTask task = new BattleOrderTask();
            public static readonly BattleOrderPath path = new BattleOrderPath();
            public static readonly DivFormationImp prev = new DivFormationImp();
            public static DivFormationImp nextPos;

            public static bool charging;
            public static bool shouldBreak;
            public static bool chargeSpeed;

            private readonly INT_OE<PlanData> stateI;
            private readonly ArrayListGrower<STATE> states = new ArrayListGrower<>();

            public Plan(Tools tools, LISTE<Plan> all, Data data, DIVTASK task)
            {
                this.t = tools;
                index = all.add(this);
                stateI = data.new DataByte();
                this.divtask = task;
            }

            public int index()
            {
                return index;
            }

            abstract void init();
            abstract void update(int gameMillis);

            abstract bool continueWhenFighting();

            protected STATE state(PlanData m)
            {
                return states.get(stateI.get(m));
            }

            abstract class STATE
            {
                readonly int index = states.add(this);
                public readonly string name;

                STATE(string name)
                {
                    this.name = name;
                }

                bool set()
                {
                    stateI.set(m, index);
                    return setAction();
                }

                abstract bool setAction();

                abstract void update(int gameMillis);

                void debugInfo(Div div, PlanData m, Str text)
                {
                }
            }
        }

        final class PlanData : SAVABLE
        {
            readonly long[] data;
            private int planI = -1;
            private int lastUpdate;

            PlanData(int size)
            {
                this.data = new long[size];
            }

            public Plan plan()
            {
                if (planI == -1)
                    return null;
                return all.get(planI);
            }

            public override void save(FilePutter file)
            {
                file.i(planI);
                file.lsE(data);
                file.i(lastUpdate);
            }

            public override void load(FileGetter file)
            {
                planI = file.i();
                file.lsE(data);
                lastUpdate = file.i();
            }

            public override void clear()
            {
                data.Fill(0);
                planI = -1;
                lastUpdate = 0;
            }
        }

        private int iii = 0;
        private string prevState;
        public void debug(Div div, Str text)
        {
            PlanData d = datas[div.index()];
            Plan p = d.plan();
            if (p == null)
                return;
            text.add(d.plan().GetType().Name);
            text.s();
            text.add('>');
            text.s();
            if (iii++ > 60 || prevState == null)
            {
                prevState = d.plan().state(d).name;
                iii = 0;
            }
            text.add(prevState);
            text.s();
            text.add(d.plan().state(d).name);

            text.NL();
            d.plan().state(d).debugInfo(div, d, text);
        }
    }
}