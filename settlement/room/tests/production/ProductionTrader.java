package settlement.room.tests.production;

import init.resources.RESOURCE;
import util.gui.misc.GBox;
import util.info.GFORMAT;
import util.text.D;

public class ProductionTrader {

	private static CharSequence ¤¤amountPrice = "items/price per item";
	private static CharSequence ¤¤price = "price";
	private static CharSequence ¤¤toll = "toll";
	private static CharSequence ¤¤tariff = "tariff";
	private static CharSequence ¤¤earnings = "earnings";
	static {
		D.ts(ProductionTrader.class);
	}
	
	private final Production prod;
	private int credits;
	
	public ProductionTrader(Production prod, int credits){
		this.prod = prod;
		this.credits = credits;
	}
	
	public int sellPrice(RESOURCE res, ProductionSpec spec, double amount, double tariff, double toll) {
		
		int price = (int) (credits*prod.price(res, spec)*amount);
		int tar = (int) Math.ceil(tariff*price);
		int tt = (int) Math.ceil(toll*credits*amount);
		return price-tar-tt;
	}
	
	public void hoverLegend(GBox b) {
		
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
	
	public void hoverSum(GBox b, int credits) {
		
		b.tab(4);
		
		b.space(180);
		b.add(GFORMAT.iIncr(b.text(), credits));
		b.NL();
	}
	
	public void hoverSale(GBox b, RESOURCE res, ProductionSpec spec, double amount, double tariff, double toll) {
		
		b.add(GFORMAT.f(b.text(), Math.ceil(amount*100)/100.0, 2));
		b.tab(1);
		b.add(res.icon());
		int price = (int) (prod.price(res, spec)*credits);
		b.add(GFORMAT.i(b.text(), price));
		
		b.tab(4);
		price*= amount;
		b.add(GFORMAT.iIncr(b.text(), price));
		b.rewind().space(60);
		int tar = (int) Math.ceil(tariff*price);
		b.add(GFORMAT.iIncr(b.text(), -tar));
		b.rewind().space(60);
		int tt = (int) Math.ceil(toll*credits*amount);
		b.add(GFORMAT.iIncr(b.text(), -tt));
		b.rewind().space(60);
		b.add(GFORMAT.iIncr(b.text(), price-tar-tt));
		b.NL();
	}
	
	public int buyPrice(RESOURCE res, ProductionSpec spec, double amount, double tariff, double toll) {
		
		int price = (int) (credits*prod.price(res, spec)*amount);
		int tar = (int) Math.ceil(tariff*price);
		int tt = (int) Math.ceil(toll*credits*amount);
		
		return price + tar + tt;
	}
	
	public void hoverPuchase(GBox b, RESOURCE res, ProductionSpec spec, double amount, double tariff, double toll) {
		
		b.add(GFORMAT.f(b.text(), -Math.ceil(amount*100)/100.0, 2));
		b.tab(1);
		b.add(res.icon());
		int price = (int) (prod.price(res, spec)*credits);
		b.add(GFORMAT.i(b.text(), price));
		
		b.tab(4);
		price*= amount;
		b.add(GFORMAT.iIncr(b.text(), -price));
		b.rewind().space(60);
		int tar = (int) Math.ceil(tariff*price);
		b.add(GFORMAT.iIncr(b.text(), -tar));
		b.rewind().space(60);
		int tt = (int) Math.ceil(toll*credits*amount);
		b.add(GFORMAT.iIncr(b.text(), -tt));
		b.rewind().space(60);
		b.add(GFORMAT.iIncr(b.text(), -(price+tar+tt)));
		b.NL();
	}
	
}
