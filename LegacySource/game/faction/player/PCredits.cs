using System;
using System.Collections.Generic;
using game.VERSION;
using game.faction.FACTIONS;
using game.faction.FCredits;
using game.faction.Faction;
using game.time.TIME;
using game.time.TIMECYCLE;
using init.sprite.UI.UI;
using init.trade.TRADABLE;
using init.value.GVALUES;
using snake2d.util.file.FileGetter;
using snake2d.util.file.FilePutter;
using snake2d.util.file.SAVABLE;
using snake2d.util.misc.ACTION;
using snake2d.util.sets.ArrayListGrower;
using snake2d.util.sets.LIST;
using util.data.INT_O;
using util.statistics.HistoryInt;
using util.text.D;
using view.sett.IDebugPanelSett;
using view.ui.economy.YearlyFinansials;

namespace game.faction.player
{
    public sealed class PCredits : FCredits
    {
        static PCredits()
        {
            if (false)
            {
                //set to STATS.DAYS_SAVED or change that to 48
            }
        }

        public static int history = 48;
        private readonly ArrayListGrower<CredHistory> all = new ArrayListGrower<CredHistory>();
        public readonly HistoryInt worth = new HistoryInt(16 * 4, TIME.seasons(), true);
        public readonly Yearly yearly = new Yearly();
        private int dayToSendMessage = (int)(TIME.years().bitsSinceStart() * TIME.years().bitConversion(TIME.days()) + TIME.years().bitConversion(TIME.days()) + 2);

        private static readonly string ¤¤turnover = "Yearly Turnover";
        private static readonly string ¤¤profits = "Yearly Profits";
        private static readonly string ¤¤losses = "Yearly losses";

        static PCredits()
        {
            D.ts(typeof(PCredits));
        }

        public PCredits() : base(history, TIME.days())
        {
            GVALUES.FACTION.pushI("CREDITS_YEARLY_TURNOVER", ¤¤turnover, UI.icons().s.money, new VV(yearly.TURNOVER));
            GVALUES.FACTION.pushI("CREDITS_YEARLY_PROFITS", ¤¤profits, UI.icons().s.money, new VV(yearly.PROFITS));
            GVALUES.FACTION.pushI("CREDITS_YEARLY_LOSSES", ¤¤losses, UI.icons().s.money, new VV(yearly.LOSSES));

            foreach (CTYPE t in Enum.GetValues(typeof(CTYPE)))
            {
                CredHistory h = new CredHistory(t, 48, TIME.days());
                all.add(h);
                GVALUES.FACTION.pushI("CREDITS_YEARLY_TURNOVER_" + t.ToString(), ¤¤turnover + " (" + t + ")", UI.icons().s.money, new VV(h.yearly.TURNOVER));
                GVALUES.FACTION.pushI("CREDITS_YEARLY_PROFITS_" + t.ToString(), ¤¤profits + " (" + t + ")", UI.icons().s.money, new VV(h.yearly.PROFITS));
                GVALUES.FACTION.pushI("CREDITS_YEARLY_LOSSES_" + t.ToString(), ¤¤losses + " (" + t + ")", UI.icons().s.money, new VV(h.yearly.LOSSES));
            }

            IDebugPanelSett.add("YEARLY FINANSIALS", new ACTION
            {
                exe = () =>
                {
                    new YearlyFinansials().send();
                }
            });
        }

        private class VV : INT_O<Faction>
        {
            private readonly HistoryInt ii;

            public VV(HistoryInt ii)
            {
                this.ii = ii;
            }

            public int get(Faction t)
            {
                if (t == FACTIONS.player())
                    return ii.get();
                return 0;
            }

            public int min(Faction t)
            {
                return 0;
            }

            public int max(Faction t)
            {
                return int.MaxValue;
            }
        }

