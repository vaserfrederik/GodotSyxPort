using System;
using game.time;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.law.stocks;
using snake2d.util.datatypes;
using util.text;

namespace settlement.entity.humanoid.ai.types.prisoner
{
    class Stocked : AIPLAN.PLANRES
    {
        private static readonly CharSequence ¤¤verb = "Being stocked";

        static Stocked()
        {
            D.ts(typeof(Stocked));
        }

        public Stocked() : base("prisStocked")
        {
            // TODO Auto-generated constructor stub
        }

        private readonly ROOM_STOCKS blue = SETT.ROOMS().STOCKS;

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            AISubActivation s = walk.set(a, d);
            if (s != null)
                return s;

            return null;
        }

        private readonly Resumer walk = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                COORDINATE c = blue.stockReserve();
                if (c == null)
                    return null;
                d.planTile.set(c);
                AISubActivation s = AI.SUBS().walkTo.cooFull(a, d, c);
                if (s == null)
                {
                    can(a, d);
                    return null;
                }
                return s;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return sit.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return blue.stockIsReserved(d.planTile.x(), d.planTile.y());
            }

            public override void can(Humanoid a, AIManager d)
            {
                blue.stockCancel(d.planTile.x(), d.planTile.y());
            }
        };

        private readonly Resumer sit = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                DIR dir = blue.stockDir(d.planTile.x(), d.planTile.y(), a.speed.dir());
                a.speed.setDirCurrent(dir);
                blue.stockUse(d.planTile.x(), d.planTile.y());
                return res(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (TIME.light().nightIs())
                {
                    can(a, d);
                    PrisonerData.self.stocked.setMax(d);
                    return null;
                }
                return AI.SUBS().LAY.activateTime(a, d, 16);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return blue.stockIsReserved(d.planTile.x(), d.planTile.y());
            }

            public override void can(Humanoid a, AIManager d)
            {
                blue.stockCancel(d.planTile.x(), d.planTile.y());
            }
        };
    }
}