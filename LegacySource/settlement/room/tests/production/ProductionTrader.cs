using init.resources;
using util.gui.misc;
using util.info;
using util.text;

namespace settlement.room.tests.production
{
    public class ProductionTrader
    {
        private static readonly string ¤¤amountPrice = "items/price per item";
        private static readonly string ¤¤price = "price";
        private static readonly string ¤¤toll = "toll";
        private static readonly string ¤¤tariff = "tariff";
        private static readonly string ¤¤earnings = "earnings";

        static ProductionTrader()
        {
            D.ts(typeof(ProductionTrader));
        }

        private readonly Production prod;
        private int credits;

        public ProductionTrader(Production prod, int credits)
        {
            this.prod = prod;
            this.credits = credits;
        }

        public int SellPrice(RESOURCE res, ProductionSpec spec, double amount, double tariff, double toll)
        {
            int price = (int)(credits * prod.Price(res, spec) * amount);
            int tar = (int)Math.Ceiling(tariff * price);
            int tt = (int)Math.Ceiling(toll * credits * amount);
            return price - tar - tt;
        }

        public void HoverLegend(GBox b)
        {
            b.textLL(¤¤amountPrice);
            b.tab(4);
            b.space(60);
            b.textLL(¤¤tariff);
            b.rewind().space(120);
            b.textLL(¤¤earnings);

            b.NL();
            b.tab(4);
            b.textLL(¤¤price);
            b.rewind().space(120);
            b.textLL(¤¤toll);
            b.NL();
        }

        public void HoverSum(GBox b, int credits)
        {
            b.tab(4);

            b.space(180);
            b.add(GFORMAT.iIncr(b.text(), credits));
            b.NL();
        }

        public void HoverSale(GBox b, RESOURCE res, ProductionSpec spec, double amount, double tariff, double toll)
        {
            b.add(GFORMAT.f(b.text(), Math.Ceiling(amount * 100) / 100.0, 2));
            b.tab(1);
            b.add(res.icon());
            int price = (int)(prod.Price(res, spec) * credits);
            b.add(GFORMAT.i(b.text(), price));

            b.tab(4);
            price *= amount;
            b.add(GFORMAT.iIncr(b.text(), price));
            b.rewind().space(60);
            int tar = (int)Math.Ceiling(tariff * price);
            b.add(GFORMAT.iIncr(b.text(), -tar));
            b.rewind().space(60);
            int tt = (int)Math.Ceiling(toll * credits * amount);
            b.add(GFORMAT.iIncr(b.text(), -tt));
            b.rewind().space(60);
            b.add(GFORMAT.iIncr(b.text(), price - tar - tt));
            b.NL();
        }

        public int BuyPrice(RESOURCE res, ProductionSpec spec, double amount, double tariff, double toll)
        {
            int price = (int)(credits * prod.Price(res, spec) * amount);
            int tar = (int)Math.Ceiling(tariff * price);
            int tt = (int)Math.Ceiling(toll * credits * amount);

            return price + tar + tt;
        }

        public void HoverPurchase(GBox b, RESOURCE res, ProductionSpec spec, double amount, double tariff, double toll)
        {
            b.add(GFORMAT.f(b.text(), -Math.Ceiling(amount * 100) / 100.0, 2));
            b.tab(1);
            b.add(res.icon());
            int price = (int)(prod.Price(res, spec) * credits);
            b.add(GFORMAT.i(b.text(), price));

            b.tab(4);
            price *= amount;
            b.add(GFORMAT.iIncr(b.text(), -price));
            b.rewind().space(60);
            int tar = (int)Math.Ceiling(tariff * price);
            b.add(GFORMAT.iIncr(b.text(), -tar));
            b.rewind().space(60);
            int tt = (int)Math.Ceiling(toll * credits * amount);
            b.add(GFORMAT.iIncr(b.text(), -tt));
            b.rewind().space(60);
            b.add(GFORMAT.iIncr(b.text(), -(price + tar + tt)));
            b.NL();
        }
    }
}