        protected override void update(double ds, Faction f)
        {
            worth.set((int)FACTIONS.WORTH().faction(FACTIONS.player()));
            base.update(ds, f);

            if (TIME.days().bitsSinceStart() >= dayToSendMessage)
            {
                dayToSendMessage = (int)(TIME.years().bitsSinceStart() * TIME.years().bitConversion(TIME.days()) + TIME.years().bitConversion(TIME.days()) + 2);
                new YearlyFinansials().send();
            }
        }

        public override void inc(double amount, CTYPE t)
        {
            if (amount < 0)
                all.get(t).OUT.inc((int)-amount);
            else
                all.get(t).IN.inc((int)amount);
        }

        public override void inc(double amount, CTYPE t, TRADABLE res, int resAm)
        {
            inc(amount, t);
            if (t == CTYPE.TRADE)
            {
                FACTIONS.player().trade.trade(amount, res, resAm);
            }
        }

        protected override void save(FilePutter file)
        {
            file.i(all.size());
            foreach (CredHistory h in all)
                h.saver.save(file);
            worth.save(file);
            yearly.save(file);
            file.i(dayToSendMessage);
            base.save(file);
        }

        protected override void load(FileGetter file)
        {
            int l = file.i();
            if (l != all.size())
            {
                for (int i = 0; i < l; i++)
                    all.get(0).saver.load(file);
                clear();
            }
            else
            {
                foreach (CredHistory h in all)
                    h.saver.load(file);
            }
            worth.load(file);
            yearly.load(file);
            if (!VERSION.versionIsBefore(71, 2))
                dayToSendMessage = file.i();
            base.load(file);
        }

        protected override void clear()
        {
            foreach (CredHistory h in all)
                h.saver.clear();
            worth.clear();
            yearly.clear();
            base.clear();
        }

        public LIST<CredHistory> all()
        {
            return all;
        }

        public CredHistory get(CTYPE type)
        {
            return all.get(type);
        }

        public class CredHistory
        {
            public readonly CTYPE type;
            public readonly HistoryInt IN;
            public readonly HistoryInt OUT;
            public readonly Yearly yearly = new Yearly();

            public CredHistory(CTYPE type, int saved, TIMECYCLE time)
            {
                IN = new HistoryInt(saved, time, false)
                {
                    protected override void change(int old, int current)
                    {
                        inc(current - old);
                        yearly.PROFITS.inc(current - old);
                        yearly.TURNOVER.inc(current - old);
                        PCredits.this.yearly.PROFITS.inc(current - old);
                        PCredits.this.yearly.TURNOVER.inc(current - old);
                    }
                };
                OUT = new HistoryInt(saved, time, false)
                {
                    protected override void change(int old, int current)
                    {
                        inc(-(current - old));
                        yearly.LOSSES.inc(-(current - old));
                        yearly.TURNOVER.inc(-(current - old));
                        PCredits.this.yearly.PROFITS.inc(-(current - old));
                        PCredits.this.yearly.TURNOVER.inc(-(current - old));
                    }
                };

                this.type = type;
            }

            public readonly SAVABLE saver = new SAVABLE()
            {
                public void save(FilePutter file)
                {
                    IN.save(file);
                    OUT.save(file);
                    yearly.save(file);
                }

                public void load(FileGetter file)
                {
                    IN.load(file);
                    OUT.load(file);
                    yearly.load(file);
                }

                public void clear()
                {
                    IN.clear();
                    OUT.clear();
                    yearly.clear();
                }
            };
        }

        public readonly class Yearly
        {
            public readonly HistoryInt TURNOVER = new HistoryInt(64, TIME.years(), false);
            public readonly HistoryInt PROFITS = new HistoryInt(64, TIME.years(), false);
            public readonly HistoryInt LOSSES = new HistoryInt(64, TIME.years(), false);

            public void save(FilePutter file)
            {
                TURNOVER.save(file);
                PROFITS.save(file);
                LOSSES.save(file);
            }

            public void load(FileGetter file)
            {
                TURNOVER.load(file);
                PROFITS.load(file);
                LOSSES.load(file);
            }

            public void clear()
            {
                TURNOVER.clear();
                PROFITS.clear();
                LOSSES.clear();
            }
        }
    }
}