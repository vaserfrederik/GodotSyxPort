using System;
using System.IO;

namespace game.battle.thread.trajectory
{
    public sealed class BattleTrajectories : BattleThread
    {
        private int divI = 0;
        private double currentSecond = TIME.currentSecond();
        private readonly UpdaterTraj up = new UpdaterTraj();
        private readonly UpdaterArtillery art = new UpdaterArtillery();

        private readonly DivTrajectory[] all;
        private readonly Request[] request;

        public BattleTrajectories() : base(1.0 / 60.0)
        {
            all = new DivTrajectory[Config.battle().DIVISIONS_PER_BATTLE];
            request = new Request[Config.battle().DIVISIONS_PER_BATTLE];

            for (int i = 0; i < all.Length; i++)
            {
                all[i] = new DivTrajectory();
                request[i] = new Request();
            }
        }

        public override void Save(BinaryWriter file)
        {
            file.Write(divI);
            foreach (DivTrajectory t in all)
                t.Save(file);
            foreach (Request r in request)
                r.Save(file);
        }

        public override void Load(BinaryReader file)
        {
            divI = file.ReadInt32();
            foreach (DivTrajectory t in all)
                t.Load(file);
            foreach (Request r in request)
                r.Load(file);
        }

        protected override void Init()
        {
            foreach (DivTrajectory t in all)
                t.Clear();
            foreach (Request r in request)
                r.Clear();
            currentSecond = TIME.currentSecond();
        }

        public void Clear()
        {
            divI = 0;
        }

        protected override void DoThreadJob()
        {
            double curr = TIME.currentSecond();
            double ds = (curr - currentSecond) * Config.battle().DIVISIONS_PER_BATTLE;
            if (ds > 0)
            {
                int old = divI;
                while (ds > 0)
                {
                    if (divI == Config.battle().DIVISIONS_PER_BATTLE)
                        art.Update();
                    else
                    {
                        DivTrajectory n = up.Update(request[divI], GAME.ARMIES().division((short)divI), all[divI]);
                        all[divI] = n;
                    }
                    divI++;

                    divI %= Config.battle().DIVISIONS_PER_BATTLE + 1;
                    if (divI == old)
                        break;
                    ds -= 1;
                }
                currentSecond = TIME.currentSecond();
            }
        }

        public static Trajectory Request(Humanoid h, Div div)
        {
            int pos = div.reporter.positionSpot(h);
            if (GAME.BATTLE_THREADS().trajs.request[div.index()].Request(pos, h, div))
            {
                return GAME.BATTLE_THREADS().trajs.all[div.index()].Get(pos, h);
            }
            return null;
        }

        public static void Register(Humanoid h, Div div)
        {
            GAME.BATTLE_THREADS().trajs.request[div.index()].Request(div.reporter.positionSpot(h), h, div);
        }

        public static int Trajectories(Div div)
        {
            return GAME.BATTLE_THREADS().trajs.all[div.index()].targets;
        }

        public static bool HasPotential(Div div)
        {
            return GAME.BATTLE_THREADS().trajs.all[div.index()].potential;
        }
    }
}