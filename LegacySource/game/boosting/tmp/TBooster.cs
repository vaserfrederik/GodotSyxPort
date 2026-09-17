using System;
using game;
using game.battle.div;
using game.boosting;
using game.faction;
using game.faction.npc;
using game.faction.player;
using init.sprite.UI;
using init.type;
using settlement.stats;
using util.text;
using world.map.regions;

namespace game.boosting.tmp
{
    class TBooster : Booster
    {
        private static readonly string ¤¤name = "Other Effects";

        static TBooster()
        {
            D.ts(typeof(TBooster));
        }

        private readonly double from;
        private readonly double to;
        private readonly BValue value;

        public TBooster(Boostable target, double min, double max, bool isMul) : base(new BSourceInfo(¤¤name + (isMul ? " (*)" : ""), UI.icons().s.question), isMul)
        {
            this.from = min;
            this.to = max;
            value = isMul ? new VBMul(target) : new VBAdd(target);
            add(target);
        }

        public override double from()
        {
            return from;
        }

        public override double to()
        {
            return to;
        }

        public override double getValue(double input)
        {
            return input;
        }

        protected override double pget(BOOSTABLE_O o)
        {
            return o.boostableValue(value);
        }

        private static class VBAdd : BValue
        {
            private readonly Boostable target;

            public VBAdd(Boostable target)
            {
                this.target = target;
            }

            public override double vGet(FactionNPC f)
            {
                return GAME.BOOST().factions.add(f, target);
            }

            public override double vGet(Player f)
            {
                return GAME.BOOST().factions.add(f, target);
            }

            public override double vGet(HCLASS_RACE t)
            {
                if (t.cl == null || t.cl.player)
                {
                    return GAME.BOOST().popcl.add(t, target) + GAME.BOOST().factions.add(FACTIONS.player(), target);
                }
                return GAME.BOOST().popcl.add(t, target);
            }

            public override double vGet(Div div)
            {
                Faction f = div.faction();
                if (f == FACTIONS.player())
                    return GAME.BOOST().popcl.add(HCLASS_RACE.clP(div.info.race()), target);
                else if (f != null)
                    return GAME.BOOST().factions.add(f, target);
                return 0;
            }

            public override double vGet(Induvidual indu)
            {
                double d = GAME.BOOST().popcl.add(indu.popCL(), target);
                if (indu.player())
                    d += GAME.BOOST().factions.add(FACTIONS.player(), target);
                return d;
            }

            public override double vGet(Region reg)
            {
                if (reg.faction() != null)
                    return GAME.BOOST().regions.add(reg, target) + GAME.BOOST().factions.add(reg.faction(), target);
                return GAME.BOOST().regions.add(reg, target);
            }
        }

        private static class VBMul : BValue
        {
            private readonly Boostable target;

            public VBMul(Boostable target)
            {
                this.target = target;
            }

            public override double vGet(FactionNPC f)
            {
                return GAME.BOOST().factions.mul(f, target);
            }

            public override double vGet(Player f)
            {
                return GAME.BOOST().factions.mul(f, target);
            }

            public override double vGet(HCLASS_RACE t)
            {
                if (t.cl == null || t.cl.player)
                {
                    return GAME.BOOST().popcl.mul(t, target) * GAME.BOOST().factions.mul(FACTIONS.player(), target);
                }
                return GAME.BOOST().popcl.mul(t, target);
            }

            public override double vGet(Div div)
            {
                Faction f = div.faction();
                if (f == FACTIONS.player())
                    return GAME.BOOST().popcl.mul(HCLASS_RACE.clP(div.info.race()), target);
                else if (f != null)
                    return GAME.BOOST().factions.mul(f, target);
                return 0;
            }

            public override double vGet(Induvidual indu)
            {
                return GAME.BOOST().popcl.mul(indu.popCL(), target) * GAME.BOOST().factions.mul(FACTIONS.player(), target);
            }

            public override double vGet(Region reg)
            {
                if (reg.faction() != null)
                    return GAME.BOOST().regions.mul(reg, target) * GAME.BOOST().factions.mul(reg.faction(), target);
                return GAME.BOOST().regions.mul(reg, target);
            }
        }
    }
}