using System;
using System.Collections.Generic;

namespace game.faction.royalty.opinion
{
    public class ROPINION
    {
        static ROPINION self;
        public static string ¤¤name = "Opinion";
        public static string ¤¤desc = "The opinion of a royalty regarding you. High opinion yields better diplomacy and allows for higher stances. Falling below the minimum threshold of opinion of your current stance might lead to the faction breaking it off.";
        public static string ¤¤wEmmi = "Emissary Target";

        static ROPINION()
        {
            D.ts(typeof(ROPINION));
        }

        private readonly OpsStance stance;
        private readonly OpsGifts gifts;
        private readonly OpsEmi emi;
        private readonly OpsOther other;
        private readonly RTrust trust;

        public ROPINION(FACTIONS factions)
        {
            self = this;

            stance = new OpsStance();
            gifts = new OpsGifts();
            emi = new OpsEmi();
            other = new OpsOther();

            GVALUES.FACTION.push("OPINION", ¤¤name, BOOSTABLES.CIVICS().bOpinion.icon, new DOUBLE_O<Faction>()
            {
                public double getD(Faction t)
                {
                    if (t is FactionNPC)
                    {
                        return get((FactionNPC)t);
                    }
                    return 0;
                }
            });

            trust = new RTrust(factions);
        }

        public static SuperBoostable<Royalty> BOOST()
        {
            return GAME.BOOSTS().OPINION;
        }

        public static OpsStance STANCE()
        {
            return self.stance;
        }

        public static OpsGifts GIFTS()
        {
            return self.gifts;
        }

        public static OpsEmi EMMI()
        {
            return self.emi;
        }

        public static OpsOther OTHER()
        {
            return self.other;
        }

        public static double get(FactionNPC f)
        {
            if (f != null && f.court().king() != null)
                return get(f.court().king().roy());
            return 0;
        }

        public static double get(Royalty roy)
        {
            return BOOST().get(roy);
        }

        public static RTrust trust()
        {
            return self.trust;
        }

        public static double get(FactionNPC f, ROpper op, double opValue)
        {
            double old = op.value.getD(f.king());
            op.value.setD(f.king(), opValue);
            double res = get(f);
            op.value.setD(f.king(), old);
            return res;
        }

        static double getOpinionValue(SuperBoostable<Royalty> bo, FactionNPC f, ROpper op, double targetValue)
        {
            Royalty k = f.king();
            double o = op.value.getD(k);
            op.value.setD(k, 0);

            if (op.to() > 0)
            {
                if (bo.get(k) > targetValue)
                {
                    op.value.setD(k, o);
                    return 0;
                }

                double inc = 1.0;

                while (bo.get(k) < targetValue)
                {
                    double prev = bo.get(k);
                    op.value.incD(k, inc);
                    if (prev == bo.get(k))
                        break;
                    if (bo.get(k) > targetValue)
                    {
                        op.value.incD(k, -inc);
                        inc /= 2;
                    }
                }
            }
            else
            {
                if (bo.get(k) < targetValue)
                {
                    op.value.setD(k, o);
                    return 0;
                }

                double inc = 1.0;

                while (bo.get(k) > targetValue)
                {
                    double prev = get(f);
                    op.value.incD(k, inc);
                    if (prev == bo.get(k))
                        break;
                    if (bo.get(k) < targetValue)
                    {
                        op.value.incD(k, -inc);
                        inc /= 2;
                    }
                }
            }

            double v = op.value.getD(k);
            op.value.setD(k, o);
            return v;
        }

        public static double tradeCost(FactionNPC f)
        {
            return DIP.get(f).tarif;
        }

        public static void trade(FactionNPC f, int price)
        {
        }
    }
}