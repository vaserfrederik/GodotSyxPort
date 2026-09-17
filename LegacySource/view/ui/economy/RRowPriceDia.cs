using System;
using game.faction;
using init.resources;
using init.sprite.UI;
using init.trade;
using settlement.main;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sprite;
using util.colors;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.statistics;

namespace view.ui.economy
{
    class RRowPriceDia : GStaples
    {
        private readonly COLOR col;
        private readonly HistoryTradable hres;
        private readonly TRADABLE res;
        private readonly int amount;

        private GStat tbuy = new GStat()
        {
            public override void update(GText text)
            {
                int b = hres.get(res);
                if (res == null)
                    b /= RESOURCES.ALL().size();
                GFORMAT.i(text, b);
            }
        }.bg();

        // private GStat tinc = new GStat()
        // {
        //     public override void update(GText text)
        //     {
        //         double nn = 0;
        //         for (int i = 1; i < 5; i++)
        //             nn += hres.history(res).get(i);
        //         nn /= 4.0;
        //         int b = hres.get(res) - (int)nn;
        //         GFORMAT.iIncr(text, b);
        //     }
        // }.bg();

        public RRowPriceDia(TRADABLE res, COLOR color, HistoryTradable hres, int height) : base(STATS.DAYS_SAVED, false)
        {
            this.amount = STATS.DAYS_SAVED;
            this.col = color;
            this.res = res;
            this.hres = hres;
            body().setWidth(5 * amount).setHeight(height);
            normalize(true);
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
        {
            base.render(r, ds, hoveredIs());
            tbuy.render(r, body().x1() + 4, body().y1() + 4);
            // tinc.render(r, body().x1() + 4, body().y2() - 18);

            COLOR c = GCOLOR.UI().GOOD.hovered;
            SPRITE s = UI.icons().s.chevron(DIR.E);
            double d = SETT.TRADE().buyer(res).tradeValue();

            if (hres == FACTIONS.player().trade.pricesBuy)
            {
                c = GCOLOR.UI().BAD.hovered;
                s = UI.icons().s.chevron(DIR.W);
                d = SETT.TRADE().seller(res).tradeValue();
            }

            int am = (int)Math.Round(d * 5);
            if (am > 8)
                am = 8;
            int x1 = body.x1() + 4;

            if (am > 0)
            {
                OPACITY.O66.bind();
                COLOR.BLACK.render(r, x1 - 1, x1 + 4 + Icon.S + (am - 1) * Icon.S / 2, body.y2() - 2 - Icon.S, body.y2() - 2);
                OPACITY.unbind();

                c.bind();
                for (int i = 0; i < am; i++)
                {
                    s.render(r, x1, body().y2() - 4 - Icon.S);
                    x1 += Icon.S / 2;
                }
                COLOR.unbind();
            }
        }

        protected override double getValue(int stapleI)
        {
            return hres.history(res).get(amount - 1 - stapleI);
        }

        protected override void hover(GBox box, int stapleI)
        {
        }

        protected override void setColor(ColorImp c, int x, double value)
        {
            c.set(col);
        }
    }
}