package settlement.room.industry.module;

import settlement.room.main.RoomInstance;
import snake2d.util.sets.LIST;
import util.gui.misc.GBox;
import util.gui.misc.GText;
import util.info.GFORMAT;

public interface INDUSTRY_HASER {

	public LIST<Industry> industries();


	
	public default double industryFormatProductionRate(GText text, IndustryResource i, RoomInstance ins) {
		double n = IndustryUtil.calcProductionRate(i.rate, ((ROOM_PRODUCER_INSTANCE) ins).industry(), ins);
		n*= ins.employees().employed();
		double nn = i.rate*ins.employees().employed();

		text.add('+');
		GFORMAT.fRel(text, n, nn);
		return n*ins.employees().efficiency();
	}
	
	public default double industryFormatConsumptionRate(GText text, IndustryResource i, RoomInstance ins) {
		
		ROOM_PRODUCER_INSTANCE pp = (ROOM_PRODUCER_INSTANCE) ins;
		double n = IndustryUtil.calcConsumptionRate(i.rate, ins, pp.industry());
		
		n*= ins.employees().employed();
	
		GFORMAT.f0(text, -n);
		return n;
	}
	
	public default double industryFormatProductionRateEmpl(GText text, IndustryResource i, RoomInstance ins) {
		double n = IndustryUtil.calcProductionRate(i.rate, ((ROOM_PRODUCER_INSTANCE) ins).industry(), ins);
		GFORMAT.fRel(text, n*ins.employees().totEfficiency(), i.rate);
		return n*ins.employees().efficiency();
	}
	
	public default void industryHoverProductionRate(GBox b, IndustryResource i, RoomInstance ins) {
		IndustryUtil.hoverProductionRate(b, i.rate, ((ROOM_PRODUCER_INSTANCE) ins).industry(), ins);
	}
	
	public default void industryHoverConsumptionRate(GBox b, IndustryResource i, RoomInstance ins) {
		
		IndustryUtil.hoverConsumptionRate(b, i.rate, ins, ((ROOM_PRODUCER_INSTANCE) ins).industry());
	}
	
	public default boolean industryIgnoreUI() {
		return false;
	}
}
