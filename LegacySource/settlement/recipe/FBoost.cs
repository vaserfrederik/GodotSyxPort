using game.battle.div;
using game.boosting;
using game.faction.npc;
using game.faction.player;
using init.sprite.UI;
using init.type;
using settlement.stats;
using snake2d.util.rnd;
using world.map.regions;

namespace settlement.recipe
{
    class FBoost : BoosterImp
    {
        static readonly double AIBonus = 6.0;
        public double mul = 1.0;

        private readonly Boostable bb;

        public FBoost(Boostable bo) : base(new BSourceInfo(Recipes.¤¤faction, UI.icons().s.world), 0, AIBonus, false)
        {
            this.bb = bo;
            Add(bo);
            Randomize();
        }

        public override double vGet(Region reg)
        {
            return 0;
        }

        public override double vGet(Induvidual indu)
        {
            return 0;
        }

        public override double vGet(Div div)
        {
            return 0;
        }

        public override double vGet(HCLASS_RACE t)
        {
            return 0;
        }

        public override double vGet(Player f)
        {
            return 0;
        }

        public override double vGet(FactionNPC f)
        {
            return f.bonus.GetD(bb) * AIBonus * mul;
        }

        public override double GetValue(double input)
        {
            return input;
        }

        public void Randomize()
        {
            mul = 0.65 + RND.rFloat(0.35);
        }
    }
}