using System;
using System.IO;
using game.debug;
using snake2d.util.file;
using view.main;
using world.WORLD;
using world.battle.Side;
using world.entity.army;
using world.map.regions;

namespace world.battle
{
    public sealed class WBattles : WorldResource
    {
        public static readonly double retreatPenalty = 0.4;

        private readonly PRegAttack regAttack;
        private readonly PFieldBattle poller;
        private readonly PSiege siege;

        public WBattles() : base("battles", "BATTLES")
        {
            Util u = new Util();
            Conflict c = new Conflict();

            Resolver pro = new Resolver();
            regAttack = new PRegAttack(c, pro, u);
            poller = new PFieldBattle(c, pro, u);
            siege = new PSiege(u, c, pro);
            new Tests();
        }

        private readonly WorldResourceManager saver = new WorldResourceManager()
        {
            public void Save(FilePutter file)
            {
                regAttack.Save(file);
                poller.Save(file);
                siege.Save(file);
            }

            public void Load(FileGetter file)
            {
                regAttack.Load(file);
                poller.Load(file);
                siege.Load(file);
            }

            public void Clear()
            {
                regAttack.Clear();
                poller.Clear();
                siege.Clear();
            }
        };

        public override WorldResourceManager Saver()
        {
            return saver;
        }

        public override void Update(double ds, Profiler prof)
        {
            prof.LogStart(this);
            siege.Update(ds);
            prof.LogEnd(this);
        }

        public void Poll()
        {
            int death = 0;
            while (CanPoll())
            {
                if (!regAttack.Poll())
                    break;
                if (death++ > 1000)
                    throw new RuntimeException();
            }

            death = 0;
            while (CanPoll())
            {
                if (!poller.Poll())
                    break;
                if (death++ > 1000)
                    throw new RuntimeException();
            }

            death = 0;
            while (CanPoll())
            {
                if (!siege.Poll())
                    break;
                if (death++ > 1000)
                    throw new RuntimeException();
            }
        }

        private bool CanPoll()
        {
            return !VIEW.B().IsActive() && !VIEW.World().UI.Battle.IsBusty();
        }

        public double BesigedTime(Region reg)
        {
            return siege.BesigedTime(reg);
        }

        public bool Besiged(Region reg)
        {
            return siege.Besiged(reg);
        }

        public void Besige(WArmy a, Region reg)
        {
            siege.Besige(a, reg);
        }

        public void RegAttack(Region reg, WArmy a)
        {
            regAttack.RegAttack(reg, a);
            Report(a);
        }

        public void Report(WArmy a)
        {
            regAttack.Register(a);
            poller.Register(a);
            siege.Register(a);
        }
    }
}