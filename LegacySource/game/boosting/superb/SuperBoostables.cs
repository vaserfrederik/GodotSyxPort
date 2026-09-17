using game;
using game.boosting;
using game.faction.royalty;

namespace game.boosting.superb
{
    public class SuperBoostables
    {
        public SuperBoostable<Royalty> OPINION = new SuperBoostable<Royalty>(BOOSTABLES.CIVICS().bOpinion);
        public SuperBoostable<Royalty> TRUST = new SuperBoostable<Royalty>(BOOSTABLES.CIVICS().TRUST);

        public SuperBoostables(GAME game)
        {
            
        }
    }
}