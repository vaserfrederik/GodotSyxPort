using game;
using game.boosting;

namespace game.battle.util
{
    public sealed class BattleUtil
    {
        public readonly Power power = new Power();
        private readonly Boosts boosts = new Boosts();
        public readonly DivTypes types = new DivTypes();
        public readonly FightingUtil fight = new FightingUtil();
        public readonly ArmyFormations formations = new ArmyFormations();

        public BattleUtil(GAME game)
        {
        }

        public double Boost(DIV_SPEC div, Boostable bo)
        {
            return boosts.Get(div, bo);
        }

        public double BoostMax(Boostable bo)
        {
            return boosts.Max(bo);
        }
    }
}