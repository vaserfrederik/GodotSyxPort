using System;
using game.boosting;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.royalty;
using game.faction.royalty.opinion;
using game.time;
using game.tourism;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.stats;
using snake2d.util.misc;
using util.text;
using world.region;
using world.region.pop;

namespace game.faction.royalty.opinion
{
    public sealed class OpsOther
    {
        private static readonly string ¤¤liberation = "Liberation";
        private static readonly string ¤¤liberationD = "Affection from previous liberation of this faction.";

        private static readonly string ¤¤vassalT = "Vassal Tribute";
        private static readonly string ¤¤vassalTD = "Based on the number of gifts that have been declined.";

        private static readonly string ¤¤proximity = "Nearness";
        private static readonly string ¤¤proximityD = "Based on the distance to this faction, capitol to capitol.";

        private static readonly string ¤¤kinship = "Kinship";
        private static readonly string ¤¤kinshipD = "Based on your race and this royalties race.";

        private static readonly string ¤¤kinshipT = "Kin Treatment";
        private static readonly string ¤¤kinshipTD = "How you are treating this royalty's race and its affiliated races.";

        private static readonly string ¤¤poison = "Rumours";
        private static readonly string ¤¤PosionD = "Rumours about you spread by deceitful factions.";

        public readonly ROpperDown liberation;
        public readonly ROpper proximity;
        public readonly ROpper vassalTribute;
        public readonly ROpper kinship;
        public readonly ROpper kintreatment;
        public readonly ROpper poison;

        static OpsOther()
        {
            D.ts(typeof(OpsOther));
        }

        public OpsOther()
        {
            double year = TIME.secondsPerDay() * 16;
            liberation = new ROpperDown("LIBERATION", ¤¤liberation, ¤¤liberationD, UI.icons().s.flags, 4, false, year * 5);

            proximity = new ROpper("PROXI", ¤¤proximity, ¤¤proximityD, UI.icons().s.wheel, 1, false)
            {
                public override double pget(Royalty roy)
                {
                    return 1.0 - CLAMP.d(RD.DIST().capitolDist(roy.court.faction) / 256.0, 0, 1);
                }

                protected override double ptarget(Royalty bo)
                {
                    return pget(bo);
                }
            };

            poison = new ROpper("POSION", ¤¤poison, ¤¤PosionD, UI.icons().s.death, -20, false)
            {
                public override double increase(Royalty roy)
                {
                    return -1.0 / (year * 2);
                }

                public override double getModifier(Royalty roy)
                {
                    return 1 - 0.5 * BOOSTABLES.NOBLE().HONOUR.get(roy.induvidual);
                }

                protected override double ptarget(Royalty bo)
                {
                    return 0;
                }
            };

            vassalTribute = new ROpper("VASSAL_GIFT", ¤¤vassalT, ¤¤vassalTD, UI.icons().s.gift, 3, false)
            {
                public override double pget(Royalty bo)
                {
                    Royalty roy = (Royalty)bo;
                    if (DIP.overlord(roy.court.faction) == FACTIONS.player())
                    {
                        return base.pget(bo);
                    }
                    return 0;
                }

                protected override double ptarget(Royalty bo)
                {
                    return 0;
                }
            };

            kinship = new ROpper("KINSHIP", ¤¤kinship, ¤¤kinshipD, UI.icons().s.human, 0.75, true)
            {
                public override double getModifier(Royalty roy)
                {
                    return BOOSTABLES.NOBLE().TOLERANCE.get(roy.induvidual);
                }

                public override double pget(Royalty roy)
                {
                    double d = roy.induvidual.race().pref().race(FACTIONS.player().race());
                    d = 1.0 - CLAMP.d(d, 0, 1);
                    return d;
                }

                protected override double ptarget(Royalty bo)
                {
                    return pget(bo);
                }
            };

            kintreatment = new ROpper("KIN_TREATMENT", ¤¤kinshipT, ¤¤kinshipTD, UI.icons().s.human, 0.5, true)
            {
                public override double getModifier(Royalty roy)
                {
                    return 1.0 - 0.5 * BOOSTABLES.NOBLE().TOLERANCE.get(roy.induvidual);
                }

                public override double pget(Royalty roy)
                {
                    double c = 0;
                    Race ra = roy.induvidual.race();
                    c += STATS.MULTIPLIERS().PROSECUTION.value(HCLASSES.CITIZEN(), ra, 0);
                    c += 20 * POP.tot(HCLASSES.SLAVE(), ra) / (1 + POP.tot(null, null));
                    RDRace rr = RD.RACE(ra);
                    if (rr != null)
                    {
                        c += RD.RACES().edicts.sanction.realm(rr).getD(FACTIONS.player()) * 0.25;
                        c += RD.RACES().edicts.exile.realm(rr).getD(FACTIONS.player()) * 0.5;
                        c += RD.RACES().edicts.massacre.realm(rr).getD(FACTIONS.player());
                    }

                    if (!TOURISM.permit(ra))
                    {
                        c += 0.1;
                    }

                    c = CLAMP.d(c, 0, 1);
                    return c;
                }

                protected override double ptarget(Royalty bo)
                {
                    return 0;
                }
            };
        }

        public void liberate(FactionNPC f)
        {
            foreach (Royalty r in f.court().all())
            {
                liberation.value.setD(r, r.isKing() ? 1.0 : 0.5);
            }
        }

        public void acceptTribute(FactionNPC f, bool accept)
        {
            double v = vassalTribute.value.getD(f.king()) + (accept ? -0.25 : 0.25);
            v = CLAMP.d(v, -1, 1);

            vassalTribute.value.setD(f.king(), v);
        }

        public void poison(FactionNPC f, double amount)
        {
            amount /= -poison.to();
            poison.value.incD(f.king(), amount);
        }
    }
}