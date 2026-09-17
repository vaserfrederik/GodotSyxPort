using System;
using System.IO;
using game.battle.div;
using game.battle.formation;
using game.battle.thread;
using init.constant;
using snake2d.util.file;
using snake2d.util.sprite.text;

namespace game.battle.thread.order
{
    public sealed class BattleOrders : BattleThread
    {
        private readonly BattleOrder[] orders;
        private readonly DivFormationImp[] nexts;
        private readonly BattleOrderUpdater plans = new BattleOrderUpdater();
        private DivFormationImp tmp = new DivFormationImp();

        public BattleOrders() : base(1.0 / 60)
        {
            orders = new BattleOrder[Config.battle().DIVISIONS_PER_ARMY * 2];
            nexts = new DivFormationImp[Config.battle().DIVISIONS_PER_ARMY * 2];
            for (int i = 0; i < orders.Length; i++)
            {
                orders[i] = new BattleOrder();
                nexts[i] = new DivFormationImp();
            }
        }

        protected override void Save(FilePutter file)
        {
            foreach (BattleOrder o in orders)
                o.Save(file);
            foreach (DivFormationImp f in nexts)
                f.Save(file);
            plans.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            foreach (BattleOrder o in orders)
                o.Load(file);
            foreach (DivFormationImp f in nexts)
                f.Load(file);
            plans.Load(file);
        }

        protected override void Init()
        {
            plans.Clear();
            for (short di = 0; di < orders.Length; di++)
            {
                orders[di].Clear();
            }
            for (short di = 0; di < orders.Length; di++)
            {
                Update(GAME.ARMIES().Division(di));
            }
        }

        protected override void DoThreadJob()
        {
            for (short di = 0; di < orders.Length; di++)
            {
                if (!thread.Working())
                    break;
                Update(GAME.ARMIES().Division(di));
            }
        }

        private void Update(Div div)
        {
            DivFormationImp f = plans.Update(div, orders[div.Index()], nexts[div.Index()]);
            if (f != null)
            {
                tmp.Copy(f);
                DivFormationImp oo = nexts[div.Index()];
                nexts[div.Index()] = tmp;
                tmp = oo;
            }
        }

        public static BattleOrder Get(Div div)
        {
            return GAME.BATTLE_THREADS().Orders.orders[div.Index()];
        }

        public static DivFormationImp Next(Div div)
        {
            return GAME.BATTLE_THREADS().Orders.nexts[div.Index()];
        }

        public void Init(Div div)
        {
            bool started = thread.Working();

            Stop();
            orders[div.Index()].Dest.Get(nexts[div.Index()]);

            orders[div.Index()].Task.Set(new BattleOrderTask().Stop(div));
            if (started)
                Start();
        }

        public static void Debug(Div div, Str text)
        {
            BattleOrders s = GAME.BATTLE_THREADS().Orders;
            s.plans.Debug(div, text);
        }
    }
}