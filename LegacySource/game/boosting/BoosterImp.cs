using System;
using game.battle.div;
using game.faction.npc;
using game.faction.player;
using init.type;
using settlement.stats;
using snake2d.util.misc;
using world.map.regions;

namespace game.boosting
{
    public class BoosterImp : Booster, BValue
    {
        private readonly double from;
        private readonly double to;

        public BoosterImp(BSourceInfo info, double from, double to, bool isMul) : base(info, isMul)
        {
            this.from = from;
            this.to = to;
        }

        public BoosterImp(BSourceInfo info, double to, bool isMul) : base(info, isMul)
        {
            this.to = to;

            if (isMul)
            {
                from = 1;
            }
            else
            {
                from = 0;
            }
        }

        public override double From()
        {
            return from;
        }

        public override double To()
        {
            return to;
        }

        public override double VGet(Region reg)
        {
            return 0;
        }

        public override double VGet(Induvidual indu)
        {
            return 0;
        }

        public override double VGet(Div div)
        {
            return 0;
        }

        public override double VGet(HCLASS_RACE popTime)
        {
            return 0;
        }

        public override double VGet(Player f)
        {
            return 0;
        }

        public override double VGet(FactionNPC f)
        {
            return 0;
        }

        public override double GetValue(double input)
        {
            input = CLAMP.d(input, 0, 1);
            return From() + input * (To() - From());
        }

        public double VNopInput()
        {
            if (IsMul)
            {
                return (1 - From()) / (To() - From());
            }
            else
            {
                return From() / (To() - From());
            }
        }

        protected override double PGet(BOOSTABLE_O o)
        {
            return o.BoostableValue(this);
        }
    }
}