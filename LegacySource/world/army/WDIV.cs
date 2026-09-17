using System;
using game;
using game.battle.DivisionBanners;
using game.battle.util;
using settlement.stats;
using snake2d.util.color;
using snake2d.util.misc;
using world.entity.army;

namespace world.army
{
    public interface WDIV : DIV_SPEC
    {
        void Resolve(Induvidual[] hs);
        void Resolve(int survivors, double experiencePerMan);

        default void Resolve(int survivors)
        {
            double xp = 0;

            if (survivors > 0)
            {
                xp = (double)0.1 * Men() / survivors;
                xp += Experience();
                xp = CLAMP.d(xp, 0, 1);
            }
            Resolve(survivors, xp);
        }

        DivGeneration Generate();

        default int Process()
        {
            return (int)GAME.Battle().Power.Get(this);
        }

        int DaysUntilMenArrives();
        bool NeedSupplies();

        default DivisionBanner Banner()
        {
            return GAME.ARMIES().Banners.Get(BannerI());
        }

        void BannerSet(int bi);
        int MenTarget();

        default int CostPerMan()
        {
            return 0;
        }

        default bool NeedConscripts()
        {
            return false;
        }

        WArmy Army();

        COLOR Color();

        DIV_SETTING Target();
    }
}