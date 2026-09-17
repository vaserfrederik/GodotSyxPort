using System;
using game;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.path.finders;
using settlement.thing.ThingsCorpses;
using snake2d.util.rnd;
using util.text;
using view.main;

namespace settlement.entity.humanoid.ai.crime
{
    final class SerialKiller : AIPLAN.PLANRES
    {
        public SerialKiller(string key) : base(key)
        {
            // TODO Auto-generated constructor stub
        }

        private static readonly CharSequence ¤¤verb = "¤Walking around";

        static
        {
            D.ts(typeof(SerialKiller));
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            d.planByte1 = 0;
            d.planByte2 = 0;
            d.otherEntitySet(null);

            return go.set(a, d);
        }

        private readonly Resumer go = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (isVictim(d.otherEntity()))
                {
                    return leave.set(a, d);
                }

                if (d.planByte1++ > 8)
                    return null;

                if (SETT.PATH().finders().randomDistanceAway.find(a.tc().x(), a.tc().y(), d.path, 64, SFinderRND.otherPeople))
                {
                    return AI.SUBS().walkTo.pathFull(a, d);
                }
                return stand.set(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return stand.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub

            }
        };

        private readonly Resumer stand = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                d.planByte2 = (byte)(6 + RND.rInt(8));
                return res(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                d.planByte2--;
                if (d.planByte2 < 0)
                    return go.set(a, d);

                return AI.SUBS().STAND.activateRndDir(a, d, 2 + RND.rInt(4));
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub

            }
        };

        private readonly Resumer leave = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (SETT.PATH().finders().randomDistanceAway.find(a.tc().x(), a.tc().y(), d.path, 64, SFinderRND.noPeople))
                {
                    return AI.SUBS().walkTo.pathFull(a, d);
                }
                return AI.SUBS().STAND.activateRndDir(a, d, 2 + RND.rInt(4));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (!isVictim(d.otherEntity()))
                    return null;

                if (VIEW.s().getWindow().zoomout() <= 1 && VIEW.s().getWindow().pixels().touches(d.otherEntity()))
                    return null;

                Humanoid v = d.otherEntity();
                int tx = v.tc().x();
                int ty = v.tc().y();
                v.kill(false, CAUSE_LEAVES.MURDER());
                Corpse c = SETT.THINGS().corpses.tGet.get(tx, ty);
                if (c != null)
                    GAME.events().killer.reportKill(c);
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub

            }
        };

        public override bool event(Humanoid a, AIManager d, HEventData e)
        {
            if (d.planByte1 < 8 && e.event == HEvent.MEET_HARMLESS)
            {
                if (!isVictim(d.otherEntity()) && isVictim(e.other))
                {
                    d.otherEntitySet((Humanoid)e.other);
                }
            }
            return base.event(a, d, e);
        }

        private bool isVictim(ENTITY e)
        {
            if (e is Humanoid)
            {
                Humanoid o = (Humanoid)e;
                if (o.indu().clas().player && o.race() == GAME.events().killer.victimRace())
                {
                    return true;
                }
            }
            return false;
        }

        protected override void cancel(Humanoid a, AIManager d)
        {
            base.cancel(a, d);
        }
    }
}