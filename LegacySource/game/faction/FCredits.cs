using System;
using System.IO;
using game.boosting;
using game.time;
using init.trade;
using init.type;
using snake2d.util.file;
using snake2d.util.rnd;
using util.data;
using util.statistics;
using util.text;

namespace game.faction
{
    public class FCredits : FactionResource, DOUBLE
    {
        protected double credits;

        private static readonly CharSequence ¤¤Treasury = "¤Treasury";
        private static readonly CharSequence ¤¤TreasuryD = "¤The amount of Denari at disposal.";

        private static readonly CharSequence ¤¤TRADE = "Trade";
        private static readonly CharSequence ¤¤TRADED = "Money that flow through imports and exports.";

        private static readonly CharSequence ¤¤INFLATION = "Inflation";
        private static readonly CharSequence ¤¤INFLATIOND = "Over time, Inflation adds credits to a negative treasury and removes credits from a positive one.";
        private static readonly CharSequence ¤¤MISC = "Misc";
        private static readonly CharSequence ¤¤MISCD = "Special sources";
        private static readonly CharSequence ¤¤TRIBUTE = "Tribute";
        private static readonly CharSequence ¤¤TRIBUTED = "Denari spent/gained from paying off other armies and factions.";

        private static readonly CharSequence ¤¤DIPLOMACYD = "Denari spent/gained from diplomacy with other factions.";
        private static readonly CharSequence ¤¤MERCINARIES = "Mercenaries";
        private static readonly CharSequence ¤¤MERCINARIESD = "Mercenaries can be conscripted into your armies and cost credits to upkeep each day.";
        private static readonly CharSequence ¤¤TOURISM = "Tourism";
        private static readonly CharSequence ¤¤TOURISMD = "Tourists that visit your city will give you some money at the end of their stay.";
        private static readonly CharSequence ¤¤CONSTRUCTION = "Construction";
        private static readonly CharSequence ¤¤CONSTRUCTIOND = "Construction of buildings in your kingdom.";
        private static readonly CharSequence ¤¤TAX = "Tax";
        private static readonly CharSequence ¤¤TAXD = "Taxation from the realm.";
        private static readonly CharSequence ¤¤SLAVESD = "Transactions from slave trade.";

        static
        {
            D.ts(typeof(FCredits));
        }

        private readonly HistoryInt creditsH;

        public FCredits(int saved, TIMECYCLE time)
        {
            creditsH = new HistoryInt(¤¤Treasury, ¤¤TreasuryD, saved, time, true);
        }

        protected override void save(FilePutter file)
        {
            file.d(credits);
            creditsH.save(file);
        }

        protected override void load(FileGetter file)
        {
            credits = file.d();
            creditsH.load(file);
        }

        protected override void clear()
        {
            credits = 0;
            creditsH.clear();
        }

        protected override void update(double ds, Faction f)
        {
            double inf = credits * 0.2 * ds / (TIME.years().bitSeconds() * BOOSTABLES.CIVICS().DEFALTION.get(f));
            int i = (int)inf;

            if (Math.Abs(inf - i) > RND.rFloat())
            {
                i += Math.Sign(i);
            }

            inc(-i, CTYPE.INFLATION);
        }

        public HISTORY_INT creditsH()
        {
            return creditsH;
        }

        public double credits()
        {
            return credits;
        }

        public double getD()
        {
            return credits;
        }

        protected void inccc(double amount)
        {
            credits += amount;
            creditsH.set((int)credits);
        }

        public void set(double amount)
        {
            credits = amount;
        }

        public void inc(double amount, CTYPE t)
        {
            inccc(amount);
        }

        public void inc(double amount, CTYPE t, TRADABLE res, int resAm)
        {
            inccc(amount);
        }

        public enum CTYPE
        {
            TRADE(¤¤TRADE, ¤¤TRADED),
            INFLATION(¤¤INFLATION, ¤¤INFLATIOND),
            MISC(¤¤MISC, ¤¤MISCD),
            TRIBUTE(¤¤TRIBUTE, ¤¤TRIBUTED),

            DIPLOMACY(Dic.¤¤Diplomacy, ¤¤DIPLOMACYD),
            MERCINARIES(¤¤MERCINARIES, ¤¤MERCINARIESD),
            TOURISM(¤¤TOURISM, ¤¤TOURISMD),
            CONSTRUCTION(¤¤CONSTRUCTION, ¤¤CONSTRUCTIOND),
            TAX(¤¤TAX, ¤¤TAXD),
            SLAVES(HCLASSES.SLAVE().name, ¤¤SLAVESD),

            ;

            public readonly CharSequence name;
            public readonly CharSequence desc;

            CTYPE(CharSequence name, CharSequence desc)
            {
                this.name = name;
                this.desc = desc;
            }
        }
    }
}