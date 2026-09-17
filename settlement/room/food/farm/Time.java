package settlement.room.food.farm;

import game.time.TIME;
import snake2d.util.MATH;

final class Time {

	public final int days;
	public final int daysPlanting;
	public final int dayPlant;
	public final int dayEvent;
	public final int dayHarvest;
	public final int dayOffWork;
	public final int dayDeath;
	public final int daysWorking;
	public final double daysWorkingI;
	
	Time(ROOM_FARM b){
		days = (int) TIME.years().bitConversion(TIME.days());

		
		daysPlanting = (int) Math.ceil(days*3.0/8.0);
		dayHarvest = (int) Math.round(b.crop.seasonalOffset*days);
		dayOffWork = MATH.mod(dayHarvest+1, days);
		
		dayDeath = MATH.mod(dayHarvest+2, days);
		
		dayPlant = MATH.mod(dayHarvest-daysPlanting, days);
		dayEvent = dayPlant+1;
		daysWorking = days-2;
		daysWorkingI = 1.0/daysWorking;
		
		
		
	}
	
	public double day() {
		return TIME.years().bitPartOf()*days;
	}
	
	public int dayI() {
		return (int) day();
	}
	
	public boolean isHarvest() {
		return dayI() == dayHarvest || dayI() == MATH.mod(dayHarvest+1, days);
	}
	
	public double daysToHarvest() {
		if (isHarvest())
			return 0;
		return MATH.distance(day(), dayHarvest, days);
	}

}
