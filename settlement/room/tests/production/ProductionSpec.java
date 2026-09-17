package settlement.room.tests.production;

import settlement.room.industry.module.Industry;

public interface ProductionSpec {

	public double bonus(Industry ins);
	public double consumptionBonus(Industry ins);
	public double wPerItemUsed();
	public double addedW();

}
