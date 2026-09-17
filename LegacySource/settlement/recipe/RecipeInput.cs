using game.boosting;
using init.trade;

namespace settlement.recipe
{
    public class RecipeInput
    {
        public readonly TRADABLE res;
        public readonly double rate;
        public readonly Boostable boost;

        public RecipeInput(TRADABLE res, double rate, Boostable boost)
        {
            this.res = res;
            this.rate = rate;
            this.boost = boost;
        }
    }
}