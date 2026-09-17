using System;
using System.Collections.Generic;
using game;
using game.faction;
using init.trade;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.colors;
using util.data.GETTER;
using util.data.INT;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.ui.goods;

namespace view.ui.economy
{
    final class RRow : GuiSection
    {
        public static readonly COLOR colorExport = new ColorImp(100, 90, 70);
        public static readonly COLOR colorInport = new ColorImp(80, 80, 100);

        private readonly int w;
        private static int amount = STATS.DAYS_SAVED;
        private static readonly int height = 60;

        private readonly GStaples[] dias;
        private INTE hi;
        private readonly TRADABLE res;
        private readonly GETTERE<TRADABLE> rcurrent;

        private static CharSequence ¤¤Imports = "Imports";
        private static CharSequence ¤¤Exports = "Exports";
        private static CharSequence ¤¤Lowest = "Lowest";
        private static CharSequence ¤¤Highest = "Highest";
        private static CharSequence ¤¤Unit = "Unit";

        static
        {
            D.ts(typeof(RRow));
        }

        RRow(TRADABLE r, INTE hi, GETTERE<TRADABLE> rcurrent, int w, UIGoodsImport im, UIGoodsExport ex)
        {
            this.res = r;
            this.hi = hi;
            this.w = w;
            this.rcurrent = rcurrent;
            dias = new GStaples[]
            {
                new TradeDiagram(r),
                new RRowPriceDia(r, GCOLOR.UI().BAD.hovered, FACTIONS.player().trade.pricesBuy, height),
                new RRowPriceDia(r, GCOLOR.UI().GOOD.hovered, FACTIONS.player().trade.pricesSell, height),
            };

            addRelBody(0, DIR.E, dias[0]);

            addRelBody(12, DIR.E, dias[1]);
            addRelBody(0, DIR.E, UIGoodsImport.miniControl(r, im));

            addRelBody(12, DIR.E, dias[2]);
            addRelBody(0, DIR.E, UIGoodsExport.mini(r, ex));

            addRelBody(8, DIR.W, res.icon().scaled(2));

            pad(2, 6);
        }

        public override bool hover(COORDINATE mCoo)
        {
            bool b = base.hover(mCoo);
            foreach (GStaples ss in dias)
            {
                if (ss.hoveredIs())
                {
                    hi.set(ss.hoverI());
                    rcurrent.set(res);
                }
            }
            if (hi.get() >= 0 && rcurrent.get() == res)
            {
                foreach (GStaples ss in dias)
                {
                    ss.setHovered(hi.get());
                }
            }
            return b;
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            if (hi.get() < 0)
            {
                base.hoverInfoGet(text);
                return;
            }
            if (rcurrent.get() != res)
            {
                base.hoverInfoGet(text);
                return;
            }
            GBox b = (GBox)text;
            b.title(res.names);

            int si = amount - hi.get() - 1;

            {
                GText t = b.text();
                t.lablify();
                DicTime.setAgo(t, si * GAME.player().res().time.bitSeconds());
                b.add(t);
                b.sep();
            }

            {
                b.textLL(Dic.¤¤Stored);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), FACTIONS.player().seller(res).storedHistorically(si)));
                b.NL();
                b.textLL(Dic.¤¤avePrice);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), FACTIONS.player().trade.pricesAve.history(res).get(si)));
                b.NL();
                b.sep();

            }
            {
                b.textLL(¤¤Imports);
                b.NL();
                b.add(b.text().add(Dic.¤¤Price).s().add('(').add(¤¤Lowest).add(')'));
                b.tab(6);
                b.add(GFORMAT.i(b.text(), FACTIONS.player().trade.pricesBuy.history(res).get(si)));
                b.NL();
                b.add(b.text().add(Dic.¤¤Bought));
                b.tab(6);
                b.add(GFORMAT.i(b.text(), FACTIONS.player().trade.unitsImported.history(res).get(si)));
                b.NL();
                b.add(b.text().add(Dic.¤¤Earnings).s().add('/').s().add(¤¤Unit));
                b.tab(6);
                b.add(GFORMAT.iIncr(b.text(), -FACTIONS.player().trade.priceImported.history(res).get(si)));
                b.NL();
                b.add(b.text().add(Dic.¤¤Earnings));
                b.tab(6);
                b.add(GFORMAT.iIncr(b.text(), -FACTIONS.player().trade.outImported.history(res).get(si)));
                b.NL();

                b.NL(8);

                b.textLL(¤¤Exports);
                b.NL();
                b.add(b.text().add(Dic.¤¤Price).s().add('(').add(¤¤Highest).add(')'));
                b.tab(6);
                b.add(GFORMAT.i(b.text(), FACTIONS.player().trade.pricesSell.history(res).get(si)));
                b.NL();
                b.add(b.text().add(Dic.¤¤Sold));
                b.tab(6);
                b.add(GFORMAT.i(b.text(), FACTIONS.player().trade.unitsExported.history(res).get(si)));
                b.NL();
                b.add(b.text().add(Dic.¤¤Earnings).s().add('/').s().add(¤¤Unit));
                b.tab(6);
                b.add(GFORMAT.iIncr(b.text(), FACTIONS.player().trade.priceExported.history(res).get(si)));
                b.NL();
                b.add(b.text().add(Dic.¤¤Earnings));
                b.tab(6);
                b.add(GFORMAT.iIncr(b.text(), FACTIONS.player().trade.inExported.history(res).get(si)));
                b.NL();

                b.sep();
                b.textL(Dic.¤¤Total);
                b.tab(6);
                b.add(GFORMAT.iIncr(b.text(), GAME.player().trade.inExported.history(res).get(si) - GAME.player().trade.outImported.history(res).get(si)));
                b.NL();

            }
        }

        private class TradeDiagram : GStaples
        {
            private readonly TRADABLE res;
            private readonly GStaples.GStapleConfig config;

            public TradeDiagram(TRADABLE res) : base(32, height)
            {
                this.res = res;
                config = new GStaples.GStapleConfig();
                config.barHeight = 10;
                config.barSpacing = 2;
                config.barWidth = 2;
                config.color = GCOLOR.UI().GOOD.normal;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                for (int i = 0; i < amount; i++)
                {
                    int value = (int)FACTIONS.player().trade.pricesSell.history(res).get(i);
                    int max = (int)FACTIONS.player().trade.pricesSell.max(res);
                    float ratio = (float)value / max;
                    config.color = value < 0 ? GCOLOR.UI().BAD.normal : GCOLOR.UI().GOOD.normal;
                    GStaples.render(r, i * (config.barWidth + config.barSpacing), 0, config, ratio);
                }
            }

            public override bool hover(int x, int y)
            {
                // Implement hover logic if needed
                return false;
            }
        }
    }
}