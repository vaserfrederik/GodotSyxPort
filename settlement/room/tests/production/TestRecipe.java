package settlement.room.tests.production;

import init.resources.RESOURCE;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LIST;

public class TestRecipe {
	
	ArrayListGrower<Input> inputs = new ArrayListGrower<Input>();
	public final Industry ins;
	public final RESOURCE res;
	public final double rate;
	public final double wPerItem;
	public final int index;
	
	TestRecipe(int index, Industry ins, IndustryResource out){
		this.ins = ins;
		res = out.resource;
		rate = out.rate;
		wPerItem = 1.0/out.rate;
		this.index = index;
	}
	
	TestRecipe(int index, TestRecipe r){
		this.ins = r.ins;
		res = r.res;
		wPerItem = r.wPerItem;
		rate = r.rate;
		this.index = index;
	}
	
	public LIST<Input> inputs(){
		return inputs;
	}
	
	public double wPerItem(ProductionSpec ibonuses) {
		return (wPerItem+ibonuses.wPerItemUsed())/ibonuses.bonus(ins);
	}
	
	public double wTotPerItem(ProductionSpec ibonuses) {
		double w = wPerItem(ibonuses);
		for (int i = 0; i < inputs.size(); i++) {
			Input ii = inputs.get(i);
			w += (ii.amount*ii.producer.wTotPerItem(ibonuses)/(rate*ibonuses.consumptionBonus(ins)));
		}
		return w;
	}
	
	public double amountPerW(ProductionSpec ibonuses) {
		return 1.0/wPerItem(ibonuses);
	}
	
	public double amountPerWTot(ProductionSpec ibonuses) {
		return 1.0/wTotPerItem(ibonuses);
	}
	
	public double pricePerItem(ProductionSpec ibonuses) {	
		double p = wPerItem(ibonuses);
		for (int i = 0; i < inputs.size(); i++) {
			Input ii = inputs.get(i);
			p += ii.producer.pricePerItem(ibonuses)*ii.amount/(rate*ibonuses.consumptionBonus(ins));
		}
		return p + ibonuses.addedW();
	}
}

