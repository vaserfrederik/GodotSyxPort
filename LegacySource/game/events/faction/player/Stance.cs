using System;
using System.Text;
using game.boosting;
using game.events.faction.player.EventDiplomacy;
using game.faction;
using game.faction.diplomacy;
using game.faction.diplomacy.deal;
using game.faction.npc;
using game.faction.npc.stockpile;
using game.faction.royalty.opinion;
using game.time;
using init.race;
using settlement.main;
using settlement.stats;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.text;
using view.ui.diplomacy;
using view.ui.message;
using world.region;

namespace game.events.faction.player
{
    public static class Stance
    {
        private static readonly CharSequence ¤¤Welcome = "Welcome";
        private static readonly CharSequence ¤¤AgreementCancelled = "¤Agreement Cancelled.";
        private static readonly CharSequence ¤¤AgreementCancelledD = "¤This faction has gone from the stance of {0} to the stance of {1}.";

        private static readonly CharSequence ¤¤Warning = "¤Relations Worsen.";
        private static readonly CharSequence ¤¤WarningD = "¤This faction is currently your {0}. If their opinion is not raised in time, it is possible they'll cancel this agreement.";

        private static readonly CharSequence ¤¤TradeCancelled = "¤Agreements Cancelled.";
        private static readonly CharSequence ¤¤TradeCancelledD = "¤Since the faction of {0} is no longer reachable to us, all agreements have been annulled.";

        private static readonly CharSequence ¤¤title = "Proposal: {0}";

        static Stance()
        {
            D.ts(typeof(Stance));
        }

        public static bool Process(FactionNPC fa, Induvidual king, EData data)
        {
            if (DIP.SecondSinceStance(fa) < TIME.SecondsPerDay())
            {
                return false;
            }

            if (!RD.DIST().Reachable(fa))
            {
                return false;
            }

            KingMessages m = king.Race().KingMessage();

            if (DIP.Get(fa).Trades && !RD.DIST().Reachable(fa))
            {
                DIP.NEUTRAL().Set(fa, FACTIONS.Player());
                new MessageText(¤¤TradeCancelled, Str.TMP.Clear().Add(¤¤TradeCancelledD).Insert(0, fa.Name)).Send();
                return true;
            }

            double opinion = ROPINION.Get(fa);

            if (DIP.TRADE().Is(fa))
            {
                if (opinion < DIP.TRADE().OpinionNeeded * 0.75)
                {
                    return MessDown(fa, DIP.NEUTRAL(), DIP.TRADE(), data);
                }
                return false;
            }

            if (DIP.PACT().Is(fa))
            {
                if (opinion < DIP.PACT().OpinionNeeded * 0.75)
                {
                    return MessDown(fa, DIP.TRADE(), DIP.PACT(), data);
                }
                return false;
            }

            if (DIP.ALLY().Is(fa))
            {
                if (opinion < DIP.ALLY().OpinionNeeded * 0.75)
                {
                    return MessDown(fa, DIP.PACT(), DIP.ALLY(), data);
                }
            }

            if (!SETT.ROOMS().IMPORT.Reqs.Passes(FACTIONS.Player()))
            {
                return false;
            }

            if (!data.Welcomed && DIP.NEUTRAL().Is(fa))
            {
                if (!RND.OneIn(4))
                {
                    return false;
                }

                if (ROPINION.Get(fa) > 0.4)
                {
                    Deal d = DIP.TMP();
                    d.SetFactionAndClear(fa);
                    double max = GiftWorth(fa);
                    if (max > 0)
                    {
                        DealDrawfter.Draft(d, max, false, false);
                        if (d.HasDeal())
                        {
                            data.Welcomed = true;
                            new UIDipMessDeal(¤¤Welcome, m.GREETING_GOOD.Get(fa), d, 0, -0.1).Send();
                            return true;
                        }
                    }
                }
                new UIDipMess(¤¤Welcome, m.GREETING_BAD.Get(fa), "", fa).Send();
                data.Welcomed = true;
                return false;
            }

            bool chance = RND.OneIn(32 * (1 + RD.DIST().Neighs().Size()));

            if (!chance)
            {
                return false;
            }

            if (DIP.NEUTRAL().Is(fa))
            {
                if (opinion > DIP.TRADE().OpinionNeeded + 0.5)
                {
                    MessUp(fa, DIP.TMP().Bools.TRADE, DIP.TRADE());
                    return true;
                }
            }

            if (DIP.TRADE().Is(fa))
            {
                if (opinion > DIP.PACT().OpinionNeeded + 0.5)
                {
                    MessUp(fa, DIP.TMP().Bools.PACT, DIP.TRADE());
                    return true;
                }
            }

            if (DIP.PACT().Is(fa))
            {
                if (opinion > DIP.ALLY().OpinionNeeded + 0.5)
                {
                    MessUp(fa, DIP.TMP().Bools.ALLY, DIP.TRADE());
                    return true;
                }
            }

            return false;
        }

        private static bool MessDown(FactionNPC fa, DipStance downTo, DipStance current, EData data)
        {
            if (fa.Request.Has())
            {
                return false;
            }
            KingMessages m = fa.Court().King().Roy().Induvidual.Race().KingMessage();
            if (data.StanceMess)
            {
                Str.TMP.Clear().Add(¤¤AgreementCancelledD);
                Str.TMP.Insert(0, DIP.Get(fa).Name);
                Str.TMP.Insert(1, downTo.Name);
                new UIDipMess(¤¤AgreementCancelled, m.STANCE_DOWN.Get(fa), Str.TMP, fa).Send();
                downTo.Set(fa);
                data.StanceMess = false;
            }
            else
            {
                Str.TMP.Clear().Add(¤¤WarningD);
                Str.TMP.Insert(0, DIP.Get(fa).Name);

                double more = ROPINION.GIFTS().GetGenerosityNeededForOpinion(fa, current.OpinionNeeded + 0.5);

                Deal d = DIP.TMP();
                d.SetFactionAndClear(fa);
                double am = d.GetWorthOfOpinion(more) * 0.9;
                DealDrawfter.Draft(d, -am, false, false);
                if (am > d.ValueCredits())
                {
                    d.Player.Credits.I += am - d.ValueCredits();
                }

                data.StanceMess = true;

                new UIDipMessDeal(¤¤Warning, m.STANCE_WARNING.Get(fa), d, more, 0).Send();
            }
            return true;
        }

        private static void MessUp(FactionNPC fa, DealBool bool, DipStance stance)
        {
            if (fa.Request.Has())
            {
                return;
            }
            Deal d = DIP.TMP();
            d.SetFactionAndClear(fa);
            bool.Set(true);
            double v = -d.ValueCredits();
            double b = v * 0.5 + (0.5 + RND.RFloat());
            DealDrawfter.Draft(d, b, false, true);
            if (v < d.Player.OfferableWorth())
            {
                KingMessages m = fa.Court().King().Roy().Induvidual.Race().KingMessage();
                new UIDipMessDeal(Str.TMP.Clear().Add(¤¤title).Insert(0, stance.Name), m.STANCE_UP.Get(fa), d, 0, -0.1).Send();
            }
        }

        private static double GiftWorth(FactionNPC fa)
        {
            Deal d = DIP.TMP();
            double min = NPCStockpile.AVERAGE_PRICE * 5;
            double max = NPCStockpile.AVERAGE_PRICE * 150;
            max = Math.Min(max, d.NPC.OfferableWorth() * 0.025);
            if (max > min)
            {
                return min + BOOSTABLES.NOBLE().PRIDE.Get(fa.King().Induvidual) * (max - min);
            }
            return 0;
        }
    }
}