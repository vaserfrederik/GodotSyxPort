using System;
using System.IO;
using game.VERSION;
using game.faction.FACTIONS;
using game.faction.Faction;
using game.faction.npc.FactionNPC;
using game.faction.royalty.opinion.ROPINION;
using game.faction.trade.FBUYER;
using game.faction.trade.FSELLER;
using game.faction.trade.TradeManager;
using init.trade.TRADABLE;
using init.trade.TRADE_TYPE;
using settlement.main.SETT;
using settlement.recipe.Recipe;
using snake2d.util.file.FileGetter;
using snake2d.util.file.FilePutter;
using snake2d.util.file.SAVABLE;
using snake2d.util.misc.CLAMP;
using world.region.RD;

namespace game.faction.npc.stockpile
{
    public sealed class NPCRes : SAVABLE
    {
        //static readonly double PLAYER_AMOUNT = 0.075;

        public readonly TRADABLE res;
        private readonly NPCStockpile s;
        private double offset = 0;
        private double playerOffset;
        private double totRate = 1;
        private double rate = 1;
        private int ri;

        public NPCRes(NPCStockpile f, TRADABLE tradable)
        {
            this.res = tradable;
            this.s = f;
        }

        public override void save(FilePutter file)
        {
            file.d(totRate);
            file.d(rate);
            file.d(offset);
            file.d(playerOffset);
            file.i(ri);
        }

        public override void load(FileGetter file)
        {
            totRate = file.d();
            rate = file.d();
            offset = file.d();
            playerOffset = file.d();
            if (!VERSION.versionIsBefore(71, 5))
                ri = file.i();
        }

        public override void clear()
        {
            totRate = 1;
            rate = 1;
            offset = 0;
            playerOffset = 0;
        }

        void update(TRADABLE re, FactionNPC f)
        {
            Recipe r = SETT.RECIPES().rates.bestRecipe(f, re);
            ri = r.index;
            rate = 1.0 / r.manpower(f);
            totRate = 1.0 / r.manpowerTotal(f);
        }

        public double amount()
        {
            return amountTarget() + offset;
        }

        public double amountTarget()
        {
            return 1 + rate * s.workforce();
        }

        public double rate()
        {
            return rate;
        }

        public double rateTot()
        {
            return totRate;
        }

        public Recipe recipe()
        {
            return SETT.RECIPES().all().get(ri);
        }

        public double priceBase()
        {
            double totRate = rateTot();
            if (totRate == 0)
                return NPCStockpile.AVERAGE_PRICE * 10000;
            return (NPCStockpile.AVERAGE_PRICE / totRate + NPCStockpile.GAME_THEORY);
        }

        public int priceAt(int added)
        {
            double price = amMulAt(added);
            price *= s.creditScore() * priceBase() * s.f.race().pref().priceMul(res);

            if (added > 0)
            {
                return ((int)price) - 1;
            }
            else if (added < 0)
            {
                return (int)(Math.Ceiling(price) + 1);
            }
            else
            {
                return (int)price;
            }
        }

        public double dailyConsumption()
        {
            double overflow = offset();
            if (overflow == 0)
                return 0;
            double target = amountTarget();

            double delta = overflow / target;
            delta = CLAMP.d(delta, -10, 10);

            double am = recipe().aiRecovery * Updater.recoveryRate * overflow;

            if (overflow > 0)
            {
                // Prevent going negative or zero
                am = Math.Max(am, -overflow);
            }
            else // overflow < 0
            {
                // Prevent going positive or zero
                am = Math.Min(am, -overflow);
            }
            return am;
        }

        public int priceSellP()
        {
            return priceAt(-1) + TradeManager.totalFee(s.f, FACTIONS.player(), RD.DIST().distance(s.f), res, 1);
        }

        public int priceBuyP()
        {
            return priceAt(1) - TradeManager.totalFee(FACTIONS.player(), s.f, RD.DIST().distance(s.f), res, 1);
        }

        public double amMulAt(int added)
        {
            double dd = amMul(amount() + added);

            if (added > 0)
            {
                if (dd < 1)
                    return dd;
                return CLAMP.d(dd - 0.4, 1, 2);
            }
            else if (added < 0)
            {
                if (dd > 1)
                    return dd;
                return CLAMP.d(dd + 0.4, 0.5, 1);
            }
            return 1;
        }

        private double amMul(double amount)
        {
            amount = Math.Round(amount);
            double tar = amountTarget();
            if (amount <= 0)
                return NPCStockpile.PRICE_MAX;
            tar /= amount;
            tar = CLAMP.d(tar, NPCStockpile.PRICE_MIN, NPCStockpile.PRICE_MAX);
            return tar;
        }

        public double offset()
        {
            return offset;
        }

        public void inc(double am)
        {
            offset += am;
        }

        public void playerSet(double d)
        {
            playerOffset = d;
        }

        public double playerTraded()
        {
            return playerOffset;
        }

        public double playerTarif(int amount)
        {
            double off = playerTraded();
            double d = pPlayerTarif((int)(Math.Abs(off) + Math.Abs(amount)));
            d += pPlayerTarif((int)(Math.Abs(off)));
            return d * 0.5;
        }

        private double pPlayerTarif(int traded)
        {
            return 0;
        }

        public readonly FBUYER buyer = new FBUYER
        {
            public override int addPrice(int amount)
            {
                if (amount <= 0)
                    return 0;
                return amount * (priceAt(1) + priceAt(amount)) / 2;
            }

            public override void addDeliver(int amount, TRADE_TYPE type)
            {
            }

            public override void addReserve(int amount, TRADE_TYPE type, int price, Faction seller)
            {
                s.f.credits().inc(-price, type.ctype, res, amount);
                s.f.res().inc(res, type.rtype, amount);
                offset += amount;
                if (seller == FACTIONS.player())
                {
                    playerOffset += amount;
                    ROPINION.trade(s.f, price);
                }
            }

            public override double buyPriority(int amount, double price)
            {
                if (int.MaxValue - amount() < amount)
                    return 0;
                return addPrice(amount) / price - 1.0;
            }
        };

        public readonly FSELLER seller = new FSELLER
        {
            public override int removePrice(int amount)
            {
                if (amount <= 0)
                    return 0;
                return amount * (priceAt(-1) + priceAt(-amount)) / 2;
            }

            public override void remove(int amount, TRADE_TYPE type, int price, Faction buyer)
            {
                s.f.credits().inc(price, type.ctype, res, amount);
                s.f.res().inc(res, type.rtype, -amount);
                offset -= amount;
                if (buyer == FACTIONS.player())
                {
                    playerOffset -= amount;
                    ROPINION.trade(s.f, price);
                }
            }

            public override int removeMax()
            {
                return (int)amount();
            }
        };
    }
}